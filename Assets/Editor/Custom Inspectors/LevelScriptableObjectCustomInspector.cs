using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

[CustomEditor(typeof(LevelScriptableObject))]
public class LevelScriptableObjectCustomInspector : Editor
{
    // this custom inspector should only display the grid and provide a button 
    //    to open the Level Editor window and pass it the inspected Level instance

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
        inspectedLevel = target as LevelScriptableObject;


        root = new VisualElement();

        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uss");

        cellTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Templates/LevelCell.uxml");

        root.styleSheets.Add(stylesheet);
    }



    public override VisualElement CreateInspectorGUI()
    {
        root.Clear();
        tree.CloneTree(root);

        Button editButton = root.Q<Button>(name = "open-editor-button");

        editButton.clickable.clicked += () => LevelEditorWindow.EditLevel(inspectedLevel);

        width = inspectedLevel.gridWidth;
        height = inspectedLevel.gridHeight;
        levelName = inspectedLevel.levelName;

        root.Q<Label>("width-value").text = width.ToString();
        root.Q<Label>("height-value").text = height.ToString();
        root.Q<Label>("name-value").text = levelName;

        CreateGrid();

        return root;
    }


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
                cellTemplate.CloneTree(cell);

                cell.name = i + " " + j;


                row.Add(cell);


                // set the display of the cell visual element according to the cell in the grid array
                // position of the cell in the array given by : (j * height + i)
                //if (grid[j * height + i].hole)
                //{
                //    VisualElement bg = cell.Q<VisualElement>("Cell");
                //    bg.style.visibility = Visibility.Hidden;
                //}

            }
            cellContainer.Add(row);
        }
    }


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
