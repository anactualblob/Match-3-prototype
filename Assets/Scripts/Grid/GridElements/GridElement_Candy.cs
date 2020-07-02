using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridElement_Candy : GridElement
{
    float swapTweenDuration = 0.2f;
    float popTweenDuration = 0.2f;
    float fallTweenDuration = 0.2f;

    Sequence _masterSequence = null;

    /// <summary>
    /// Property that wraps _masterSequence, setting it to a new sequence if it is null or marked as dirty.
    /// <para>
    /// Also passes anonymous functions to the sequence's OnStart and OnComplete callbacks.
    /// </para>
    /// </summary>
    Sequence MasterSequence
    {
        get
        {
            if (sequenceDirty || _masterSequence == null)
            {
                _masterSequence = DOTween.Sequence();
                sequenceDirty = false;

                _masterSequence.OnStart(() =>
                {
                    GridDisplayer.NotifyTweenStart(this);
                });

                _masterSequence.OnComplete(() =>
                {
                    GridDisplayer.NotifyTweenEnd(this);
                    wasSwapped = false;
                    sequenceDirty = true;
                });
            }

            return _masterSequence;
        }
    }


    bool sequenceDirty = false;
    bool wasSwapped = false;
    bool registered = false;

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




    private void Start()
    {
        DOTween.Init();
    }




    public override void OnSwap(Vector2Int newCellPos)
    {
        // deregister methods from the original cell
        DeRegisterMethodsFromCell(x, y);

        // register methods on the new cell
        RegisterMethodsOnCell(newCellPos);

        // set x and y to new cell
        x = newCellPos.x;
        y = newCellPos.y;

        // tween position
        MasterSequence.Append(transform.DOMove(GridDisplayer.GridToWorld(x, y), swapTweenDuration));

        wasSwapped = true;
    }



    public override void OnSwapFail(Vector2Int newCellPos)
    {
        // move to the new position then back to the old one
        MasterSequence.Append(transform.DOMove(GridDisplayer.GridToWorld(newCellPos), swapTweenDuration));
        MasterSequence.Append(transform.DOMove(GridDisplayer.GridToWorld(x, y), swapTweenDuration));
    }



    public override void OnPop()
    {
        // spawn particles ?
        // sound effects ?
        // etc.


        // if this cell is part of a match but isn't the one that was swapped this frame, it 
        //    needs to wait for the swap tween to end before disappearing
        if (!wasSwapped)
            MasterSequence.AppendInterval(swapTweenDuration);

        MasterSequence.Append( transform.DOScale(0, popTweenDuration).SetEase(Ease.InBack) );
        MasterSequence.AppendCallback(() => GridDisplayer.ReturnCandyToPool(this)) ;

        // scale the game object back to 1 after returning it so that it's scaled properly when we get it back from the pool again
        MasterSequence.AppendCallback( () => transform.localScale = new Vector3(1, 1, 1));
    }



    public override void OnFall(int fallHeight)
    {
        DeRegisterMethodsFromCell(x, y);
        RegisterMethodsOnCell(x, y+fallHeight);
        
        y += fallHeight;

        MasterSequence.Append( transform.DOMove( GridDisplayer.GridToWorld(x, y), fallTweenDuration).SetEase(Ease.OutBounce) );
    }






    public void RegisterMethodsOnCell(int x, int y)
    {
        if (!registered)
        {
            GridManager.GRID[x, y].Pop += this.OnPop;
            GridManager.GRID[x, y].Swap += this.OnSwap;
            GridManager.GRID[x, y].SwapFail += this.OnSwapFail;
            GridManager.GRID[x, y].Fall += this.OnFall;

            registered = true;
        }
        else
        {
            Debug.LogError("GridElement_Candy.cs : Couldn't register methods on cell because this candy is already registered on another cell.", this);
        }
    }

    public void RegisterMethodsOnCell(Vector2Int cellPos)
    {
        RegisterMethodsOnCell(cellPos.x, cellPos.y);
    }

    public void DeRegisterMethodsFromCell(int x, int y)
    {
        if (registered)
        {
            GridManager.GRID[x, y].Pop -= this.OnPop;
            GridManager.GRID[x, y].Swap -= this.OnSwap;
            GridManager.GRID[x, y].SwapFail -= this.OnSwapFail;
            GridManager.GRID[x, y].Fall -= this.OnFall;

            registered = false;
        }
        else
        {
            Debug.LogError("GridElement_Candy.cs : Couldn't deregister methods from cell because this candy is no registered on any cell.", this);
        }
    }

    public void DeRegisterMethodsFromCell(Vector2Int cellPos)
    {
        DeRegisterMethodsFromCell(cellPos.x, cellPos.y);
    }






    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        // need to check if this isn't null because the GameObjects are disabled before the grid 
        //   is created, since object pools are created in awake and the grid is setup in start
        if (GridManager.GRID != null)
        {
            DeRegisterMethodsFromCell(x, y);
        }
        
    }
}
