using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

#pragma warning disable 0649

    static GridManager _S;
    public static GridManager S
    {
        get
        {
            if (_S != null) return _S;
            Debug.LogError("GridManager.cs : Attempting to access singleton instance but it has not been set.");
            return null;
        }
        set
        {
            if (_S != null) Debug.LogError("GridManager.cs : Singleton instance was modified but it already had a value.");
            _S = value;
        }
    }


    [SerializeField] TextAsset levelJSON = null;


    TouchInfo primaryTouchInfo;

    GridCell selectedCell;
    bool cellSelected;
    GridCell swipedCell;
    bool shouldGetSwipedCell = false;

    
    GridCell[,] grid;
    public static GridCell[,] GRID
    {
        get { return S.grid; }
    }



    [Header("Grid Settings")]
    [SerializeField] int gridHeight;
    static public int GRID_HEIGHT
    {
        get { return S.gridHeight; }
        private set { S.gridHeight = value; }
    }

    [SerializeField] int gridWidth;
    static public int GRID_WIDTH
    {
        get { return S.gridWidth; }
        private set { S.gridWidth = value; }
    }



    //[Header("Grid Display")]
    //[SerializeField] float verticalMargin;
    //[SerializeField] float horizontalMargin;
    //[SerializeField] Vector2 offsetFromCenter;
    //float cellSize = 1;

    //[Space]
    //[SerializeField] GameObject gridCellBackgroundPrefab;
    //[SerializeField] SpawnableGridElementsScriptableObject spawnableGridElements;

    //SpriteRenderer gridBackgroundSprite;
    //Transform cellContainer;
    //Transform gridElementContainer;

    private void Awake()
    {
        //Singleton initialization
        S = this;
    }
    private void Start()
    {


        // Check that nothing important is null
        //gridBackgroundSprite = transform.Find("GridBackground").GetComponent<SpriteRenderer>();
        //if (gridBackgroundSprite == null) Debug.LogError("GridManager.cs : gridBackgroundSprite couldn't be found. Is there a SpriteRenderer on a child of this object?");
        //
        //cellContainer = transform.Find("CellContainer");
        //if (cellContainer == null) Debug.LogError("GridManager.cs : GridContainer object couldn't be found in children of this object.");
        //
        //gridElementContainer = transform.Find("GridElementContainer");
        //if (gridElementContainer == null) Debug.LogError("GridManager.cs : GridElementContainer object couldn't be found in children of this object.");
        //
        //if (gridCellBackgroundPrefab == null) Debug.LogError("GridManager.cs : The grid cell prefab is null. Has it been assigned in the inspector?");
        ////if (spawnableGridElements == null) Debug.LogError("GridManager.cs : The spawnableGridElements scriptable object is null. Has it been assigned in the inspector?");



        // grid initialization
        SetupGrid();

        primaryTouchInfo = new TouchInfo();
    }



    private void Update()
    {
        if (primaryTouchInfo.touching && !cellSelected)
        {
            if (primaryTouchInfo.startGridPosition != new Vector2Int(int.MinValue, int.MinValue)) // grid position is (minvalue, minvalue) when outside the grid
            {
                selectedCell = grid[primaryTouchInfo.startGridPosition.x, primaryTouchInfo.startGridPosition.y];
            }

            cellSelected = true;
        }
        if (!primaryTouchInfo.touching && cellSelected)
        {
            cellSelected = false;
        }

        if(shouldGetSwipedCell)
        {
            if (primaryTouchInfo.startGridPosition != new Vector2Int(int.MinValue, int.MinValue))
            {
                int x = primaryTouchInfo.startGridPosition.x + primaryTouchInfo.swipeDirection.x;
                int y = primaryTouchInfo.startGridPosition.y - primaryTouchInfo.swipeDirection.y;

                if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                {
                    swipedCell = grid[x, y];

                    


                }
            }
        }
    }


    #region Grid Setup functions
    [ContextMenu("Initialize Grid")]
    public void SetupGrid()
    {
        //tear down current grid first
        TeardownGrid();

        grid = new GridCell[GRID_WIDTH, GRID_HEIGHT];


        for (int i = 0; i < GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GRID_HEIGHT; ++j)
            {
                // set grid cell struct
                grid[i, j].visible = true;
                grid[i, j].empty = false;
                grid[i, j].x = i;
                grid[i, j].y = j;

                // FIND A BETTER WAY TO DO THIS
                grid[i, j].cellContent = (CellContents)Random.Range(1, 6); 

            }
        }

        GridDisplayer.InitializeGridDisplay();

    }


    public void TeardownGrid()
    {
        grid = new GridCell[GRID_WIDTH, GRID_HEIGHT];
    }

    public void EmptyCellContents()
    {
        // delete all cell contents
    }
    #endregion



    



    #region Input receiving
    /// <summary>
    /// Called by Input.cs on the frame a primary touch is detected.
    /// </summary>
    /// <param name="worldTouchPos">The world-space position of the primary touch.</param>
    public static void TouchBegin(Vector3 worldTouchPos)
    {
        // touchInfo is a struct, so it's passed by value.
        // when we operate on primtouch, we're not actually updating S.primaryTouchInfo.
        TouchInfo primTouch = S.primaryTouchInfo;

        primTouch.touching = true;

        // worldPosition and startWorldPosition
        primTouch.worldPosition = primTouch.startWorldposition = worldTouchPos;

        // gridPosition and startGridPosition
        Vector2Int gridPos = GridDisplayer.WorldToGrid(worldTouchPos);
        if (gridPos.x >= GRID_WIDTH || gridPos.x < 0 || gridPos.y >= GRID_HEIGHT || gridPos.y < 0)
        {
            primTouch.gridPosition = new Vector2Int(int.MinValue, int.MinValue);
        }
        else
        {
            primTouch.gridPosition = primTouch.startGridPosition = gridPos;
        }

        // end positions
        primTouch.endWorldPosition = new Vector2(float.MinValue, float.MinValue);
        primTouch.endGridPosition = new Vector2Int(int.MinValue, int.MinValue);

        S.primaryTouchInfo = primTouch;

        }

    
    /// <summary>
    /// Called by Input.cs starting every frame a primary touch is detected, starting from the frame after TouchBegin is called.
    /// </summary>
    /// <param name="worldTouchPos">The world-space position of the primary touch.</param>
    public static void TouchMove(Vector3 worldTouchPos)
    {
        TouchInfo touchInfo = S.primaryTouchInfo;

        //world position
        touchInfo.worldPosition = worldTouchPos;

        // gridPosition
        Vector2Int gridPos = GridDisplayer.WorldToGrid(worldTouchPos);
        if (gridPos.x >= GRID_WIDTH || gridPos.x < 0 || gridPos.y >= GRID_HEIGHT || gridPos.y < 0)
        {
            touchInfo.gridPosition = new Vector2Int(int.MinValue, int.MinValue);
        }
        else
        {
            touchInfo.gridPosition = gridPos;
        }


        S.primaryTouchInfo = touchInfo;
    }


    /// <summary>
    /// Called by Input.cs on the frame a primary touch stops being detected.
    /// </summary>
    /// <param name="worldTouchPos">The world-space position of the touch when it left the screen.</param>
    public static void TouchEnd(Vector3 worldTouchPos)
    {
        TouchInfo touchInfo = S.primaryTouchInfo;

        // world position and end world position
        touchInfo.worldPosition = touchInfo.endWorldPosition = worldTouchPos;

        // gridPosition and endGridPosition
        Vector2Int gridPos = GridDisplayer.WorldToGrid(worldTouchPos);
        if (gridPos.x >= GRID_WIDTH || gridPos.x < 0 || gridPos.y >= GRID_HEIGHT || gridPos.y < 0)
        {
            touchInfo.gridPosition = new Vector2Int(int.MinValue, int.MinValue);
        }
        else
        {
            touchInfo.gridPosition =  touchInfo.endGridPosition = gridPos;
        }

        S.primaryTouchInfo = touchInfo;

        if (touchInfo.swipeDirection != Vector2.zero)
        {
            S.shouldGetSwipedCell = true;
        }
    }


    /// <summary>
    /// Called by Input.cs every frame a primary touch isn't detected, starting from the frame after TouchEnd is called.
    /// </summary>
    public static void NoTouch()
    {
        TouchInfo touchInfo = S.primaryTouchInfo;

        touchInfo.touching = false;

        touchInfo.worldPosition = touchInfo.startWorldposition  = touchInfo.endWorldPosition = new Vector2(float.MinValue, float.MinValue);
        touchInfo.gridPosition = touchInfo.startGridPosition = touchInfo.endGridPosition = new Vector2Int(int.MinValue, int.MinValue);


        S.primaryTouchInfo = touchInfo;
        S.shouldGetSwipedCell = false;
    }
    #endregion



    #region Structs and Classes
    [System.Serializable]
    public struct GridCell
    {
        public int x, y;

        [HideInInspector]
        public bool visible;
        [HideInInspector]
        public bool empty;

        public CellContents cellContent;        
    }

    public enum CellContents
    {
        hole    = -1,
        empty   = 0,
        candy_blue      = 1,
        candy_red       = 2,
        candy_green     = 3,
        candy_orange    = 4,
        candy_yellow    = 5
    }

    public struct TouchInfo
    {
        public bool touching;

        public Vector2 worldPosition;
        public Vector2Int gridPosition;

        public Vector2 startWorldposition;
        public Vector2Int startGridPosition;

        public Vector2Int swipeDirection
        {
            get 
            {
                Vector2 dir = (worldPosition - startWorldposition).normalized;
                return new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y)) ;
            }
        }

        public Vector2 endWorldPosition;
        public Vector2Int endGridPosition;

    }

    #endregion
}
