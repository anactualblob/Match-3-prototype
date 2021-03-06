﻿using System.Collections.Generic;
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
    [Header("Display Parameters")]
    [SerializeField] float verticalMargin;
    [SerializeField] float horizontalMargin;
    [Space]
    [SerializeField] Vector2 offsetFromCenter;
    float cellSize = 1;
    [Space]
    [SerializeField] GameObject gridCellBackgroundPrefab;
    [Space]
    [Tooltip("Size of candies in proportion of the size of the cell background. where 1 = size of cell background")]
    [SerializeField] [Range(0, 1)] float candySize;


    [Header("Candy prefabs")]
    [SerializeField] GameObject candyBlue;
    [SerializeField] GameObject candyRed;
    [SerializeField] GameObject candyOrange;
    [SerializeField] GameObject candyYellow;
    [SerializeField] GameObject candyGreen;


    [Header("Candy Pool Parameters")]
    [Tooltip("What proportion of the total cell count should serve as the number of pooled objects? 1 = each pool has as many objects as there are cells in the grid, 0.5 = half as much, etc.")]
    [SerializeField] float totalCellsToPoolCandiesRatio;
    int numberOfPooledCandies;
    CandyPool redCandyPool,
        blueCandyPool,
        orangeCandyPool,
        greenCandyPool,
        yellowCandyPool;


    SpriteRenderer gridBackgroundSprite;
    Transform cellContainer;
    Transform gridElementContainer;
    Transform poolContainer;


    List<GridElement_Candy> tweeningCandies = new List<GridElement_Candy>();


    private void Awake()
    {
        S = this;

        gridBackgroundSprite = transform.Find("GridBackground").GetComponent<SpriteRenderer>();
        if (gridBackgroundSprite == null) Debug.LogError("GridDisplayer.cs : gridBackgroundSprite couldn't be found. Is there a SpriteRenderer on a child of this object?");

        cellContainer = transform.Find("CellContainer");
        if (cellContainer == null) Debug.LogError("GridDisplayer.cs : GridContainer object couldn't be found in children of this object.");

        gridElementContainer = transform.Find("GridElementContainer");
        if (gridElementContainer == null) Debug.LogError("GridManager.cs : GridElementContainer object couldn't be found in children of this object.");

        poolContainer = transform.Find("PoolContainer");
        if (poolContainer == null) Debug.LogError("GridManager.cs : PoolContainer object couldn't be found in children of this object.");

        if (gridCellBackgroundPrefab == null) Debug.LogError("GridDisplayer.cs : The grid cell prefab is null. Has it been assigned in the inspector?");
    }

    private void InitializeCandyPools(int nbCandies)
    {
        redCandyPool = new CandyPool(candyRed, numberOfPooledCandies, poolContainer);
        blueCandyPool = new CandyPool(candyBlue, numberOfPooledCandies, poolContainer);
        orangeCandyPool = new CandyPool(candyOrange, numberOfPooledCandies, poolContainer);
        greenCandyPool = new CandyPool(candyGreen, numberOfPooledCandies, poolContainer);
        yellowCandyPool = new CandyPool(candyYellow, numberOfPooledCandies, poolContainer);
    }

    private void Start()
    {

    }



    #region Gird Display Initialization & Teardown
    void GridDisplayInit()
    {
        numberOfPooledCandies = Mathf.RoundToInt((GridManager.GRID_HEIGHT * GridManager.GRID_WIDTH) * totalCellsToPoolCandiesRatio);

        InitializeCandyPools(numberOfPooledCandies);


        float hCellSize = (gridBackgroundSprite.size.x - (horizontalMargin * 2)) / GridManager.GRID_WIDTH;
        float vCellSize = (gridBackgroundSprite.size.y - (verticalMargin * 2)) / GridManager.GRID_HEIGHT;

        cellSize = Mathf.Min(hCellSize, vCellSize);

        GameObject current = null;

        for (int i = 0; i < GridManager.GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GridManager.GRID_HEIGHT; ++j)
            {
                current = null; 

                if (GridManager.GRID[i, j].cellContent != GridManager.CellContents.hole)
                {
                    // instantiate cells and set their sizes
                    // maybe use a pool for cell backgrounds ? (no need probably)
                    GameObject cell = Instantiate(gridCellBackgroundPrefab, GridToWorld(i, j), Quaternion.identity, cellContainer);
                    cell.GetComponent<SpriteRenderer>().size = new Vector2(cellSize, cellSize);
                }


                switch (GridManager.GRID[i, j].cellContent)
                {
                    case GridManager.CellContents.hole:
                        break;
                    case GridManager.CellContents.empty:
                        break;


                    case GridManager.CellContents.candy_blue:
                        current = blueCandyPool.TakeFromPool(true, gridElementContainer);
                        break;

                    case GridManager.CellContents.candy_red:
                        current = redCandyPool.TakeFromPool(true, gridElementContainer);
                        break;

                    case GridManager.CellContents.candy_green:
                        current = greenCandyPool.TakeFromPool(true, gridElementContainer);
                        break;

                    case GridManager.CellContents.candy_orange:
                        current = orangeCandyPool.TakeFromPool(true, gridElementContainer);
                        break;

                    case GridManager.CellContents.candy_yellow:
                        current = yellowCandyPool.TakeFromPool(true, gridElementContainer);
                        break;
                }

                if (current != null)
                {
                    current.transform.position = GridToWorld(i, j);
                    current.transform.rotation = Quaternion.identity;

                    current.transform.localScale = new Vector3(cellSize * candySize, cellSize * candySize, cellSize * candySize);

                    GridElement_Candy gridElementComponent = current.GetComponent<GridElement_Candy>();

                    // wire the GridElement methods to the GridCell delegates
                    gridElementComponent.RegisterMethodsOnCell(i, j);

                    gridElementComponent.x = i;
                    gridElementComponent.y = j;
                }
            }
        }
    }

    void GridDisplayTeardown()
    {
        //// return all grid gameobjects to their pools, and destroy cell backgrounds.
        //foreach (Transform t in gridElementContainer)
        //{
        //    ReturnCandyToPool( t.GetComponent<GridElement_Candy>() );
        //    Debug.Log("returned to pool");
        //}
        //
        //foreach (Transform t in cellContainer)
        //{
        //    Destroy(t.gameObject);
        //}
    }

    // static wrapper for calling GridDisplayInit from external scripts
    public static void InitializeGridDisplay()
    {
        S.GridDisplayInit();
    }

    // static wrapper for calling GridDisplayTeardown from external scripts
    public static void TeardownGridDisplay()
    {
        S.GridDisplayTeardown();
    }
    #endregion




    public static void SpawnNewCandy(GridManager.CellContents candyType, Vector2Int gridPosition)
    {
        if (candyType == GridManager.CellContents.empty || candyType == GridManager.CellContents.hole)
        {
            Debug.LogError("GridDisplayer.cs : Couldn't spawn new candy because given CellContents value is not a candy.");
            return;
        }

        GameObject newCandy = null;

        switch (candyType)
        {
            case GridManager.CellContents.candy_blue:
                newCandy = S.blueCandyPool.TakeFromPool(true, S.gridElementContainer);
                break;

            case GridManager.CellContents.candy_red:
                newCandy = S.redCandyPool.TakeFromPool(true, S.gridElementContainer);
                break;

            case GridManager.CellContents.candy_green:
                newCandy = S.greenCandyPool.TakeFromPool(true, S.gridElementContainer);
                break;

            case GridManager.CellContents.candy_orange:
                newCandy = S.orangeCandyPool.TakeFromPool(true, S.gridElementContainer);
                break;

            case GridManager.CellContents.candy_yellow:
                newCandy = S.yellowCandyPool.TakeFromPool(true, S.gridElementContainer);
                break;
        }


        newCandy.transform.position = GridToWorld(gridPosition);
        newCandy.transform.rotation = Quaternion.identity;
        newCandy.transform.localScale = new Vector3(S.cellSize * S.candySize, S.cellSize * S.candySize, S.cellSize * S.candySize);

        GridElement_Candy gridElementComponent = newCandy.GetComponent<GridElement_Candy>();

        // wire the GridElement methods to the GridCell delegates
        gridElementComponent.RegisterMethodsOnCell(gridPosition);

        gridElementComponent.x = gridPosition.x;
        gridElementComponent.y = gridPosition.y;
    }


    /// <summary>
    /// This static function returns a GridElement_Candy to its pool according to its color variable.
    /// </summary>
    /// <param name="candy"></param>
    public static void ReturnCandyToPool(GridElement_Candy candy)
    {
        CandyPool pool = null;
        switch (candy.color)
        {
            case GridElement_Candy.CandyColor.red:
                pool = S.redCandyPool;
                break;

            case GridElement_Candy.CandyColor.blue:
                pool = S.blueCandyPool;
                break;

            case GridElement_Candy.CandyColor.green:
                pool = S.greenCandyPool;
                break;

            case GridElement_Candy.CandyColor.yellow:
                pool = S.yellowCandyPool;
                break;

            case GridElement_Candy.CandyColor.orange:
                pool = S.orangeCandyPool;
                break;

            default:
                Debug.LogError("GridDisplayer.cs : The candy you are trying to return to its pool has an unrecognized color.", candy.gameObject);
                break;
        }

        if (pool != null) pool.ReturnToPool(candy.gameObject);
    }


    #region Candy Tween Tracking
    public static void NotifyTweenStart(GridElement_Candy candy)
    {
        S.tweeningCandies.Add(candy);
    }

    public static void NotifyTweenEnd(GridElement_Candy candy)
    {
        S.tweeningCandies.Remove(candy);
    }

    public static bool TweenInProgress()
    {
        return (S.tweeningCandies.Count > 0);
    }
    #endregion


    #region Grid helper functions
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


    #region Candy Object Pool
    class CandyPool
    {
        //GameObject[] pool;
        //int index;

        //List<GameObject> inactive = new List<GameObject>();
        List<GameObject> activeList = new List<GameObject>();
        Stack<GameObject> inactiveStack = new Stack<GameObject>();

        Transform container;


        /// <summary>
        /// Create a pool of a given Candy prefab.
        /// </summary>
        /// <param name="objectToPool"></param>
        /// <param name="numberOfPooledObjects"></param>
        public CandyPool(GameObject objectToPool, int numberOfPooledObjects, Transform objectsContainer = null)
        {
            //pool = new GameObject[numberOfPooledObjects];
            container = objectsContainer;

            GameObject current;
            for (int i = 0; i < numberOfPooledObjects; ++i)
            {
                current = null;
                current = Instantiate(objectToPool, Vector3.zero, Quaternion.identity);
                current.transform.SetParent(container);

                current.SetActive(false);

                //pool[i] = current;
                inactiveStack.Push(current);
            }


            //index = 0;
        }

        /// <summary>
        /// Get a GameObject from this pool if there are any available.
        /// </summary>
        /// <param name="activate">Whether or not the returned GameObject should already be active. Set to true by default.</param>
        /// <param name="parent">What transform the returned GameObject should be parented to. Set to null by default.</param>
        /// <returns>A GameObject from the pool, or null if there are none available.</returns>
        public GameObject TakeFromPool(bool activate = true, Transform parent = null)
        {
            if (/*index >= pool.Length*/ inactiveStack.Count <= 0)
            {
                Debug.LogError("CandyPool : Trying to get a new object from pool but there is no more.");
                return null;
            }

            //GameObject ret = pool[index];
            //++index;

            GameObject ret = inactiveStack.Pop();
            activeList.Add(ret);


            if (ret == null)
            {
                Debug.LogError("CandyPool : Can't take from pool because the GameObject at the given index is null. Has the pool been properly initialized?");
                return null;
            }

            ret.SetActive(activate);
            if (parent != null)
                ret.transform.parent = parent;

            return ret;
        }
        
        /// <summary>
        /// Return an object to the pool, making it available again. Resets its parent transform and deactivates it.
        /// </summary>
        /// <param name="returnedObject">The object to return to the pool.</param>
        public void ReturnToPool(GameObject returnedObject)
        {
            //--index;

            if (!activeList.Contains(returnedObject))
            {
                Debug.LogError("GridDisplayer.cs : Couldn't return object to pool because it is not in the active list of this pool.", returnedObject);
            }
            else
            {
                activeList.Remove(returnedObject);
            }

            inactiveStack.Push(returnedObject);

            returnedObject.SetActive(false);
            returnedObject.transform.parent = container;
        }

    }


    

    #endregion
}
