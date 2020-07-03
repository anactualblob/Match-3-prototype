using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class LevelEditorWindow : EditorWindow
{
#pragma warning disable 0649
    int gridHeight;
    int gridWidth;

    bool autosave;
    new string name;

    EditableGridCell[,] grid;



    LevelScriptableObject editedLevel;

    // Serialized Object, used to serialize the scriptable object we're editing
    SerializedObject serializedLevel;



    VisualTreeAsset tree;
    StyleSheet stylesheet;

    VisualTreeAsset cellTemplate;

    public static void EditLevel(LevelScriptableObject levelToEdit)
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>();
        window.serializedLevel = new SerializedObject(levelToEdit);
        window.SetupEditor();
    }

    private void OnEnable()
    {
        VisualElement root = rootVisualElement;

        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uss");

        cellTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Templates/LevelCell.uxml");

        root.styleSheets.Add(stylesheet);
        tree.CloneTree(root);

        //ObjectField levelSO = root.Q<ObjectField>("level-scriptable-object");
        //if (levelSO.value == null)
        //{
        //    root.Q<Label>("no-level-warning").visible = true;
        //}
        //else
        //{
        //    root.Q<Label>("no-level-warning").visible = false;
        //}
    }


    void Save()
    {
        serializedLevel.FindProperty("gridWidth").intValue = gridWidth;
        serializedLevel.FindProperty("gridHeight").intValue = gridHeight;
        serializedLevel.FindProperty("levelName").stringValue = name;

        SerializedProperty gridProperty = serializedLevel.FindProperty("grid");
        for (int i = 0; i < gridWidth * gridHeight; ++i)
        {
            gridProperty.InsertArrayElementAtIndex(i);
        }
            

        serializedLevel.ApplyModifiedProperties();
    }


    void SetupEditor()
    {
        VisualElement root = rootVisualElement;

        // wiring fields, internal variables and serialized properties for level name
        TextField nameField = root.Q<TextField>("name-input");
        nameField.value = name = serializedLevel.FindProperty("levelName").stringValue;
        nameField.RegisterValueChangedCallback(evt =>
        {
            name = evt.newValue;
            if (autosave)
            {
                serializedLevel.FindProperty("levelName").stringValue = evt.newValue;
                serializedLevel.ApplyModifiedProperties();
            }
        });
        

        // wiring fields, internal variables and serialized properties for width
        IntegerField widthField = root.Q<IntegerField>(name = "width-input");
        widthField.value = gridWidth = serializedLevel.FindProperty("gridWidth").intValue;
        widthField.RegisterValueChangedCallback(evt =>
        {
            gridWidth = evt.newValue;
            if (autosave)
            {
                serializedLevel.FindProperty("gridWidth").intValue = evt.newValue;
                serializedLevel.ApplyModifiedProperties();
            }
            SetupGrid();
        });

        // wiring fields, internal variables and serialized properties for height
        IntegerField heightField = root.Q<IntegerField>(name = "height-input");
        heightField.value = gridHeight = serializedLevel.FindProperty("gridHeight").intValue;
        heightField.RegisterValueChangedCallback(evt =>
        {
            gridHeight = evt.newValue;
            if (autosave)
            {
                serializedLevel.FindProperty("gridHeight").intValue = gridHeight;
                serializedLevel.ApplyModifiedProperties();
            }
            SetupGrid();
        });
        
        // toggle autosave bool
        root.Q<Toggle>("autosave-toggle").RegisterValueChangedCallback(evt =>
        {
            root.Q<Button>(name = "save-button").visible = !evt.newValue;
            autosave = evt.newValue;
        });

        // save changes to serializedLevel when clicking the save button
        root.Q<Button>(name = "save-button").clickable.clicked += () => Save();

        SetupGrid();
    }


    void SetupGrid()
    {
        grid = new EditableGridCell[gridWidth, gridHeight];

        VisualElement cellContainer = rootVisualElement.Q<VisualElement>("grid");
        cellContainer.Clear();
    
        for (int j= 0; j < gridHeight; ++j)
        {

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            for (int i = 0; i < gridWidth; ++i)
            {
                VisualElement cell = new VisualElement();
                cellTemplate.CloneTree(cell);

                cell.name = i + " " + j;

                row.Add(cell);



                //make new grid cell
                grid[i, j] = new EditableGridCell()
                {
                    x = i,
                    y = j,

                    presetContent = false,
                    content = LevelEditorCellContent.random,
                    hole = false,

                    displayCell = cell
                };

                


            }
            cellContainer.Add(row);
        }
    }


    //[MenuItem("Tools/Level Editor", priority = 50)]
    //public static void ShowWindow()
    //{
    //    LevelEditorWindow window = GetWindow<LevelEditorWindow>();
    //
    //    window.titleContent = new GUIContent("Level Editor");
    //
    //    window.minSize = new Vector2(250, 50);
    //
    //
    //}

    struct EditableGridCell
    {
        public int x, y;

        public bool hole;

        public bool presetContent;
        public LevelEditorCellContent content;

        public VisualElement displayCell;


    }

    enum LevelEditorCellContent
    {
        random = 0,
        candy_blue = 1,
        candy_red = 2,
        candy_green = 3,
        candy_orange = 4,
        candy_yellow = 5
    }
}
