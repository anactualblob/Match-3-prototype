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



    [SerializeField] LevelScriptableObject level = null;


    TouchInfo primaryTouchInfo;
    Vector2Int selectedCell;
    bool cellSelected;
    Vector2Int swipedCell;
    bool shouldGetSwipedCell = false;
    //bool swipeHappenedThisTouch = false;


    public static bool LevelLoaded
    {
        get
        {
            return !(S.level == null);
        }
    }
    
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
        // if we're currently animating, don't update grid state until the display of the grid is caught up ith the current grid state.
        if (GridDisplayer.TweenInProgress())
        {
            return;
        }

        if (primaryTouchInfo.touching && !cellSelected)
        {
            // user is touching inside the grid
            if (primaryTouchInfo.startGridPosition != new Vector2Int(int.MinValue, int.MinValue)) 
            {
                selectedCell = primaryTouchInfo.startGridPosition;
                cellSelected = true;
                Debug.Log(grid[selectedCell.x, selectedCell.y].cellContent);
            }
        }

        if (!primaryTouchInfo.touching && cellSelected)
        {
            cellSelected = false;
        }

        #region Candy Swapping
        if (shouldGetSwipedCell)
        {
            // user has touched inside the grid
            if (primaryTouchInfo.startGridPosition != new Vector2Int(int.MinValue, int.MinValue))
            {
                int x = primaryTouchInfo.startGridPosition.x + primaryTouchInfo.swipeDirection.x;
                int y = primaryTouchInfo.startGridPosition.y - primaryTouchInfo.swipeDirection.y;

                // user is swiping inside the grid
                if (x < GRID_WIDTH && y < GRID_HEIGHT && x >= 0 && y >= 0)
                {

                    // the cell the player is swiping
                    swipedCell = new Vector2Int(x,y);

                    // swap the cellContent variables of the cells being swapped
                    CellContents temp = grid[swipedCell.x, swipedCell.y].cellContent;
                    grid[swipedCell.x, swipedCell.y].cellContent = grid[selectedCell.x, selectedCell.y].cellContent;
                    grid[selectedCell.x, selectedCell.y].cellContent = temp;

                    // check for matches
                    List<Vector2Int> matches = new List<Vector2Int>();
                    matches.AddRange(GetMatchesAtCell(swipedCell));
                    matches.AddRange(GetMatchesAtCell(selectedCell));
                    
                    if (matches.Count != 0)
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
                            grid[selectedCell.x, selectedCell.y].Swap(swipedCell);

                        if (swipedCellCopy.Swap != null)
                            swipedCellCopy.Swap(selectedCell);


                        foreach (Vector2Int cell in matches)
                        {
                            if (grid[cell.x, cell.y].Pop != null)
                                grid[cell.x, cell.y].Pop();

                            grid[cell.x, cell.y].cellContent = CellContents.empty;
                        }
                        return;
                    }
                    else
                    {
                        // call swap fail event (no need for the above hack since GridElement_Candy's 
                        //    SwapFail method doesn't do any registering/deregistering)
                        if (grid[selectedCell.x, selectedCell.y].SwapFail != null)
                            grid[selectedCell.x, selectedCell.y].SwapFail(swipedCell);

                        if (grid[swipedCell.x, swipedCell.y].SwapFail != null)
                            grid[swipedCell.x, swipedCell.y].SwapFail(selectedCell);


                        // swap cell contents back
                        CellContents temp2 = grid[swipedCell.x, swipedCell.y].cellContent;
                        grid[swipedCell.x, swipedCell.y].cellContent = grid[selectedCell.x, selectedCell.y].cellContent;
                        grid[selectedCell.x, selectedCell.y].cellContent = temp2;
                    }
                }
            }

            //shouldGetSwipedCell = false;
        }
        #endregion

        #region Candy Falling
        if ( GridHasEmptyCells() )
        {
            bool candiesFellThisFrame = false;

            // go backwards throught the grid, starting 1 line above the bottom because we're checking if cells below are emtpy
            for (int j = GRID_HEIGHT - 2; j >= 0; --j)
            {
                for (int i = GRID_WIDTH - 1; i >= 0; --i)
                {
                    if (grid[i, j].cellContent == CellContents.hole)
                    {
                        continue;
                    }

                    // if we find a non empty cell above an empty cell or a hole
                    if ( (grid[i, j+1].cellContent == CellContents.empty || grid[i, j + 1].cellContent == CellContents.hole) && grid[i, j].cellContent != CellContents.empty)
                    {
                        int dist = 0;
                        int holeCounter = 0;
                    
                        // go down until we find a cell that's above a non empty cell or at the bottom of the grid
                        for (int y = j+1; y < GRID_HEIGHT; ++y)
                        {
                            if (grid[i, y].cellContent == CellContents.hole)
                            {
                                // if the cell we're checking is a hole, we don't want to add to dist just yet in 
                                //    case its holes all the way down, in which case the candy should not drop, so
                                //    we keep a separate counter for holes
                                holeCounter++;
                                
                            }
                            else
                            {
                                dist++;
                    
                                // if we've hit a cell that's not a hole and the holeCounter is greater than 0,
                                //    that means the ccandy can fall down through the holes. we add the holecounter
                                //    to dist and we reset it in case we find more holes lower in the grid.
                                if (holeCounter > 0)
                                {
                                    dist += holeCounter;
                                    holeCounter = 0;
                                }
                            }
                          
                            if (y<GRID_HEIGHT-1)
                            {
                                // break if the cell below the one we're checking is not empty and not a hole
                                if (grid[i, y + 1].cellContent != CellContents.empty && grid[i, y + 1].cellContent != CellContents.hole) break;
                            }
                        }
                    
                        // dist is now the number of empty cells between the cell we're at and the next non-empty cell under it
                        // we call Fall on grid[i,j] and pass it dist

                        if (dist > 0)
                        {
                            if (grid[i, j].Fall != null) grid[i, j].Fall(dist);

                            grid[i, j + dist].cellContent = grid[i, j].cellContent;
                            grid[i, j].cellContent = CellContents.empty;

                            candiesFellThisFrame = true;
                        }
                    }
                }
            }

            if (candiesFellThisFrame) return;


            //for each grid column i, check if grid[i, 0] is empty. if yes, new candies should be spawned.
            for (int i = GRID_WIDTH - 1; i >= 0; --i)
            {
                int topColumnCellYCoord = 0;

                // if the top cell of the column is a hole, go down until we find one that's not
                if (grid[i, topColumnCellYCoord].cellContent == CellContents.hole)
                {
                    // todo
                }

                if (grid[i, topColumnCellYCoord].cellContent == CellContents.empty)
                {
                    // "probe" down to see how far down the next non-empty cell is
                    // Call a GridDisplayer Function to spawn in a new candy in the top row cell
                    // call Fall on this top row cell if the distance to the non-emtpy cell is > 0
                    int dist = 0;
            
                    for (int j = 1; j < GRID_WIDTH; ++j)
                    {
                        if (grid[i, j].cellContent == CellContents.empty)
                        {
                            ++dist;
                        }
                        else
                        {
                            break;
                        }
                    }
            
                    // get a random color for the new candy
                    // FIND A BETTER WAY TO DO THIS
                    CellContents color = (CellContents)Random.Range(1, 6);
            
                    GridDisplayer.SpawnNewCandy(color, new Vector2Int(i, 0));
            
                    if (dist > 0)
                    {
                        if (grid[i, 0].Fall != null) 
                            grid[i, 0].Fall(dist);
            
                        grid[i, dist].cellContent = color;
                    }
                    else
                    {
                        grid[i, 0].cellContent = color;
                    }
                    
                }
            }

            

        }
        #endregion

        // check the whole grid for matches and pop them
        // for some reason this doesn't detect 4+ matches sometimes ?
        for (int i = 0; i < GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GRID_HEIGHT; ++j)
            {
                foreach (Vector2Int cellPos in GetMatchesAtCell(new Vector2Int(i, j)))
                {
                    if (grid[cellPos.x, cellPos.y].Pop != null)
                    {
                        grid[cellPos.x, cellPos.y].Pop();
                    }
                    grid[cellPos.x, cellPos.y].cellContent = CellContents.empty;
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

        Vector2Int[,] directionalMatches = new Vector2Int[4, 2];
        
        for (int i = 0; i < 4; ++i)
            for (int j = 0; j<2; ++j)
                directionalMatches[i, j] = new Vector2Int(-1, -1);

        //bool match4 = false;
        //bool match5 = false;


        List<Vector2Int> matches = new List<Vector2Int>();

        if (cell.cellContent == CellContents.empty || cell.cellContent == CellContents.hole) return matches;


        // find matches of type XOO
        for (int i = 0; i<directions.Length; ++i)
        {
            // if the cell we need to check is outside the grid, skip to the next loop iteration
            if (cell.x + directions[i].x >= GRID_WIDTH || cell.x + directions[i].x < 0 || cell.y + directions[i].y >= GRID_HEIGHT || cell.y + directions[i].y < 0)
            {
                continue;
            }

            GridCell checkedCell = grid[cell.x + directions[i].x, cell.y + directions[i].y];

            if (cell.cellContent == checkedCell.cellContent)
            {
                // fill directionalMatches array in order to check for matches of type OXO later
                directionalMatches[i, 0] = new Vector2Int(checkedCell.x, checkedCell.y);

                if (checkedCell.x + directions[i].x >= GRID_WIDTH || checkedCell.x + directions[i].x < 0 || checkedCell.y + directions[i].y >= GRID_HEIGHT || checkedCell.y + directions[i].y < 0)
                {
                    // if the cell we need to check is outside the grid, skip to the next loop iteration
                    continue;
                }

                GridCell thirdCell = grid[checkedCell.x + directions[i].x, checkedCell.y + directions[i].y];
                if (checkedCell.cellContent == thirdCell.cellContent)
                {
                    matches.Add(new Vector2Int(cell.x, cell.y));
                    matches.Add(new Vector2Int(checkedCell.x, checkedCell.y));
                    matches.Add(new Vector2Int(thirdCell.x, thirdCell.y));

                    // fill directionalMatches array in order to check for matches of type OXO later
                    directionalMatches[i, 1] = new Vector2Int(thirdCell.x, thirdCell.y);
                }
            }
        }



        // Find matches of type OXO

        // The way this works is that we've populated a directionnalMatches array of 4 rows & 2 columns in the code above. 
        // Each row corresponds to a direction, in the order: Up Right Down Left. We check if we stored a match by comparing
        //    an item of the array to a (-1,-1) vector, which we filled the array with after having created it. If it's not
        //    (-1,-1), the item is a match because it has overwritten the default (-1,-1).
        // So we first check if the first items of rows 0 and 2 (directions up and down) are both a match. If yes, that means
        //    it's a match 3 and we add both items + the original cell we're checking to the list of matches (we need to add 
        //    the OG cell because this match wasn't caught by the block above). Then, we check the second items of each row,
        //    because if they're a match then it's a match 4 (or 5 if they're both matches). We add the items to the matches
        //    list accordingly.
        // This makes it so that we can detect both OXO matches, and matches of more than 3. I could maybe have optimized it
        //    or wrapped it in a loop or have thought up a better algorithm, but this works so eh ¯\_(ツ)_/¯
        // Also we do the same again further below but for rows 1 and 3, directions right and left.
        // i've commented out the non loop version and pu in a loop version. it's maybe less readable but also less error prone.
        // if it somehow doesn't work (it seems fine to me), i'll put the non-looping version back in.
        Vector2Int noMatch = new Vector2Int(-1, -1);
        for (int i = 0; i<2; ++i)
        {
            if (directionalMatches[i, 0] != noMatch && directionalMatches[i+2, 0] != noMatch)
            {
                if (!matches.Contains(directionalMatches[i, 0])) matches.Add(directionalMatches[i, 0]);
                if (!matches.Contains(directionalMatches[i+2, 0])) matches.Add(directionalMatches[i+2, 0]);
                if (!matches.Contains(new Vector2Int(cell.x, cell.y))) matches.Add(new Vector2Int(cell.x, cell.y));

                if (directionalMatches[i, 1] != noMatch)
                {
                    //match4 = true;
                    if (!matches.Contains(directionalMatches[i, 1])) matches.Add(directionalMatches[i, 1]);
                }

                if (directionalMatches[i+2, 1] != noMatch)
                {
                    //if (match4) match5 = true;
                    //else match4 = true;

                    if (!matches.Contains(directionalMatches[i+2, 1])) matches.Add(directionalMatches[2, 1]);
                }
            }
        }
        
        // i know the code above is terrible it works don't @ me 
        // also TODO find a way to use match4 and match5 ?

        return matches;
    }

    public List<Vector2Int> GetMatchesAtCell(Vector2Int cellPos)
    {
        return GetMatchesAtCell(grid[cellPos.x, cellPos.y]);
    }

    public bool GridHasEmptyCells()
    {
        foreach (GridCell cell in GRID)
        {
            if (cell.cellContent == CellContents.empty)
            {
                return true;
            }
        }

        return false;
    }



    #region Grid Setup
    [ContextMenu("Initialize Grid")]
    public void SetupGrid()
    {
        GridDisplayer.TeardownGridDisplay();

        if (level == null)
        {
            // create random grid
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

        }
        else
        {
            // load info from level
            GRID_WIDTH = level.gridWidth;
            GRID_HEIGHT = level.gridHeight;

            grid = new GridCell[GRID_WIDTH, GRID_HEIGHT];

            // go through level.grid, a 1D array
            for (int i = 0; i < GRID_WIDTH * GRID_HEIGHT; ++i)
            {
                // find x and y 2D grid positions of the cell at i with the formula : i = x * gridHeight + y
                int gridX = i / gridHeight;
                int gridY = i % gridHeight;

                grid[gridX, gridY] = new GridCell()
                {
                    visible = true,
                    empty = false,
                    x = gridX,
                    y = gridY
                };

                // cell contents
                if (level.grid[i].hole)
                {
                    grid[gridX, gridY].cellContent = CellContents.hole;
                }
                else 
                {
                    // if the current cell is empty in the level SO, it hasn't been defined and we should fill it with a random color
                    if (level.grid[i].content == CellContents.empty)
                    {
                        grid[gridX, gridY].cellContent = level.GetRandomColor();
                    }
                    else
                    {
                        grid[gridX, gridY].cellContent = level.grid[i].content;
                    }
                }
                
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
                
                if (level != null)
                {
                    grid[cell.x, cell.y].cellContent = level.GetRandomColor();
                }
                else
                {
                    grid[cell.x, cell.y].cellContent = (CellContents)Random.Range(1, 6);
                }

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

        //if ((touchInfo.gridPosition - touchInfo.startGridPosition).magnitude >= 1 && !S.swipeHappenedThisTouch)
        //{
        //    S.shouldGetSwipedCell = true;
        //    S.swipeHappenedThisTouch = true;
        //}
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
        //S.swipeHappenedThisTouch = false;
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
        public delegate void FallDelegate(int height);
        public delegate void PopDelegate();

        /// <summary>
        /// Call this when this cell's content is being swapped with another.
        /// </summary>
        public SwapDelegate Swap;

        /// <summary>
        /// Call this when the user tries to swap this cell's content with another, but the swap wouldn't result in a match and must be reversed.
        /// </summary>
        public SwapDelegate SwapFail;

        public FallDelegate Fall;

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
                Vector2 dir = (worldPosition - startWorldposition).normalized;
                Vector2Int intDir = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

                // if the vector doesn't have a grater than 1 magnitude (that is if it's not diagonal)
                if (intDir.magnitude <= 1)
                {
                    return intDir;
                }
                
                return Vector2Int.zero;
            }
        }

        public Vector2 endWorldPosition;
        public Vector2Int endGridPosition;

    }


    
    #endregion
}

