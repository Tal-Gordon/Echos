using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileCreationWizard : MonoBehaviour
{
    [Header("Pages")]
    public List<GameObject> wizardPages;

    [Header("Navigation Buttons")]
    public Button backButton;
    public Button nextButton;
    private TextMeshProUGUI nextButtonText;

    [Header("Info Page Elements")]
    public TMP_InputField nameInputField;
    public Button selectPictureButton;

    [Header("Password Page Elements")]
    public TMP_InputField passwordInputField;
    public TMP_InputField passwordRepeatInputField;

    private WizardPageNavigator wizardPageNavigator;
    private DataManager.DataCategory userCat;
    private ProfileManager profileManager;
    void Start()
    {
        InitializeCreationWizard();
    }

    public void InitializeCreationWizard()
    {
        nextButtonText = nextButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        userCat = DataManager.DataCategory.User;
        profileManager = FindAnyObjectByType<ProfileManager>();

        wizardPageNavigator = new(wizardPages);
        wizardPageNavigator.Initialize();
    }

    public void BackButton()
    {
        wizardPageNavigator.PreviousPage();
        UpdateNavigationButtons();
    }

    public void NextButton()
    {
        if (wizardPageNavigator.GetCurrentPageIndex() == wizardPages.Count - 1)
        {
            CreateProfile();
        }
        wizardPageNavigator.NextPage();
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        backButton.interactable = wizardPageNavigator.GetCurrentPageIndex() > 0;
        if (wizardPageNavigator.GetCurrentPageIndex() >= wizardPages.Count - 1)
        {
            nextButtonText.text = "Finish";
        }
        else
        {
            nextButtonText.text = "Next";
        }
    }

    private string GetProfileName()
    {
        return nameInputField.text;
    }

    private string GetPassword()
    {
        return passwordInputField.text;
    }

    private async void CreateProfile()
    {
        string profileName = GetProfileName();
        string password = GetPassword();

        string profileID = await ProfileManagerService.CreateNewProfileAsync(profileName);
        if (!string.IsNullOrEmpty(password))
        {
            await DataManager.WriteDataAsync(userCat, "password", password);
        }

        profileManager.LoadProfile(profileID);
    }

    private static Color GetDarkerShade(Color baseColor, double darkeningFactor)
    {
        Math.Clamp(darkeningFactor, 0, 1);

        // Calculate the darkened RGB components
        // We subtract from 255 to reverse the direction of darkeningFactor
        // When darkeningFactor is 0, we subtract 0 (no change)
        // When darkeningFactor is 1, we subtract the max, effectively making it black

        int red = Math.Clamp((int)(baseColor.r * (1 - darkeningFactor)), 0, 255);
        int green = Math.Clamp((int)(baseColor.g * (1 - darkeningFactor)), 0, 255);
        int blue = Math.Clamp((int)(baseColor.b * (1 - darkeningFactor)), 0, 255);

        return new Color(baseColor.a, red, green, blue);
    }
}