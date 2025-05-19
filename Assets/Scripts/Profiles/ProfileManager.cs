using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject existingProfilesPage;
    public GameObject profileCreationWizard;
    public GameObject loadingPage;

    [Header("Existing Profiles Page Elements")]
    public Transform existingProfilesContainer;
    public GameObject profileButtonPrefab;

    [Space(10)]
    public GameObject deleteProfileButton;

    private List<ProfileManagerService.ProfileInfo> existingProfileInfos = new();
    private bool isDeleteMode = false;

    public bool IsDeleteMode { get => isDeleteMode; private set => isDeleteMode = value; }

    void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        existingProfileInfos = await ProfileManagerService.GetAllProfilesInfo();
        if (existingProfileInfos.Count != 0)
        {
            await UpdateProfileChooser();
        }
        else
        {
            profileCreationWizard.SetActive(true);
            // TODO
        }
    }

    public void CreateNewProfile()
    {
        profileCreationWizard.SetActive(true);
        existingProfilesPage.SetActive(false);
    }

    public void DeleteProfile()
    {
        IsDeleteMode = !IsDeleteMode;
        UpdateDeleteModeVisuals();
    }

    void UpdateDeleteModeVisuals()
    {
        TextMeshProUGUI tmpro = deleteProfileButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        tmpro.text = IsDeleteMode ? "Cancel" : "Delete profile";
        // Maybe subtly highlight profile buttons or change their cursor in delete mode
    }

    public async Awaitable UpdateProfileChooser()
    {
        existingProfilesPage.SetActive(true);
        existingProfileInfos = await ProfileManagerService.GetAllProfilesInfo();
        foreach (Transform child in existingProfilesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (ProfileManagerService.ProfileInfo profileInfo in existingProfileInfos)
        {
            GameObject profileButton = Instantiate(profileButtonPrefab, existingProfilesContainer.transform);
            ProfileSelectButtonController psbc = profileButton.GetComponent<ProfileSelectButtonController>();
            psbc.SetProfileManager(this);
            psbc.SetProfileName(profileInfo.ProfileName);
            psbc.ProfileID = profileInfo.ProfileId;
            //psbc.SetProfilePicture()
            // TODO
        }
    }

    public void LoadProfile(string profileID)
    {
        profileCreationWizard.SetActive(false);
        existingProfilesPage.SetActive(false);
        loadingPage.SetActive(true);
        StartCoroutine(ProfileManagerService.LoadProfile(profileID));
    }
}
