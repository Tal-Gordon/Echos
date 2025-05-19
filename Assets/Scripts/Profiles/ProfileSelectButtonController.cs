using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileSelectButtonController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI profileName;
    [SerializeField] private Image profilePicture;
    private Button buttonComponent;
    private string profileID;

    public string ProfileID { get => profileID; set => profileID = value; }

    private ProfileManager profileManager;

    void Awake()
    {
        if (buttonComponent == null)
        {
            buttonComponent = GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError("Button component not found on ProfileSelectButtonController!");
            }
            buttonComponent.onClick.AddListener(async () => await OnButtonClicked());
        }
    }
    public void SetProfileManager(ProfileManager manager)
    {
        profileManager = manager;
        if (profileManager == null)
        {
            Debug.LogError("ProfileManager reference is null in ProfileSelectButtonController!");
        }
    }

    public void SetProfileName(string textValue)
    {
        if (profileName != null)
        {
            profileName.text = textValue;
        }
        else
        {
            Debug.LogError("Button Text component not assigned in ProfileSelectButtonController!");
        }
    }

    public void SetProfilePicture(Sprite newImage)
    {
        if (profilePicture != null)
        {
            profilePicture.sprite = newImage;
        }
        else
        {
            Debug.LogError("Button Image component not assigned in ProfileSelectButtonController!");
        }
    }

    private async Awaitable OnButtonClicked()
    {
        if (profileManager != null)
        {
            if (profileManager.IsDeleteMode)
            {
                // TODO: deletion dialog confirmation
                if (await ProfileManagerService.DeleteProfileAsync(profileID))
                {
                    await profileManager.UpdateProfileChooser();
                }
            }
            else
            {
                profileManager.LoadProfile(profileID);
            }
        }
        else
        {
            Debug.LogError("ProfileManager is not assigned in ButtonPrefabController. Cannot load profile.");
        }
    }
}