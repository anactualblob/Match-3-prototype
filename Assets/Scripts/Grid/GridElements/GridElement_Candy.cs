using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement_Candy : GridElement
{
    public override void FetchPositionFromGridManager()
    {
        throw new System.NotImplementedException();
    }


   

    public override void SetPosition(Vector2 worldPos)
    {
        transform.position = worldPos;
    }

    public enum CandyColor
    {
        red,
        blue,
        green,
        yellow,
        orange
    }


    public CandyColor color;

}
