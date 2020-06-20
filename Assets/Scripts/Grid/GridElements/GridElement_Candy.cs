using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement_Candy : GridElement
{
    
    // possibly useless now
    public enum CandyColor
    {
        red,
        blue,
        green,
        yellow,
        orange
    }
    public CandyColor color;


    public override void OnSwap(Vector2Int newCellPos)
    {
        // deregister onSwap & OnPop from the original cell
        GridManager.GRID[x, y].Pop -= this.OnPop;
        GridManager.GRID[x, y].Swap -= this.OnSwap;

        // register onSwap and OnPop on the new cell
        // PROBLEM : this makes the first object swap 2 times, because it registers on the other 
        //     cell's Swap delegate, which is called right after and thus calls this again.
        GridManager.GRID[newCellPos.x, newCellPos.y].Pop += this.OnPop;
        GridManager.GRID[newCellPos.x, newCellPos.y].Swap += this.OnSwap;

        // set x and y to new cell
        x = newCellPos.x;
        y = newCellPos.y;

        // set position (tween here)
        transform.position = GridDisplayer.GridToWorld(x, y);
    }



    public override void OnPop()
    {
        // disappear
        // spawn aprticles ?
        // sound effects ?
        // etc.
    }



    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        // need to check if this isn't null because the GameObjects are disabled before the grid 
        //   is created, since object ppols are created in awake and the grid is setup in start
        if (GridManager.GRID != null)
        {
            GridManager.GRID[x, y].Pop -= this.OnPop;
            GridManager.GRID[x, y].Swap -= this.OnSwap;
        }
        
    }

    
}
