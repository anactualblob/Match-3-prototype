using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridElement : MonoBehaviour
{
    int x, y;

    public abstract void FetchPositionFromGridManager();

    public abstract void SetPosition(Vector2 worldPos);

}
