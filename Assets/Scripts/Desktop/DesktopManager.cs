using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the desktop layout and app behavior.
/// </summary>
public class DesktopManager : MonoBehaviour
{
    public GameObject rightClickMenuGO;
    public Transform appContainer; // Parent object for app instances
    public GameObject iconsHolder; // Parent object for icons
    public GameObject taskbar;
    public GameObject canvas;

    [Header("Desktop Settings")]
    public int iconsPerRow = 25;
    public int iconsPerColumn = 10;
    [Header("Debug")]
    public bool drawDebugGrid = false;

    private float desktopWidth; // Width of the desktop area
    private float desktopHeight; // Height of the desktop area
    private float horizontalMargin;
    private float verticalMargin;

    [HideInInspector]
    public bool arrangeIcons, alignIcons, toggleIcons;

    private DataManager.DataCategory cat;
    private bool[,] iconsGrid;

    void Start()
    {
        cat = DataManager.DataCategory.System;
        iconsGrid = new bool[iconsPerRow, iconsPerColumn];
        CalculateDesktopSize();
        CalculateMargins();
        SetGridSize(iconsPerRow, iconsPerColumn);
        LoadSettings();
    }

    private void CalculateDesktopSize()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.orthographic)
        {
            desktopHeight = mainCam.orthographicSize * 2f;
            desktopWidth = desktopHeight * mainCam.aspect;
        }
        else
        {
            Debug.LogWarning("Main camera not found or not orthographic. Using default desktop size.");
            desktopWidth = 21.333f;
            desktopHeight = 12f;
        }
    }

    private void CalculateMargins()
    {
        float cellWidth = desktopWidth / iconsPerRow;
        float cellHeight = desktopHeight / iconsPerColumn;

        if (cellWidth <= cellHeight)
        {
            horizontalMargin = 0f;
            verticalMargin = cellHeight - cellWidth;
        }
        else
        {
            verticalMargin = 0f;
            horizontalMargin = cellWidth - cellHeight;
        }
    }

    public async void LoadSettings() // TODO
    {
        // arrange icons
        // align icons
        if (iconsHolder.activeSelf != await DataManager.ReadDataAsync<bool>(cat, "IconsShown"))
        {
            ToggleIcons();
        }
        // save desktop order
    }

    public Vector2 GetCellSize()
    {
        // Calculate the usable cell size with margins included within each cell
        float cellWidth = desktopWidth / iconsPerRow;
        float cellHeight = desktopHeight / iconsPerColumn;

        // The active area of each cell is smaller than the full cell size
        float activeWidth = cellWidth - horizontalMargin;
        float activeHeight = cellHeight - verticalMargin;

        return new Vector2(activeWidth, activeHeight);
    }

    private void GetCanvasGridParameters(
        out float minX,
        out float maxX,
        out float minY,
        out float maxY,
        out float worldWidth,
        out float worldHeight,
        out float cellWidth,
        out float cellHeight)
    {
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Vector3[] corners = Utils.GetObjectWorldCorners(canvasRectTransform);

        // Initialize bounds from first values
        minX = corners[0].x;
        maxX = corners[1].x;
        minY = corners[3].y;
        maxY = corners[0].y;

        // Ensure we cover all corners
        foreach (Vector3 corner in corners)
        {
            minX = Mathf.Min(minX, corner.x);
            maxX = Mathf.Max(maxX, corner.x);
            minY = Mathf.Min(minY, corner.y);
            maxY = Mathf.Max(maxY, corner.y);
        }

        worldWidth = maxX - minX;
        worldHeight = maxY - minY;
        cellWidth = worldWidth / iconsPerRow;
        cellHeight = worldHeight / iconsPerColumn;
    }

    // Snaps a given position to the nearest available grid point on the desktop
    public Vector2 GetSnappedPosition(Vector2 position)
    {
        float clampMargin = 0.01f;
        GetCanvasGridParameters(out float minX, out float maxX, out float minY, out float maxY,
                                out float worldWidth, out float worldHeight, out float cellWidth, out float cellHeight);

        // Clamp the input position to the canvas bounds (with a small margin)
        position.x = Mathf.Clamp(position.x, minX + clampMargin, maxX - clampMargin);
        position.y = Mathf.Clamp(position.y, minY + clampMargin, maxY - clampMargin);

        // Normalize positions within the canvas bounds (assuming a top-down grid)
        float normalizedX = (position.x - minX) / worldWidth;
        float normalizedY = (maxY - position.y) / worldHeight;
        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);

        int gridX = Mathf.FloorToInt(normalizedX * iconsPerRow);
        int gridY = Mathf.FloorToInt(normalizedY * iconsPerColumn);
        gridX = Mathf.Clamp(gridX, 0, (int)iconsPerRow - 1);
        gridY = Mathf.Clamp(gridY, 0, (int)iconsPerColumn - 1);

        // Snap to the center of the computed grid cell
        float snappedX = minX + (gridX + 0.5f) * cellWidth;
        float snappedY = maxY - (gridY + 0.5f) * cellHeight;

        return new Vector2(snappedX, snappedY);
    }

    // Checks if the given position is occupied by another icon
    public bool IsPositionOccupied(Vector2 position)
    {
        Vector2Int gridIndex = GetGridIndex(position);
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;

        // Check if the grid indices are within bounds
        if (gridX >= 0 && gridX < iconsPerRow && gridY >= 0 && gridY < iconsPerColumn)
        {
            return iconsGrid[gridX, gridY];
        }
        return true; // Position is out of grid bounds, consider it occupied
    }

    public void UpdatePosition(Vector2 oldPosition, Vector2 newPosition)
    {
        Vector2Int gridIndexOld = GetGridIndex(oldPosition);
        int gridXOld = gridIndexOld.x;
        int gridYOld = gridIndexOld.y;
        Vector2Int gridIndexNew = GetGridIndex(newPosition);
        int gridXNew = gridIndexNew.x;
        int gridYNew = gridIndexNew.y;

        iconsGrid[gridXOld, gridYOld] = false;
        iconsGrid[gridXNew, gridYNew] = true;
    }

    // Gets world position, returns grid index
    public Vector2Int GetGridIndex(Vector2 position)
    {
        GetCanvasGridParameters(out float minX, out _, out _, out float maxY,
                                out float worldWidth, out float worldHeight, out _, out _);

        // Normalize the position similar to GetSnappedPosition (top-down grid)
        float normalizedX = (position.x - minX) / worldWidth;
        float normalizedY = (maxY - position.y) / worldHeight;
        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);

        int gridX = Mathf.FloorToInt(normalizedX * iconsPerRow);
        int gridY = Mathf.FloorToInt(normalizedY * iconsPerColumn);
        gridX = Mathf.Clamp(gridX, 0, iconsPerRow - 1);
        gridY = Mathf.Clamp(gridY, 0, iconsPerColumn - 1);

        return new Vector2Int(gridX, gridY);
    }

    public void UpdateIconSizes()
    {
        Vector2 cellSize = GetCellSize();
        float activeWidth = cellSize.x;
        float activeHeight = cellSize.y;

        if (iconsHolder != null)
        {
            Icon[] icons = iconsHolder.GetComponentsInChildren<Icon>();
            foreach (Icon icon in icons)
            {
                icon.UpdateIconSize(activeWidth, activeHeight);
            }
        }
    }

    public void SetGridSize(int rows, int cols)
    {
        iconsPerRow = rows;
        iconsPerColumn = cols;
        iconsGrid = new bool[iconsPerRow, iconsPerColumn]; // Re-initialize the grid
        UpdateIconSizes(); // Update icon sizes based on the new grid size
    }

    // Handles the logic of opening an application window
    public void OpenApp(GameObject appPrefab)
    {
        if (appPrefab != null && appContainer != null)
        {
            Instantiate(appPrefab, appContainer);
        }
    }

    public async void ArrangeIcons()
    {
    }

    public async void AlignIcons()
    {
    }

    public async void ToggleIcons()
    {
        iconsHolder.SetActive(!iconsHolder.activeSelf);
        await DataManager.WriteDataAsync(cat, "IconsShown", iconsHolder.activeSelf);
    }

    public void OpenSettings()
    {
    }

    public async void SaveDesktopOrder()
    {
    }

    public void NewTextFile()
    {
    }

    private void DrawDebugGrid()
    {
        if (!drawDebugGrid) return; // Don't draw if debug grid is disabled

        float cellWidth = desktopWidth / iconsPerRow;
        float cellHeight = desktopHeight / iconsPerColumn;

        Camera mainCam = Camera.main;
        Vector3 cameraPosition = mainCam.transform.position;
        float desktopLeftEdge = cameraPosition.x - (desktopWidth / 2f);
        float desktopRightEdge = cameraPosition.x + (desktopWidth / 2f);
        float desktopTopEdge = cameraPosition.y + (desktopHeight / 2f);
        float desktopBottomEdge = cameraPosition.y - (desktopHeight / 2f);

        Color gridColor = Color.cyan;
        Color marginColor = Color.yellow;

        // Draw vertical lines for cell boundaries
        for (int x = 0; x <= iconsPerRow; x++)
        {
            float xPos = desktopLeftEdge + (x * cellWidth);
            Debug.DrawLine(new Vector3(xPos, desktopTopEdge, 0), new Vector3(xPos, desktopBottomEdge, 0), gridColor);
        }

        // Draw horizontal lines for cell boundaries
        for (int y = 0; y <= iconsPerColumn; y++)
        {
            float yPos = desktopTopEdge - (y * cellHeight);
            Debug.DrawLine(new Vector3(desktopLeftEdge, yPos, 0), new Vector3(desktopRightEdge, yPos, 0), gridColor);
        }

        // Draw active areas inside cells (to visualize margins)
        for (int x = 0; x < iconsPerRow; x++)
        {
            for (int y = 0; y < iconsPerColumn; y++)
            {
                float leftX = desktopLeftEdge + (x * cellWidth) + (horizontalMargin / 2);
                float rightX = desktopLeftEdge + ((x + 1) * cellWidth) - (horizontalMargin / 2);
                float topY = desktopTopEdge - (y * cellHeight) - (verticalMargin / 2);
                float bottomY = desktopTopEdge - ((y + 1) * cellHeight) + (verticalMargin / 2);

                Debug.DrawLine(new Vector3(leftX, topY, 0), new Vector3(rightX, topY, 0), marginColor);
                Debug.DrawLine(new Vector3(leftX, bottomY, 0), new Vector3(rightX, bottomY, 0), marginColor);
                Debug.DrawLine(new Vector3(leftX, topY, 0), new Vector3(leftX, bottomY, 0), marginColor);
                Debug.DrawLine(new Vector3(rightX, topY, 0), new Vector3(rightX, bottomY, 0), marginColor);
            }
        }
    }

    // Unity Gizmos to draw the debug grid in Scene View
    private void OnDrawGizmos()
    {
        DrawDebugGrid();
    }
}