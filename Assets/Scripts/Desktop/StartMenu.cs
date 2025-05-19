using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public SceneAsset profileManager;

    private GameObject startMenu;
    private GameObject searchBar;
    private GameObject apps;
    private GameObject profileImage;
    void Start()
    {
        startMenu = transform.Find("Background").gameObject;
        searchBar = transform.Find("Background").Find("Search Bar").gameObject;
        apps = transform.Find("Background").Find("AppsScrollRect").gameObject;
        profileImage = transform.Find("Background").Find("Profile Image").gameObject;
    }

    void Update()
    {
        
    }
    public void SetProfileImage(Sprite image)
    {
        profileImage.GetComponent<Image>().sprite = image;
    }

    public void PowerDown()
    {
        Application.Quit();
    }

    public void LogOff()
    {
        SceneManager.LoadScene(profileManager.name, LoadSceneMode.Single);
    }
}
