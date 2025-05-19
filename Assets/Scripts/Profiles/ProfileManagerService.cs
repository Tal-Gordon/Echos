using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using static DataManager;

public class ProfileManagerService : MonoBehaviour
{
    private static readonly string appDataPath = Application.persistentDataPath;
    private static readonly string defaultProfileID = "default";
    private static readonly string userData = "UserData";
    private static readonly string jsonExtension = ".json";
    private static readonly string profilesInfoFilePath = Path.Combine(appDataPath, userData, "ProfilesInfo.json");
    private static readonly DataCategory userCat = DataCategory.User;
    // Class for displaying profiles in UI
    public class ProfileInfo
    {
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ProfilePassword { get; set; }
        // TODO: pictures
    }

    public static void InitializeProfileManager(string initialProfileID)
    {
        if (string.IsNullOrEmpty(initialProfileID))
        {
            Debug.LogError("No profile provided for initialization");
            return;
        }
        else
        {
            SwitchProfile(initialProfileID);
        }

        // Ensure default profile directory and files exist
        string defaultProfileDataPath = GetProfileDirectory(defaultProfileID);
        if (!Directory.Exists(defaultProfileDataPath))
        {
            Debug.LogError("Default profile folder was not found!!!");
        }
        foreach (DataCategory cat in Enum.GetValues(typeof(DataCategory)))
        {
            EnsureFileExists(Path.Combine(defaultProfileDataPath, cat.ToString() + jsonExtension));
        }
    }

    public static async Awaitable<string> CreateNewProfileAsync(string profileName)
    {
        // We skip data validation checks, they are performed before

        string newProfileID = Utils.Get8CharacterRandomString();
        SwitchProfile(newProfileID);
        await CopyDefaultProfileDataAsync(newProfileID);

        await UpdateProfilesInfoFileAsync(profileName, newProfileID);
        return newProfileID;
    }

    public static async Awaitable<bool> DeleteProfileAsync(string profileIDToDelete)
    {
        if (string.IsNullOrEmpty(profileIDToDelete))
        {
            Debug.LogError("Profile ID to delete cannot be null or empty.");
            return false;
        }

        if (profileIDToDelete == defaultProfileID)
        {
            Debug.LogError("Cannot delete the default profile.");
            return false;
        }

        string profileDataPathToDelete = GetProfileDirectory(profileIDToDelete);

        if (!Directory.Exists(profileDataPathToDelete))
        {
            Debug.LogError($"Profile directory not found for ID: {profileIDToDelete}. Path: {profileDataPathToDelete}");
        }

        await FileAccessSemaphore.WaitAsync();
        try
        {
            try
            {
                Directory.Delete(profileDataPathToDelete, true); // 'true' for recursive delete
            }
            catch (IOException e)
            {
                Debug.LogError($"Error deleting profile directory for ID: {profileIDToDelete}. Path: {profileDataPathToDelete}. Error: {e.Message}");
                return false;
            }

            // Remove profile info from ProfilesInfo.json
            Dictionary<string, string> profilesInfo = await LoadProfilesInfoAsync();
            string profileNameToRemove = null;

            foreach (var pair in profilesInfo)
            {
                if (pair.Value == profileIDToDelete)
                {
                    profileNameToRemove = pair.Key;
                    break;
                }
            }

            if (profileNameToRemove != null)
            {
                profilesInfo.Remove(profileNameToRemove);
                await SaveProfilesInfoAsync(profilesInfo);
            }
            else
            {
                Debug.LogWarning($"Profile ID: {profileIDToDelete} not found in ProfilesInfo.json, but directory was deleted. ProfilesInfo.json might be inconsistent.");
            }

            Debug.Log($"Profile deleted successfully");
            return true;

        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static IEnumerator LoadProfile(string profileID)
    {
        InitializeProfileManager(profileID);

        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync("Echos", LoadSceneMode.Single);
        loadSceneAsync.allowSceneActivation = false;

        float fakeLoadingDuration = 2f;
        float timer = 0f;

        while (timer < fakeLoadingDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        loadSceneAsync.allowSceneActivation = true;
    }

    private static async Awaitable UpdateProfilesInfoFileAsync(string profileName, string profileID)
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            Dictionary<string, string> profilesInfo = await LoadProfilesInfoAsync();
            profilesInfo[profileName] = profileID; // Add/Update profile info
            await SaveProfilesInfoAsync(profilesInfo);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    private static async Awaitable<Dictionary<string, string>> LoadProfilesInfoAsync()
    {
        if (!File.Exists(profilesInfoFilePath))
        {
            Debug.LogError($"ProfilesInfo path not found. Returning empty profile info.");
            File.Create(profilesInfoFilePath);
            return new Dictionary<string, string>();
        }

        try
        {
            string content = await ReadFileAsync(profilesInfoFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();
        }
        catch (IOException e)
        {
            Debug.LogError($"Error loading ProfilesInfo.json: {e.Message}. Returning empty profile info.");
            return new Dictionary<string, string>(); // Return empty on error
        }
    }

    private static async Awaitable SaveProfilesInfoAsync(Dictionary<string, string> profilesInfo)
    {
        try
        {
            string jsonContent = JsonConvert.SerializeObject(profilesInfo, Formatting.Indented);
            await WriteFileAsync(profilesInfoFilePath, jsonContent);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error saving ProfilesInfo.json: {e.Message}");
        }
    }

    public static async Awaitable<List<ProfileInfo>> GetAllProfilesInfo()
    {
        Dictionary<string, string> profilesInfoMap = await LoadProfilesInfoAsync();
        List<ProfileInfo> allProfilesInfo = new();

        foreach (var profileEntry in profilesInfoMap)
        {
            string profileName = profileEntry.Key;
            string profileId = profileEntry.Value;
            string profilePassword = await ReadDataAsync<string>(profileId, userCat, "password");
            profilePassword ??= "";

            // TODO: load picture

            allProfilesInfo.Add(new ProfileInfo()
            {
                ProfileId = profileId,
                ProfileName = profileName,
                ProfilePassword = profilePassword,
            });
        }

        return allProfilesInfo;
    }

    public static async Awaitable<string> GetProfileIdByName(string profileName)
    {
        Dictionary<string, string> profilesInfo = await LoadProfilesInfoAsync();
        foreach (var res in profilesInfo)
        {
            Console.WriteLine("Profile with name {0}: ID = {1}", res.Key, res.Value);
        }
        if (profilesInfo.TryGetValue(profileName, out string foundProfileName))
        {
            return foundProfileName;
        }
        return null;
    }

    private static async Awaitable CopyDefaultProfileDataAsync(string newProfileID)
    {
        string defaultProfileDataPath = GetProfileDirectory(defaultProfileID);
        string newProfileDataPath = GetProfileDirectory(newProfileID);

        foreach (DataCategory cat in Enum.GetValues(typeof(DataCategory)))
        {
            string defaultFilePath = Path.Combine(defaultProfileDataPath, cat.ToString() + jsonExtension);
            string newFilePath = Path.Combine(newProfileDataPath, cat.ToString() + jsonExtension);

            try
            {
                string defaultFileContent = await ReadFileAsync(defaultFilePath);
                await WriteFileAsync(newFilePath, defaultFileContent);
            }
            catch (IOException e)
            {
                Debug.LogError($"Error copying default profile data for category {cat}: {e.Message}");
                await WriteFileAsync(newFilePath, "{}");
            }
        }
    }

    private static void SwitchProfile(string profileIdToSwitch)
    {
        ProfileID = profileIdToSwitch;
        InitializeProfileDirectory();
    }
}
