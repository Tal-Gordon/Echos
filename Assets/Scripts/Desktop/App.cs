using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class App : MonoBehaviour
{
    public Sprite taskbarAppSprite;

    private GameObject taskbarIconPrefab;
    private GameObject taskbarApp;
    private Sprite maximizedWindowControlButton;
    private Sprite nonMaximizedWindowControlButton;
    private Image maximizeGraphic;
    private GameObject dragZone;

    private readonly float maxWidth = 1920f;
    private readonly float maxHeight = 1030f; //1080-taskbar
    private bool maximized = false;
    private float tempNonMaximizedWidth;
    private float tempNonMaximizedHeight;
    private float tempPositionX;
    private float tempPositionY;
    void Start()
    {
        dragZone = transform.Find("DragZone").gameObject;
        GameObject windowControls = dragZone.transform.Find("Window Controls").gameObject;
        GameObject taskbarApps = transform.parent.transform.parent.transform.Find("Taskbar").transform.Find("Apps").gameObject;
        maximizeGraphic = windowControls.transform.Find("Maximize").transform.Find("Graphic").GetComponent<Image>();

        GameObject taskbarAppPrefab = Resources.Load<GameObject>("Prefabs/TaskbarApp");

        taskbarApp = Instantiate(taskbarAppPrefab, taskbarApps.transform);
        taskbarApp.GetComponent<Image>().sprite = taskbarAppSprite;
        taskbarApp.GetComponent<Button>().onClick.AddListener(OnTaskbarAppClick);

        nonMaximizedWindowControlButton = Utils.Load("Graphics/UI/Icons/WindowControls", "WindowControls_1");
        maximizedWindowControlButton = Utils.Load("Graphics/UI/Icons/WindowControls", "WindowControls_2");
        if (maximized)
        {
            maximizeGraphic.sprite = maximizedWindowControlButton;
        }
        else
        {
            maximizeGraphic.sprite = nonMaximizedWindowControlButton;
        }
    }
    public void Close()
    {
        Destroy(taskbarApp);
        Destroy(gameObject);
    }
    public void Maximize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (maximized)
        {
            rectTransform.sizeDelta = new Vector2(tempNonMaximizedWidth, tempNonMaximizedHeight);
            rectTransform.anchoredPosition = new Vector3(tempPositionX, tempPositionY, 0);
            maximizeGraphic.sprite = nonMaximizedWindowControlButton;
        }
        else
        {
            tempNonMaximizedWidth = rectTransform.sizeDelta.x;
            tempNonMaximizedHeight = rectTransform.sizeDelta.y;
            tempPositionX = rectTransform.anchoredPosition.x;
            tempPositionY = rectTransform.anchoredPosition.y;

            rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);
            rectTransform.anchoredPosition = Vector3.zero;
            maximizeGraphic.sprite = maximizedWindowControlButton;
        }
        dragZone.GetComponent<AppDragZone>().UpdateBoxColliderSize();
        dragZone.GetComponent<AppDragZone>().UpdateInternalMaximizedBoolean();
        maximized = !maximized;
    }
    public void DragMaximize(float defaultNonMaximizedWidth, float defaultNonMaximizedHeight)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (maximized)
        {
            rectTransform.sizeDelta = new Vector2(defaultNonMaximizedWidth, defaultNonMaximizedHeight);
            rectTransform.anchoredPosition = new Vector3(Utils.GetMousePos().x, (maxHeight - defaultNonMaximizedHeight) / 2, 0);
            maximizeGraphic.sprite = nonMaximizedWindowControlButton;
        }
        else
        {
            tempNonMaximizedWidth = rectTransform.sizeDelta.x;
            tempNonMaximizedHeight = rectTransform.sizeDelta.y;
            tempPositionX = rectTransform.anchoredPosition.x;
            tempPositionY = rectTransform.anchoredPosition.y;

            rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);
            rectTransform.anchoredPosition = Vector3.zero;
            maximizeGraphic.sprite = maximizedWindowControlButton;
        }
        dragZone.GetComponent<AppDragZone>().UpdateBoxColliderSize();
        dragZone.GetComponent<AppDragZone>().UpdateInternalMaximizedBoolean();
        maximized = !maximized;
    }
    public void Minimize()
    {
        gameObject.SetActive(false);
    }
    void OnTaskbarAppClick()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
