using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RightClickMenu : MonoBehaviour, IPointerDownHandler
{
    public GameObject menuPanel;
    public GameObject buttonPrefab;
    public DesktopManager desktopManager;
    //[SerializeField] List<MenuItem> menuButtons;
    private List<SubmenuManager> submenuManagersList;
    void Start()
    {
        submenuManagersList = new();
    }

    private void Update()
    {
        if (menuPanel.activeSelf)
        {
            PointerEventData eventDataCurrentPosition = new(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };
            List<RaycastResult> raycastResults = new();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, raycastResults);
            HashSet<SubmenuManager> seenInRaycastResults = new();

            foreach (RaycastResult result in raycastResults)
            {
                SubmenuManager[] submenuManagers = result.gameObject.GetComponentsInParent<SubmenuManager>();
                foreach (SubmenuManager submenuManager in submenuManagers)
                {
                    if (submenuManager)
                    {
                        if (!submenuManagersList.Contains(submenuManager) && !submenuManager.toggleCoroutineRunning)
                        {
                            submenuManager.gameObject.SetActive(true);
                            submenuManager.PublicToggleSubmenu(true);
                            submenuManagersList.Add(submenuManager);
                        }
                        seenInRaycastResults.Add(submenuManager);
                    } 
                }
            }
            for (int i = submenuManagersList.Count - 1; i >= 0; i--)
            {
                if (!seenInRaycastResults.Contains(submenuManagersList[i]) && !submenuManagersList[i].toggleCoroutineRunning)
                {
                    submenuManagersList[i].PublicToggleSubmenu(false);
                    submenuManagersList.Remove(submenuManagersList[i]);
                }
            }
        }
    }

    void UpdateMenu(IMenuProvider menuProvider, GameObject panel)
    {
        foreach (Transform child in panel.transform)
            Destroy(child.gameObject);
        submenuManagersList.Clear();

        List<MenuItem> menuButtons = new(menuProvider.GetMenuItems());
        foreach (var item in menuButtons)
        {
            GameObject button = Instantiate(buttonPrefab, panel.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = item.Title;
            button.name = $"{item.Title} {item.GetType()}";
            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.AddListener(item.OnClick);
            buttonComponent.onClick.AddListener(HideMenu);
            button.GetComponent<RectTransform>().localScale = Vector3.one;

            if (item is ToggleMenuItem toggleItem)
            {
                GameObject icon = button.transform.Find("Icon").gameObject;
                icon.GetComponent<Image>().sprite = Resources.Load<Sprite>("Graphics/UI/checkmark");
                buttonComponent.onClick.AddListener(() => { ToggleCheckmark(button); });
                icon.SetActive(toggleItem.IsOn);
            }

            if (item is SubmenuMenuItem submenuItem)
            {
                buttonComponent.onClick.RemoveAllListeners();
                button.transform.Find("Additional").GetComponent<TextMeshProUGUI>().text = ">";

                SubmenuManager submenuManager = Utils.GetOrAddComponent<SubmenuManager>(button);
                submenuManager.Initialize(submenuItem);
                UpdateMenu(submenuItem, submenuManager.menuPanel);
            }
        }
    }
    public void ShowMenu()
    {
        RectTransform panelRectTransform = menuPanel.GetComponent<RectTransform>();

        Vector3[] panelCorners = Utils.GetObjectWorldCorners(menuPanel.GetComponent<RectTransform>());
        Vector2 panelSize = panelCorners[3] - panelCorners[1];

        Vector3[] desktopCorners = Utils.GetObjectWorldCorners(desktopManager.GetComponent<RectTransform>());

        Vector3 mousePos = Utils.GetMousePos();
        Vector3 menuPosition = mousePos + new Vector3(0.1f, -0.1f, 0); // slight offset, for visual purposes

        panelRectTransform.pivot = new Vector2(0, 1f); // Below and to the right of the mouse
        if (mousePos.x + panelSize.x > desktopCorners[3].x) // If menu would go beyond the right screen edge
        {
            // Move it to the left enough so it fits within the screen
            menuPosition.x = desktopCorners[3].x - panelSize.x;
        }

        if (mousePos.y + panelSize.y < desktopCorners[3].y) // If menu would go below the bottom screen edge
        {
            // Move it up enough so it fits within the screen
            menuPosition.y = desktopCorners[3].y - panelSize.y;
        }

        menuPanel.transform.position = menuPosition;
        menuPanel.SetActive(true);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
    }
    public void ToggleCheckmark(GameObject button)
    {
        GameObject icon = button.transform.Find("Icon").gameObject;
        icon.SetActive(!icon.activeSelf);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Debug.Log("Left mouse button down: " + eventData.pointerCurrentRaycast.gameObject.name);
            if (!eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(desktopManager.rightClickMenuGO.transform))
            {
                HideMenu();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //Debug.Log("Right mouse button down: " + eventData.pointerCurrentRaycast.gameObject.name);
            if (eventData.pointerCurrentRaycast.gameObject.GetComponent<IMenuProvider>() != null)
            {
                GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
                UpdateMenu(clickedObject.GetComponent<IMenuProvider>(), menuPanel);
                ShowMenu();
            }
        }
    }
    public bool IsPanelOn()
    {
        return menuPanel.activeSelf;
    }
}
