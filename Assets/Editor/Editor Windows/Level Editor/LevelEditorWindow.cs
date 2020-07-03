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
    Vector2Int selectedCell;

    enum Tools
    {
        none,
        hole,
        reset,
        candy_blue
    }

    Tools activeTool;

    


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
            

        //SerializedProperty gridProperty = serializedLevel.FindProperty("grid");
        //gridProperty.ClearArray();
        //gridProperty.arraySize = gridWidth * gridHeight;
        //
        //for (int i = 0; i < gridProperty.arraySize; ++i)
        //{
        //    gridProperty.InsertArrayElementAtIndex(i);
        //
        //    SerializedProperty serializedCell = gridProperty.GetArrayElementAtIndex(i);
        //
        //    // i = y * height + x
        //
        //    int gridX = 0; // i / gridHeight;
        //    int gridY = 0; // i % gridWidth;
        //
        //
        //    serializedCell.FindPropertyRelative("x").intValue = grid[gridX, gridY].x;
        //    serializedCell.FindPropertyRelative("y").intValue = grid[gridX, gridY].y;
        //    
        //    serializedCell.FindPropertyRelative("hole").boolValue = grid[gridX, gridY].hole;
        //
        //    serializedCell.FindPropertyRelative("presetContent").boolValue = grid[gridX, gridY].presetContent;
        //
        //    serializedCell.FindPropertyRelative("content").enumValueIndex = (int)grid[gridX, gridY].content;
        //}
           
       
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
                Save();
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
                Save();
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
                Save();
            }
            SetupGrid();
        });



        ToolbarButton holeToolButton = root.Q<ToolbarButton>("tool-hole");
        holeToolButton.clickable.clicked += () =>
        {
            Toolbar toolbar = root.Q<Toolbar>("toolbar");
            foreach (VisualElement e in toolbar.Children())
            {
                e.RemoveFromClassList("tool-active");
                e.AddToClassList("tool-inactive");
            }

            holeToolButton.AddToClassList("tool-active");
            holeToolButton.RemoveFromClassList("tool-inactive");
            activeTool = Tools.hole;

            
        };

        


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

                // TOOL HANDLING
                cell.RegisterCallback<MouseDownEvent>(evt =>
                {
                    //it's dumb but whatevs
                    int x = int.Parse(cell.name.Split(' ')[0]);
                    int y = int.Parse(cell.name.Split(' ')[1]);

                    

                    if (evt.button == 0) // left click
                    {
                        switch (activeTool)
                        {
                            case Tools.none:
                                break;

                            case Tools.hole:
                                // toggle cell visibility
                                VisualElement bg = cell.Q<VisualElement>("Cell");
                                bg.style.visibility = bg.style.visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

                                grid[x, y].hole = (bg.style.visibility == Visibility.Hidden);
                                break;

                            case Tools.reset:
                                break;

                            case Tools.candy_blue:
                                break;
                        }
                    }

                    if (autosave) Save();
                });


                


                // load cell
                //SerializedProperty gridProperty = serializedLevel.FindProperty("grid");
                ////if (gridProperty.arraySize == gridHeight * gridWidth)
                ////{
                ////Debug.Log("loading cell");
                //
                ////Debug.Log(j * gridHeight + i);
                //SerializedProperty serializedCell = gridProperty.GetArrayElementAtIndex(j * gridHeight + i);
                //
                //grid[i, j].x = serializedCell.FindPropertyRelative("x").intValue;
                //grid[i, j].y = serializedCell.FindPropertyRelative("y").intValue;
                //
                //
                //grid[i, j].hole = serializedCell.FindPropertyRelative("hole").boolValue;
                //grid[i, j].presetContent = serializedCell.FindPropertyRelative("presetContent").boolValue;
                //grid[i, j].content = (LevelEditorCellContent)serializedCell.FindPropertyRelative("content").enumValueIndex;
                //
                ////}
                //
                //
                //if (grid[i, j].hole)
                //{
                //    cell.style.visibility = Visibility.Hidden;
                //}



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
