using System.Net;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Icon on the desktop
/// </summary>
public class Icon: MonoBehaviour
{
    public GameObject appPrefab; // The window that opens when app is launched
    public DesktopManager desktopManager;
    public Canvas canvas;

    private Vector3 dragOffset; // Makes sure the dragged icon is located where you clicked, not centered around mouse position
    private GameObject dragGhost; // "ghost" of app in previous position when dragging across desktop
    private Vector3 initialPosition;
    private Image iconImage;
    private BoxCollider2D boxCollider;
    private RectTransform rectTransform;
    private HoverPanel HoverPanel;

    private readonly float clickThreshold = 0.3f;
    private bool isClicked = false;
    private float lastClickTime;
    void Awake()
    {
        iconImage = GetComponent<Image>();
        boxCollider = GetComponent<BoxCollider2D>();
        rectTransform = GetComponent<RectTransform>();
        HoverPanel = GetComponent<HoverPanel>();
    }

    private void Start()
    {
        initialPosition = rectTransform.TransformPoint(Vector3.zero);
        PositionIcon(initialPosition);
    }

    private void Update()
    {
        if (isClicked && Time.time > (lastClickTime + clickThreshold))
        {
            //Single clicked
            isClicked = false;
        }
    }

    void OnMouseDown()
    {
        dragOffset = transform.position - Utils.GetMousePos();
        initialPosition = transform.position;
        dragGhost = Instantiate(gameObject, initialPosition, Quaternion.identity, gameObject.transform.parent);
        boxCollider.enabled = false;

        Color objectColor = iconImage.color;
        iconImage.color = new Color(objectColor.r, objectColor.g, objectColor.b, 0.5f);
    }

    void OnMouseUp()
    {
        PositionIcon(Utils.GetMousePos());
        boxCollider.enabled = true;
        if (dragGhost != null) { Destroy(dragGhost); }

        Color objectColor = iconImage.color;
        iconImage.color = new Color(objectColor.r, objectColor.g, objectColor.b, 1f);

        if (isClicked && Time.time <= (lastClickTime + clickThreshold))
        {
            //Double clicked
            isClicked = false;
            desktopManager.OpenApp(appPrefab);
        }
        else
        {
            isClicked = true;
            lastClickTime = Time.time;
        }
    }
    void OnMouseDrag()
    {
        transform.position = Utils.GetMousePos() + dragOffset;
    }
    void PositionIcon(Vector3 position)
    {
        if (desktopManager == null) return;
        Vector2 gridPosition = desktopManager.GetSnappedPosition(position);

        if (desktopManager.IsPositionOccupied(gridPosition))
        {
            transform.position = initialPosition;
        }
        else
        {
            transform.position = gridPosition;
            desktopManager.UpdatePosition(initialPosition, gridPosition);
        }
    }

    public void UpdateIconSize(float cellWidth, float cellHeight)
    {
        Camera camera = Camera.main;
        Vector3 originScreen = camera.WorldToScreenPoint(Vector3.zero);

        float cellWidthPixels = camera.WorldToScreenPoint(new Vector3(cellWidth, 0, 0)).x - originScreen.x;
        float cellHeightPixels = camera.WorldToScreenPoint(new Vector3(0, cellHeight, 0)).y - originScreen.y;

        float canvasScale = canvas.scaleFactor;
        float scaledCellWidthPixels = cellWidthPixels / canvasScale;
        float scaledCellHeightPixels = cellHeightPixels / canvasScale;

        float squareCellSize = Mathf.Min(scaledCellWidthPixels, scaledCellHeightPixels);

        rectTransform.sizeDelta = new Vector2(squareCellSize, squareCellSize);

        HoverPanel.SetHoverPanelSize(scaledCellWidthPixels, scaledCellHeightPixels);
    }
}