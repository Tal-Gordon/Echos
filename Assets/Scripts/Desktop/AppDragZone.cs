using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class AppDragZone : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Vector3 _dragOffset;
    private Image maximizeGraphic;
    private Sprite maximizedWindowControlButton;
    private Sprite nonMaximizedWindowControlButton;

    private float buttonsSizeOffset;
    private bool maximized;
    private bool unmaximizedByDrag = false;
    private Vector3 mouseDragStartPosition;
    void Start()
    {
        boxCollider = Utils.GetOrAddComponent<BoxCollider2D>(gameObject);
        maximizeGraphic = transform.Find("Window Controls").transform.Find("Maximize").transform.Find("Graphic").GetComponent<Image>();

        nonMaximizedWindowControlButton = Utils.Load("Graphics/UI/Icons/WindowControls", "WindowControls_1");
        maximizedWindowControlButton = Utils.Load("Graphics/UI/Icons/WindowControls", "WindowControls_2");

        buttonsSizeOffset = transform.Find("Window Controls").transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x * 3;
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        boxCollider.offset = new Vector2(-buttonsSizeOffset/2, 0);

        UpdateBoxColliderSize();
        UpdateInternalMaximizedBoolean();
    }
    public void UpdateBoxColliderSize()
    {
        float appWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        boxCollider.size = new Vector2(appWidth - buttonsSizeOffset, GetComponent<RectTransform>().sizeDelta.y);
    }
    public void UpdateInternalMaximizedBoolean()
    {
        if (maximizeGraphic.sprite == maximizedWindowControlButton)
        {
            maximized = true;
        }
        else if (maximizeGraphic.sprite == nonMaximizedWindowControlButton)
        {
            maximized = false;
        }
    }
    void OnMouseDown()
    {
        Vector3 mousePos = Utils.GetMousePos();
        _dragOffset = transform.parent.position - mousePos;
        mouseDragStartPosition = mousePos;
    }
    void OnMouseDrag()
    {
        Vector3 mousePos = Utils.GetMousePos();
        if (mousePos != mouseDragStartPosition)
        {
            if (!maximized)
            {
                if (unmaximizedByDrag) 
                { 
                    _dragOffset = transform.parent.position - mousePos;
                    unmaximizedByDrag = false; 
                }
                transform.parent.position = mousePos + _dragOffset;
            }
            else
            {
                transform.parent.GetComponent<App>().DragMaximize(1600, 900);
                unmaximizedByDrag = true;
            }
        }
    }
}
