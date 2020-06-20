using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridElement : MonoBehaviour
{
    /// <summary>
    /// x position of the GridElement in the grid.
    /// </summary>
    public int x;

    /// <summary>
    /// y position of the GridElement in the grid.
    /// </summary>
    public int y;

    /// <summary>
    /// Called when the cell this GridElement is in is swapped with another.
    /// </summary>
    /// <param name="newCellPos">The position of the cell this GridElement is being swapped with.</param>
    public abstract void OnSwap(Vector2Int newCellPos);

    /// <summary>
    /// Called when this GridElement must disappear from the grid, e.g. when a match is made.
    /// </summary>
    public abstract void OnPop();



    
}
