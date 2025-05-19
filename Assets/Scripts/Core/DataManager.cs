using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

public static class DataManager
{
    public enum DataCategory
    {
        User,
        System,
        Apps,
        Widgets,
        Network
    }

    private static string profileID;
    private static readonly string appDataPath = Application.persistentDataPath;
    private static readonly string defaultProfileID = "default";
    private static readonly string userData = "UserData";
    private static readonly string jsonExtension = ".json";
    private static string dataPath;

    private static readonly SemaphoreSlim fileAccessSemaphore = new(1);
    public static string ProfileID
    {
        get => profileID;
        set
        {
            profileID = value;
            InitializeProfileDirectory();
        }
    }

    public static SemaphoreSlim FileAccessSemaphore => fileAccessSemaphore;

    public static void InitializeProfileDirectory()
    {
        dataPath = Path.Combine(appDataPath, userData, ProfileID);
        Directory.CreateDirectory(dataPath);
    }

    public static string GetProfileDirectory(string profileID)
    {
        return Path.Combine(appDataPath, userData, profileID);
    }

    private static string GetDataPath(DataCategory cat, string currentProfileID)
    {
        string profileDataPath = GetProfileDirectory(currentProfileID);
        return Path.Combine(profileDataPath, cat.ToString() + jsonExtension);
    }

    private static string GetDataPath(DataCategory cat) //Overload for current profile
    {
        return GetDataPath(cat, ProfileID);
    }

    public static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "{}");
        }
    }

    public static async Awaitable<string> ReadFileAsync(string path)
    {
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error reading file {path}: {e.Message}");
            return "{}";
        }
    }

    public static async Awaitable WriteFileAsync(string path, string content)
    {
        try
        {
            await File.WriteAllTextAsync(path, content);
        }
        catch (IOException e)
        {
            Debug.LogError($"Error writing to file {path}: {e.Message}");
        }
    }

    private static async Awaitable<Dictionary<string, object>> LoadDataAtPathAsync(string filePath)
    {
        EnsureFileExists(filePath);
        string fileContent = await ReadFileAsync(filePath);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(fileContent) ?? new Dictionary<string, object>();
    }

    private static async Awaitable SaveDataAtPathAsync(string filePath, Dictionary<string, object> data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        await WriteFileAsync(filePath, json);
    }

    private static async Awaitable<Dictionary<string, object>> LoadDataAsync(DataCategory cat, string currentProfileID)
    {
        string filePath = GetDataPath(cat, currentProfileID);
        return await LoadDataAtPathAsync(filePath);
    }

    private static async Awaitable<Dictionary<string, object>> LoadDataAsync(DataCategory cat) //Overload for current profile
    {
        return await LoadDataAsync(cat, ProfileID);
    }

    private static async Awaitable SaveDataAsync(DataCategory cat, Dictionary<string, object> data, string currentProfileID)
    {
        string filePath = GetDataPath(cat, currentProfileID);
        await SaveDataAtPathAsync(filePath, data);
    }

    private static async Awaitable SaveDataAsync(DataCategory cat, Dictionary<string, object> data) //Overload for current profile
    {
        await SaveDataAsync(cat, data, ProfileID);
    }

    public static async Awaitable WriteDataAsync(DataCategory cat, string key, object value) // Uses current profile
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            Dictionary<string, object> data = await LoadDataAsync(cat);
            data[key] = value;
            await SaveDataAsync(cat, data);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable WriteDataAsync(string profileID, DataCategory cat, string key, object value) // Specify profile
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            Dictionary<string, object> data = await LoadDataAsync(cat, profileID);
            data[key] = value;
            await SaveDataAsync(cat, data, profileID);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    private static async Awaitable<T> ReadDataFromProfileAsync<T>(string profileID, DataCategory cat, string key, bool useDefaultFallback = false)
    {
        Dictionary<string, object> data = await LoadDataAsync(cat, profileID);
        if (data.ContainsKey(key))
        {
            return ConvertValue<T>(key, data[key]);
        }

        if (useDefaultFallback && profileID != defaultProfileID)
        {
            return await ReadDataFromDefaultAsync<T>(cat, key);
        }

        Debug.LogWarning($"Could not find {key} within {cat} in profile: {profileID}{(useDefaultFallback ? " or default profile." : ".")}");
        return default;
    }

    public static async Awaitable<T> ReadDataAsync<T>(DataCategory cat, string key) // Uses current profile and default
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ReadDataFromProfileAsync<T>(ProfileID, cat, key, true);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable<T> ReadDataAsync<T>(string profileID, DataCategory cat, string key) // Specify profile, no default
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ReadDataFromProfileAsync<T>(profileID, cat, key, false);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable<T> ReadDataOrDefaultAsync<T>(string profileID, DataCategory cat, string key) // Specify profile, with default fallback
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ReadDataFromProfileAsync<T>(profileID, cat, key, true);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    private static T ConvertValue<T>(string key, object value)
    {
        if (value is T typedValue)
        {
            return typedValue;
        }
        else
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                Debug.LogError($"Failed to cast value for {key} to type {typeof(T)}.");
                return default;
            }
        }
    }

    private static async Awaitable<bool> ContainsJsonKeyInProfileAsync(string profileID, DataCategory cat, string key, bool useDefaultFallback = false)
    {
        Dictionary<string, object> data = await LoadDataAsync(cat, profileID);
        if (data.ContainsKey(key))
        {
            return true;
        }

        if (useDefaultFallback && profileID != defaultProfileID)
        {
            return await ContainsJsonKeyInDefaultAsync(cat, key);
        }
        return false;
    }

    public static async Awaitable<bool> ContainsJsonKeyAsync(DataCategory cat, string key) // Uses current profile and default
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ContainsJsonKeyInProfileAsync(ProfileID, cat, key, true);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable<bool> ContainsJsonKeyAsync(string profileID, DataCategory cat, string key) // Specify profile, no default
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ContainsJsonKeyInProfileAsync(profileID, cat, key, false);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable<bool> ContainsJsonKeyOrDefaultAsync(string profileID, DataCategory cat, string key) // Specify profile, with default fallback
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            return await ContainsJsonKeyInProfileAsync(profileID, cat, key, true);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable DeleteDataAsync(DataCategory cat, string key) // Uses current profile
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            Dictionary<string, object> data = await LoadDataAsync(cat);
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                await SaveDataAsync(cat, data);
            }
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable DeleteDataAsync(string profileID, DataCategory cat, string key) // Specify profile
    {
        await FileAccessSemaphore.WaitAsync();
        try
        {
            Dictionary<string, object> data = await LoadDataAsync(cat, profileID);
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                await SaveDataAsync(cat, data, profileID);
            }
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }

    public static async Awaitable DeleteDataCategoryAsync(DataCategory cat) // Uses current profile
    {
        string filePath = GetDataPath(cat);
        await WriteFileAsync(filePath, "{}");
    }

    public static async Awaitable DeleteDataCategoryAsync(string profileID, DataCategory cat) // Specify profile
    {
        string filePath = GetDataPath(cat, profileID);
        await WriteFileAsync(filePath, "{}");
    }

    private static async Awaitable<T> ReadDataFromDefaultAsync<T>(DataCategory cat, string key)
    {
        string defaultFilePath = GetDataPath(cat, defaultProfileID);
        return await ReadDataAtPathAsync<T>(defaultFilePath, key);
    }

    private static async Awaitable<T> ReadDataAtPathAsync<T>(string filePath, string key)
    {
        Dictionary<string, object> defaultData = await LoadDataAtPathAsync(filePath);
        if (defaultData.ContainsKey(key))
        {
            return ConvertValue<T>(key, defaultData[key]);
        }
        return default;
    }

    private static async Awaitable<bool> ContainsJsonKeyInDefaultAsync(DataCategory cat, string key)
    {
        string defaultFilePath = GetDataPath(cat, defaultProfileID);
        Dictionary<string, object> defaultData = await LoadDataAtPathAsync(defaultFilePath);
        return defaultData.ContainsKey(key);
    }
}