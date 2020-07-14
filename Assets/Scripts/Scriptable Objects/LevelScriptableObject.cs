using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Scriptable Object/Blank Level", fileName = "Level")]
public class LevelScriptableObject : ScriptableObject
{
    public int gridHeight;
    public int gridWidth;
    public string levelName;
    public SerializedGridCell[] grid;

    public bool presetColor_blue;
    public GridManager.CellContents[] presetColors;

    /// <summary>
    /// Returns an allowed random color, that is a color that hasn't been preset in the level editor, as a CellContents enum value.
    /// </summary>
    /// <returns></returns>
    public GridManager.CellContents GetRandomColor()
    {
        GridManager.CellContents val = 0;

        if (presetColors.Length == 0) throw new System.Exception();

        // if all the colors are preset we just return a random value because the loop below would be infinite
        if (presetColors.Length >= 5) return (GridManager.CellContents)Random.Range(1, 6);

        do
        {
            val = (GridManager.CellContents) Random.Range(1, 6);
        }
        while ( presetColors.Contains( val ) );
        
        

        return val;
    }


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
