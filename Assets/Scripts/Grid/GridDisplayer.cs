using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDisplayer : MonoBehaviour
{

    static GridDisplayer _S;

    public static GridDisplayer S
    {
        get
        {
            if (_S != null) return _S;
            Debug.LogError("GridDisplayer.cs : Attempting to access singleton instance but it has not been set.");
            return null;
        }
        set
        {
            if (_S != null) Debug.LogError("GridDisplayer.cs : Singleton instance was modified but it already had a value.");
            _S = value;
        }
    }

#pragma warning disable 0649
    [SerializeField] float verticalMargin;
    [SerializeField] float horizontalMargin;
    [Space]
    [SerializeField] Vector2 offsetFromCenter;
    float cellSize = 1;
    [Space]
    [SerializeField] GameObject gridCellBackgroundPrefab;
    //[SerializeField] SpawnableGridElementsScriptableObject spawnableGridElements;

    SpriteRenderer gridBackgroundSprite;
    Transform cellContainer;
    Transform gridElementContainer;

    private void Awake()
    {
        S = this;
    }


    private void Start()
    {

        gridBackgroundSprite = transform.Find("GridBackground").GetComponent<SpriteRenderer>();
        if (gridBackgroundSprite == null) Debug.LogError("GridDisplayer.cs : gridBackgroundSprite couldn't be found. Is there a SpriteRenderer on a child of this object?");

        cellContainer = transform.Find("CellContainer");
        if (cellContainer == null) Debug.LogError("GridDisplayer.cs : GridContainer object couldn't be found in children of this object.");

        gridElementContainer = transform.Find("GridElementContainer");
        if (gridElementContainer == null) Debug.LogError("GridManager.cs : GridElementContainer object couldn't be found in children of this object.");

        if (gridCellBackgroundPrefab == null) Debug.LogError("GridDisplayer.cs : The grid cell prefab is null. Has it been assigned in the inspector?");
        //if (spawnableGridElements == null) Debug.LogError("GridManager.cs : The spawnableGridElements scriptable object is null. Has it been assigned in the inspector?");
    }

    public void GridDisplayInit()
    {

        float hCellSize = (gridBackgroundSprite.size.x - (horizontalMargin * 2)) / GridManager.GRID_WIDTH;
        float vCellSize = (gridBackgroundSprite.size.y - (verticalMargin * 2)) / GridManager.GRID_HEIGHT;

        cellSize = Mathf.Min(hCellSize, vCellSize);


        for (int i = 0; i < GridManager.GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GridManager.GRID_HEIGHT; ++j)
            {
                GameObject cell = Instantiate(gridCellBackgroundPrefab, GridToWorld(i, j), Quaternion.identity, cellContainer);

                cell.GetComponent<SpriteRenderer>().size = new Vector2(cellSize, cellSize);

            }
        }
    }


    // static wrapper for calling GridDisplayInit from external scripts
    public static void InitializeGridDisplay()
    {
        S.GridDisplayInit();
    }



    # region Grid helper functions
    public static Vector2 GridToWorld(Vector2Int gridPos)
    {
        float x = S.offsetFromCenter.x + (S.cellSize * gridPos.x) - (S.cellSize * (GridManager.GRID_WIDTH - 1)) / 2;
        float y = S.offsetFromCenter.y + (S.cellSize * -gridPos.y) + (S.cellSize * (GridManager.GRID_HEIGHT - 1)) / 2;

        return new Vector2(x, y);
    }

    public static Vector2 GridToWorld(int gridX, int gridY)
    {
        float x = S.offsetFromCenter.x + (S.cellSize * gridX) - (S.cellSize * (GridManager.GRID_WIDTH - 1)) / 2;
        float y = S.offsetFromCenter.y + (S.cellSize * -gridY) + (S.cellSize * (GridManager.GRID_HEIGHT - 1)) / 2;

        return new Vector2(x, y);
    }

    public static Vector2Int WorldToGrid(Vector2 worldPos)
    {
        float x = (-worldPos.x + S.offsetFromCenter.x - (S.cellSize * (GridManager.GRID_WIDTH - 1)) / 2) / -S.cellSize;
        float y = (-worldPos.y + S.offsetFromCenter.y + (S.cellSize * (GridManager.GRID_HEIGHT - 1)) / 2) / S.cellSize;

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    #endregion
}
