using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Blank Level", fileName = "Level")]
public class LevelScriptableObject : ScriptableObject
{
    public int gridHeight;
    public int gridWidth;
    public string levelName;
    public SerializedGridCell[] grid;

    public LevelScriptableObject(int width, int height, SerializedGridCell[] grid)
    {
        this.grid = grid;
        gridHeight = height;
        gridWidth = width;
    }


    [System.Serializable]
    public struct SerializedGridCell
    {
        public int x, y;

        public bool hole;

        public bool presetContent;
        public GridManager.CellContents content;
    }
}
