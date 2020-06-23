﻿using System.Collections;
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
    Vector2Int selectedCell;
    bool cellSelected;
    Vector2Int swipedCell;
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



    private void Awake()
    {
        //Singleton initialization
        S = this;
    }


    private void Start()
    {
        // grid initialization
        SetupGrid();

        primaryTouchInfo = new TouchInfo();
    }



    private void Update()
    {
        if (!primaryTouchInfo.touching)
        {
            //swipedCell = null;
            //selectedCell = null;
        }

        if (primaryTouchInfo.touching && !cellSelected)
        {
            if (primaryTouchInfo.startGridPosition != new Vector2Int(int.MinValue, int.MinValue)) // grid position is (minvalue, minvalue) when outside the grid
            {
                selectedCell = primaryTouchInfo.startGridPosition;
                //Debug.Log(grid[selectedCell.x, selectedCell.y].cellContent);
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
                    // the cell the player is swiping
                    swipedCell = new Vector2Int(x,y);

                    // swap the cellContent variables of the cells being swapped
                    CellContents temp = grid[swipedCell.x, swipedCell.y].cellContent;
                    grid[swipedCell.x, swipedCell.y].cellContent = grid[selectedCell.x, selectedCell.y].cellContent;
                    grid[selectedCell.x, selectedCell.y].cellContent = temp;


                    if (GetMatchesAtCell(swipedCell).Count != 0 || GetMatchesAtCell(selectedCell).Count != 0)
                    {

                        // this is kind of a hack :
                        // Basically, when a GridElement swaps, it deregisters its swap method from its 
                        //    current cell and registers it on the new cell. however, since the new 
                        //    cell's swap method gets called right after, the first GridElement's swap 
                        //    method was being called twice. 
                        //    We "solve" this by taking a "snapshot" of the swiped gridcell before the 
                        //    first GridElement registers its swap method on it, and calling the swap
                        //    method of this "snapshot". The first GridElement's swap method is only 
                        //    called once and still registers itself on the right cell.
                        GridCell swipedCellCopy = grid[swipedCell.x, swipedCell.y];

                        if (grid[selectedCell.x, selectedCell.y].Swap != null)
                            grid[selectedCell.x, selectedCell.y].Swap(new Vector2Int(swipedCell.x, swipedCell.y));

                        if (swipedCellCopy.Swap != null)
                            swipedCellCopy.Swap(new Vector2Int(selectedCell.x, selectedCell.y));
                    }
                    else
                    {
                        // call swap fail event (also create a swap fail event)

                        // swap cell contents back
                        CellContents temp2 = grid[swipedCell.x, swipedCell.y].cellContent;
                        grid[swipedCell.x, swipedCell.y].cellContent = grid[selectedCell.x, selectedCell.y].cellContent;
                        grid[selectedCell.x, selectedCell.y].cellContent = temp2;
                    }

                }
            }
        }
    }


    
    public List<Vector2Int> GetMatchesAtCell(GridCell cell)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        List<Vector2Int> matches = new List<Vector2Int>();

        for (int i = 0; i<directions.Length; ++i)
        {
            if (cell.x + directions[i].x >= GRID_WIDTH || cell.x + directions[i].x < 0 || cell.y + directions[i].y >= GRID_HEIGHT || cell.y + directions[i].y < 0)
            {
                continue;
            }
            GridCell checkedCell = grid[cell.x + directions[i].x, cell.y + directions[i].y];

            if (cell.cellContent == checkedCell.cellContent)
            {
                if (checkedCell.x + directions[i].x >= GRID_WIDTH || checkedCell.x + directions[i].x < 0 || checkedCell.y + directions[i].y >= GRID_HEIGHT || checkedCell.y + directions[i].y < 0)
                {
                    continue;
                }

                GridCell thirdCell = grid[checkedCell.x + directions[i].x, checkedCell.y + directions[i].y];
                if (checkedCell.cellContent == thirdCell.cellContent)
                {
                    matches.Add(new Vector2Int(cell.x, cell.y));
                    matches.Add(new Vector2Int(checkedCell.x, checkedCell.y));
                    matches.Add(new Vector2Int(thirdCell.x, thirdCell.y));
                }
            }
        }

        return matches;
    }

    public List<Vector2Int> GetMatchesAtCell(Vector2Int cellPos)
    {
        return GetMatchesAtCell(grid[cellPos.x, cellPos.y]);
    }




    #region Grid Setup functions
    [ContextMenu("Initialize Grid")]
    public void SetupGrid()
    {

        grid = new GridCell[GRID_WIDTH, GRID_HEIGHT];


        for (int i = 0; i < GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GRID_HEIGHT; ++j)
            {
                grid[i, j] = new GridCell()
                {
                    visible = true,
                    empty = false,
                    x = i,
                    y = j,

                    // FIND A BETTER WAY TO DO THIS
                    cellContent = (CellContents)Random.Range(1, 6)
                };
            }
        }


        // INITIAL MATCH DETECTION FOR CLEAN GRID
        List<Vector2Int> allMatches = new List<Vector2Int>();
        do
        {
            // we first remove the matches if there are any.
            for (int i = 0; i < allMatches.Count; i++)
            {
                Vector2Int cell = allMatches[i];
                grid[cell.x, cell.y].cellContent = (CellContents)Random.Range(1, 6);
                allMatches.Remove(cell);
            }

            // then we check the grid for matches. The above operation might have created new matches.
            for (int i = 0; i < GRID_WIDTH; ++i)
            {
                for (int j = 0; j < GRID_HEIGHT; ++j)
                {
                    foreach (Vector2Int cellPos in GetMatchesAtCell(new Vector2Int(i, j)))
                    {
                        if (!allMatches.Contains(cellPos))
                        {
                            allMatches.Add(cellPos);
                        }
                    }
                }
            }
            // finally we loop back as long as the above nested for loop has found at least one match
        } while (allMatches.Count != 0);


        GridDisplayer.InitializeGridDisplay();
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

        /// <summary>
        /// [possibly deprecated] Whether or not this cell is part of the grid and can contain something.
        /// </summary>
        [HideInInspector]
        public bool visible;

        /// <summary>
        /// Whether or not this cell currently contains anything.
        /// </summary>
        [HideInInspector]
        public bool empty;


        public CellContents cellContent;


        public delegate void SwapDelegate(Vector2Int newCellPos);
        public delegate void PopDelegate();

        /// <summary>
        /// Call this when this cell's content is being swapped with another.
        /// </summary>
        public SwapDelegate Swap;

        /// <summary>
        /// Call this when this cell's content are being "popped", eg when a match is made.
        /// </summary>
        public PopDelegate Pop;


        public static CellContents GetRandomCellContent()
        {
            return 0;
        }
    }

    public enum CellContents
    {
        hole = -1,
        empty = 0,
        candy_blue = 1,
        candy_red = 2,
        candy_green = 3,
        candy_orange = 4,
        candy_yellow = 5
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
                // find better way to get the swipe direction
                // this is based soleley on angle and it's not ideal
                Vector2 dir = (worldPosition - startWorldposition).normalized;
                return new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y)) ;
            }
        }

        public Vector2 endWorldPosition;
        public Vector2Int endGridPosition;

    }


    
    #endregion
}

