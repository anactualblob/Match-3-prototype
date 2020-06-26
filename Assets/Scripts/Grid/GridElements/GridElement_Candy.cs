using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridElement_Candy : GridElement
{
    float swapTweenDuration = 0.2f;
    float popTweenDuration = 0.2f;

    Sequence masterSequence = null;

    bool sequenceDirty = false;
    bool wasSwapped = false;

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

        // deregister onSwap & OnPop from the original cell
        GridManager.GRID[x, y].Pop -= this.OnPop;
        GridManager.GRID[x, y].Swap -= this.OnSwap;


        // register onSwap and OnPop on the new cell
        GridManager.GRID[newCellPos.x, newCellPos.y].Pop += this.OnPop;
        GridManager.GRID[newCellPos.x, newCellPos.y].Swap += this.OnSwap;

        // set x and y to new cell
        x = newCellPos.x;
        y = newCellPos.y;

        // tween position
        DOTweenMasterSequenceInit();

        masterSequence.Append(transform.DOMove(GridDisplayer.GridToWorld(x, y), swapTweenDuration));

        wasSwapped = true;
    }



    public override void OnSwapFail(Vector2Int newCellPos)
    {
        //notify the GridDisplayer that we're starting to tween
        //GridDisplayer.NotifyTweenStart(this);

        //craft the tween sequence
        DOTweenMasterSequenceInit();

        masterSequence.Append(transform.DOMove(GridDisplayer.GridToWorld(newCellPos), swapTweenDuration))
            .Append(transform.DOMove(GridDisplayer.GridToWorld(x, y), swapTweenDuration));
    }



    public override void OnPop()
    {
        // spawn particles ?
        // sound effects ?
        // etc.


        DOTweenMasterSequenceInit();

        // if this cell is part of a match but isn't the one that was swapped this frame, it 
        //    needs to wait for the swap tween to end before disappearing
        if (!wasSwapped)
            masterSequence.AppendInterval(swapTweenDuration);

        masterSequence.Append(transform.DOScale(0, popTweenDuration));
        masterSequence.AppendCallback(() => GridDisplayer.ReturnCandyToPool(this));
    }


    /// <summary>
    /// This function  must be called before attemtping to append tweens to the masterSequence. It initiates the masterSequence if it's null or if it is marked as dirty.
    /// <para>It also registers GridDisplayer NotifyTweenStart/End on the masterSequence's OnStart and OnComplete, respectively.</para>
    /// </summary>
    void DOTweenMasterSequenceInit()
    {
        if (sequenceDirty || masterSequence == null)
        {
            masterSequence = DOTween.Sequence();
            sequenceDirty = false;

            masterSequence.OnStart(() =>
            {
                GridDisplayer.NotifyTweenStart(this);
            });

            masterSequence.OnComplete(() =>
            {
                GridDisplayer.NotifyTweenEnd(this);
                wasSwapped = false;
                sequenceDirty = true;
            });
        }
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
            GridManager.GRID[x, y].Pop -= this.OnPop;
            GridManager.GRID[x, y].Swap -= this.OnSwap;
            GridManager.GRID[x, y].SwapFail -= this.OnSwapFail;
        }
        
    }
}
