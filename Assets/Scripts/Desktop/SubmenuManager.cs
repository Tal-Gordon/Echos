using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubmenuManager : MonoBehaviour
{
    public GameObject menuPanel;

    public bool toggleCoroutineRunning = false;

    private SubmenuMenuItem mMenuItem;
    private RectTransform rectTransform;
    private RectTransform menuPanelRT;
    private BoxCollider2D menuPanelBC2D;

    public void Initialize(SubmenuMenuItem submenu)
    {
        menuPanel = Resources.Load<GameObject>("Prefabs/Panel");
        rectTransform = GetComponent<RectTransform>();
        menuPanelBC2D = Utils.GetOrAddComponent<BoxCollider2D>(menuPanel);
        menuPanelRT = menuPanel.GetComponent<RectTransform>();

        menuPanel.transform.localScale = Vector3.one;

        Utils.GetOrAddComponent<BoxCollider2D>(gameObject).size = menuPanelRT.sizeDelta;
        Utils.GetOrAddComponent<Rigidbody2D>(gameObject).bodyType = RigidbodyType2D.Static;

        mMenuItem = submenu;
        menuPanel.name = mMenuItem.Title + " submenu";
        menuPanelBC2D.size = menuPanelRT.sizeDelta + new Vector2(0.04f, 0);
        menuPanelBC2D.offset = new Vector2(menuPanelRT.sizeDelta.x / 2, -menuPanelRT.sizeDelta.y / 2);

        menuPanel = Instantiate(menuPanel, gameObject.transform);
        menuPanelBC2D = menuPanel.GetComponent<BoxCollider2D>();
        menuPanelRT = menuPanel.GetComponent<RectTransform>();
    }
    public void PublicToggleSubmenu(bool isActive)
    {
        StartCoroutine(ToggleSubmenu(isActive));
    }
    private IEnumerator ToggleSubmenu(bool isActive)
    {
        if (mMenuItem.isSubmenuVisible == isActive || toggleCoroutineRunning) { yield return null; }
        toggleCoroutineRunning = true;
        // Close all submenus down the hierarchy
        if (isActive == false)
        {
            foreach (SubmenuManager submenuManager in GetComponentsInChildren<SubmenuManager>(false))
            {
                if (submenuManager != this)
                {
                    StartCoroutine(submenuManager.ToggleSubmenu(isActive));
                }
            }
        }
        yield return new WaitForSeconds(0.2f);

        menuPanel.SetActive(isActive);

        rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, -0.5f);
        menuPanelBC2D.size = menuPanelRT.sizeDelta + new Vector2(0.04f, 0);
        menuPanelBC2D.offset = new Vector2(menuPanelRT.sizeDelta.x / 2, -menuPanelRT.sizeDelta.y / 2);
        Vector3 posOffset = new(0.03f, 0, 0);
        Vector3[] desktopManagerCorners = Utils.GetObjectWorldCorners(FindAnyObjectByType<DesktopManager>().GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(menuPanelRT);

        // Right alignment
        menuPanelRT.pivot = new Vector2(0, 1f);
        menuPanel.transform.position = Utils.GetObjectWorldCorners(rectTransform)[2] + posOffset;
        Vector3[] menuPanelCorners = Utils.GetObjectWorldCorners(menuPanelRT);

        if (menuPanelCorners[2].x > desktopManagerCorners[2].x)
        {
            // Left alignment
            menuPanelRT.pivot = new Vector2(1f, 0.5f);
            menuPanel.transform.position = Utils.GetObjectWorldCorners(rectTransform)[0] - posOffset;
        }

        menuPanelCorners = Utils.GetObjectWorldCorners(menuPanelRT);
        if (menuPanelCorners[0].y < desktopManagerCorners[0].y)
        {
            // Adjust upwards
            float yOffset = desktopManagerCorners[0].y - menuPanelCorners[0].y;
            menuPanel.transform.position += new Vector3(0, yOffset, 0); 
        }

        mMenuItem.isSubmenuVisible = isActive;
        toggleCoroutineRunning = false;
    }
}
