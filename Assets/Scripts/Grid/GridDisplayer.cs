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
    [Header("Display Parameters")]
    [SerializeField] float verticalMargin;
    [SerializeField] float horizontalMargin;
    [Space]
    [SerializeField] Vector2 offsetFromCenter;
    float cellSize = 1;
    [Space]
    [SerializeField] GameObject gridCellBackgroundPrefab;
    //[SerializeField] SpawnableGridElementsScriptableObject spawnableGridElements;


    [Header("Candy prefabs")]
    [SerializeField] GameObject candyBlue;
    [SerializeField] GameObject candyRed;
    [SerializeField] GameObject candyOrange;
    [SerializeField] GameObject candyYellow;
    [SerializeField] GameObject candyGreen;

    [Header("Candy Pool Parameters")]
    [Tooltip("What proportion of the total cell count should serve as the number of pooled objects? 1 = pools have as many objects as there are cells in the grid, 0.5 = half as much, etc.")]
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
        //if (spawnableGridElements == null) Debug.LogError("GridManager.cs : The spawnableGridElements scriptable object is null. Has it been assigned in the inspector?");


        // Object Pools initialization
        numberOfPooledCandies = Mathf.RoundToInt((GridManager.GRID_HEIGHT * GridManager.GRID_WIDTH) * totalCellsToPoolCandiesRatio);
        
        redCandyPool = new CandyPool(candyRed, numberOfPooledCandies, poolContainer);
        blueCandyPool = new CandyPool(candyBlue, numberOfPooledCandies, poolContainer);
        orangeCandyPool = new CandyPool(candyOrange, numberOfPooledCandies, poolContainer);
        greenCandyPool = new CandyPool(candyGreen, numberOfPooledCandies, poolContainer);
        yellowCandyPool = new CandyPool(candyYellow, numberOfPooledCandies, poolContainer);
    }


    private void Start()
    {

    }

    public void GridDisplayInit()
    {
        float hCellSize = (gridBackgroundSprite.size.x - (horizontalMargin * 2)) / GridManager.GRID_WIDTH;
        float vCellSize = (gridBackgroundSprite.size.y - (verticalMargin * 2)) / GridManager.GRID_HEIGHT;

        cellSize = Mathf.Min(hCellSize, vCellSize);

        GameObject current = null;

        for (int i = 0; i < GridManager.GRID_WIDTH; ++i)
        {
            for (int j = 0; j < GridManager.GRID_HEIGHT; ++j)
            {
                if (GridManager.GRID[i, j].cellContent != GridManager.CellContents.hole)
                {
                    // instantiate cells and set their sizes
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
                }
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


    #region Candy Object Pool
    class CandyPool
    {
        GameObject[] pool;
        int index;

        Transform container;


        /// <summary>
        /// Create a pool of a given Candy prefab.
        /// </summary>
        /// <param name="objectToPool"></param>
        /// <param name="numberOfPooledObjects"></param>
        public CandyPool(GameObject objectToPool, int numberOfPooledObjects, Transform objectsContainer = null)
        {
            pool = new GameObject[numberOfPooledObjects];
            container = objectsContainer;

            GameObject current;
            for (int i = 0; i < numberOfPooledObjects; ++i)
            {
                current = Instantiate(objectToPool, Vector3.zero, Quaternion.identity);
                current.transform.SetParent(container);

                current.SetActive(false);

                pool[i] = current;
            }


            index = 0;


        }

        /// <summary>
        /// Get a GameObject from this pool if there are any available.
        /// </summary>
        /// <param name="activate">Whether or not the returned GameObject should already be active. Set to true by default.</param>
        /// <param name="parent">What transform the returned GameObject should be parented to. Set to null by default.</param>
        /// <returns>A GameObject from the pool, or null if there are none available.</returns>
        public GameObject TakeFromPool(bool activate = true, Transform parent = null)
        {
            if (index >= pool.Length)
            {
                Debug.LogError("CandyPool : Trying to get a new object from pool but there is no more.");
                return null;
            }

            GameObject ret = pool[index];
            ++index;

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
            --index;

            returnedObject.SetActive(false);
            returnedObject.transform.parent = container;
        }

    }

    #endregion
}
