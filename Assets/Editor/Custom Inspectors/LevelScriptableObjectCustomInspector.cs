using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

[CustomEditor(typeof(LevelScriptableObject))]
public class LevelScriptableObjectCustomInspector : Editor
{
    // primary data
    LevelScriptableObject inspectedLevel;
    int width, height;
    string levelName;
    LevelScriptableObject.SerializedGridCell[] grid;


    VisualTreeAsset tree;
    StyleSheet stylesheet;
    VisualElement root;

    VisualTreeAsset cellTemplate;


    private void OnEnable()
    {
        // get the inspected LevelScriptableObject
        inspectedLevel = target as LevelScriptableObject;

        root = new VisualElement();

        // load UIElements assets
        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uss");

        cellTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Templates/LevelCell.uxml");

        root.styleSheets.Add(stylesheet);
    }



    public override VisualElement CreateInspectorGUI()
    {
        // display the UIElements assets loaded in OnEnable
        root.Clear();
        tree.CloneTree(root);

        // Add a callback to the inspector button to open a new LevelEditorWindow with the inspectedLevel as an arg
        Button editButton = root.Q<Button>(name = "open-editor-button");
        editButton.clickable.clicked += () => LevelEditorWindow.EditLevel(inspectedLevel);

        // fill the info labels
        width = inspectedLevel.gridWidth;
        height = inspectedLevel.gridHeight;
        levelName = inspectedLevel.levelName;
        root.Q<Label>("width-value").text = width.ToString();
        root.Q<Label>("height-value").text = height.ToString();
        root.Q<Label>("name-value").text = levelName;

        // display the grid
        CreateGrid();

        // return root to display it
        return root;
    }

    /// <summary>
    /// Display the inspectedLevel's grid.
    /// </summary>
    void CreateGrid()
    {
        grid = inspectedLevel.grid;

        VisualElement cellContainer = root.Q<VisualElement>("cells-container");
        cellContainer.Clear();

        for (int j = 0; j < height; ++j)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            for (int i = 0; i < width; ++i)
            {
                VisualElement cell = new VisualElement();

                // clone the cell template we loaded in OnEnable into the new VisualElement
                cellTemplate.CloneTree(cell);

                row.Add(cell);

                // the inspectedLevel's grid is a 1D array, but the grid is 2D
                // the position of the (i,j) cell in the 1D array is given by : (i * height + j)
                if (grid[i * height + j].hole)
                {
                    VisualElement bg = cell.Q<VisualElement>("Cell");
                    bg.style.visibility = Visibility.Hidden;
                }
                else
                {
                    // add other cases for cell display here
                }
            }
            cellContainer.Add(row);
        }
    }


    /// <summary>
    /// this function exectutes everytime we open an asset. If the asset is a LevelScriptableObject, we open it in the LevelEditorWindow. 
    /// Otherwise we return false because we didn't handle the opening.
    /// </summary>
    /// <returns>Whether or not we handled the opening of the asset.</returns>
    [OnOpenAsset()]
    public static bool OpenEditor(int instanceId, int line)
    {
        LevelScriptableObject level = EditorUtility.InstanceIDToObject(instanceId) as LevelScriptableObject;

        if (level != null)
        {
            LevelEditorWindow.EditLevel(level);
            return true;
        }
        return false;
    }
}
