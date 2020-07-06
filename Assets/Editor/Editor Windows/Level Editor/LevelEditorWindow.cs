using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class LevelEditorWindow : EditorWindow
{
#pragma warning disable 0649

    // core level variables
    int gridHeight;
    int gridWidth;

    bool autosave;
    new string name;

    EditableGridCell[,] grid;

    // tools
    enum Tools
    {
        none,
        hole,
        reset,
        candy_blue
    }
    Tools activeTool;


    bool firstWidthValueChange = true;
    bool firstHeightValueChange = true;
    

    // Serialized Object, used to serialize the scriptable object we're editing
    SerializedObject serializedLevel;

    // UIElements Assets
    VisualTreeAsset tree;
    StyleSheet stylesheet;

    VisualTreeAsset cellTemplate;

    // Entry point
    public static void EditLevel(LevelScriptableObject levelToEdit)
    {
        // calls OnEnable
        LevelEditorWindow window = GetWindow<LevelEditorWindow>();
        window.serializedLevel = new SerializedObject(levelToEdit);

        // Load values from serializedLevel and build the grid
        window.LoadAndSetupGrid();

        // do the binding and stuff
        window.SetupEditor();
    }

    private void OnEnable()
    {
        VisualElement root = rootVisualElement;

        // load basic UIElements assets
        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uss");

        // load the template for the grid cell
        cellTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Templates/LevelCell.uxml");

        root.styleSheets.Add(stylesheet);
        tree.CloneTree(root);
    }

    /// <summary>
    /// Sets base UI field values and registers the appropriate callbacks.
    /// </summary>
    void SetupEditor()
    {
        VisualElement root = rootVisualElement;

        // wire together the level name field and the name internal variable
        TextField nameField = root.Q<TextField>("name-input");
        nameField.value = name;
        nameField.RegisterValueChangedCallback(evt =>
        {
            name = evt.newValue;
            if (autosave) Save();
        });


        // wire together the width field and the width internal variable
        IntegerField widthField = root.Q<IntegerField>("width-input");
        widthField.value = gridWidth;
        widthField.RegisterValueChangedCallback(evt =>
        {
            gridWidth = evt.newValue;
            
            // do not rebuild grid on the first value change, because otherwise it overwrites the loading of the grid
            if(!firstWidthValueChange) RebuildGrid();
            else firstWidthValueChange = false; 

            if (autosave) Save();
        });

        // wire together the height field and the height internal variable
        IntegerField heightField = root.Q<IntegerField>("height-input");
        heightField.value = gridHeight;
        heightField.RegisterValueChangedCallback(evt =>
        {
            gridHeight = evt.newValue;

            // do not rebuild grid on the first value change, because otherwise it overwrites the loading of the grid
            if (!firstHeightValueChange) RebuildGrid(); 
            else firstHeightValueChange = false; 

            if (autosave) Save();
        });


        // "homemade" button toggle for the hole tool
        // sets the "inactive" class on all the toolbar's buttons
        // sets the hole too as the active one if it isn't active, otherwise sets the active tool to none
        // note : consider making this a function with a ToolbarButton arg and a Tools arg to avoid copy pasting
        ToolbarButton holeToolButton = root.Q<ToolbarButton>("tool-hole");
        holeToolButton.clickable.clicked += () =>
        {
            Toolbar toolbar = root.Q<Toolbar>("toolbar");
            foreach (VisualElement e in toolbar.Children())
            {
                e.RemoveFromClassList("tool-active");
                e.AddToClassList("tool-inactive");
            }

            if (activeTool != Tools.hole)
            {
                holeToolButton.AddToClassList("tool-active");
                holeToolButton.RemoveFromClassList("tool-inactive");
                activeTool = Tools.hole;
            }
            else
            {
                activeTool = Tools.none;
            }
        };


        // toggle autosave, and save if it's toggled on
        root.Q<Toggle>("autosave-toggle").RegisterValueChangedCallback(evt =>
        {
            root.Q<Button>("save-button").visible = !evt.newValue;
            autosave = evt.newValue;
            if (autosave) Save();
        });


        // save changes to serializedLevel when clicking the save button
        root.Q<Button>("save-button").clickable.clicked += () => Save();
    }

    /// <summary>
    /// Creates a new grid of empty EditableGridCell instances. Does not load values from serializedLevel.
    /// </summary>
    void RebuildGrid()
    {
        Debug.Log("rebuild grid");
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

                // name the cell with its coordinates (important for the MouseDownEvent callback)
                cell.name = i + " " + j;
                row.Add(cell);

                //make new empty grid cell
                grid[i, j] = new EditableGridCell()
                {
                    x = i,
                    y = j,

                    presetContent = false,
                    content = LevelEditorCellContent.random,
                    hole = false
                };

                // register callback on the cell
                cell.RegisterCallback<MouseDownEvent>(evt => MouseDownCellCallback (evt, cell));
            }
            cellContainer.Add(row);
        }
    }

    /// <summary>
    /// Loads property values from serializedLevel for gridHeight, gridWidth, name. Creates a new grid of 
    /// EditableGridCell structs and fills them with values from serializedLevel's grid.
    /// </summary>
    public void LoadAndSetupGrid()
    {
        name = serializedLevel.FindProperty("levelName").stringValue;
        gridWidth = serializedLevel.FindProperty("gridWidth").intValue;
        gridHeight = serializedLevel.FindProperty("gridHeight").intValue;

        SerializedProperty gridProperty = serializedLevel.FindProperty("grid");

        grid = new EditableGridCell[gridWidth, gridHeight];

        VisualElement cellContainer = rootVisualElement.Q<VisualElement>("grid");
        cellContainer.Clear();

        for (int j = 0; j < gridHeight; ++j)
        {

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            for (int i = 0; i < gridWidth; ++i)
            {
                VisualElement cell = new VisualElement();
                cellTemplate.CloneTree(cell);

                // name the cell with its coordinates (important for the MouseDownEvent callback)
                cell.name = i + " " + j;
                row.Add(cell);

                //make new grid cell
                grid[i, j] = new EditableGridCell();

                //register the callback on the cell
                cell.RegisterCallback<MouseDownEvent>(evt => MouseDownCellCallback(evt, cell));
                               
                // load cell values from SerializedObject
                SerializedProperty serializedCell = gridProperty.GetArrayElementAtIndex(i * gridHeight + j);
                grid[i, j].x = serializedCell.FindPropertyRelative("x").intValue;
                grid[i, j].y = serializedCell.FindPropertyRelative("y").intValue;
                grid[i, j].hole = serializedCell.FindPropertyRelative("hole").boolValue;
                grid[i, j].presetContent = serializedCell.FindPropertyRelative("presetContent").boolValue;
                grid[i, j].content = (LevelEditorCellContent)serializedCell.FindPropertyRelative("content").enumValueIndex;
                
                // if the cell we loaded is a hole, update its display
                if (grid[i, j].hole)
                {
                    VisualElement bg = cell.Q<VisualElement>("Cell");
                    bg.style.visibility = Visibility.Hidden;
                }
            }
            cellContainer.Add(row);
        }
    }

    /// <summary>
    /// Callback registered on MouseDownEvent on every cell of the grid.
    /// <para>
    /// Handles the actions of tools on the cell.
    /// </para>
    /// </summary>
    /// <param name="evt">The MouseDownEvent this callback is listening to.</param>
    /// <param name="cell">The cell this callback is registered on.</param>
    void MouseDownCellCallback(MouseDownEvent evt, VisualElement cell)
    {
        // use the cell's name to get its coordinates
        // it's dirty but whatevs, didn't find another way to do it
        int x = int.Parse(cell.name.Split(' ')[0]);
        int y = int.Parse(cell.name.Split(' ')[1]);

        // if the click was a left click
        if (evt.button == 0) 
        {
            switch (activeTool)
            {
                case Tools.none:
                    break;

                case Tools.hole: // toggle cell visibility
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
    }


    /// <summary>
    /// Apply the editor's state and data to the serializedLevel and save it.
    /// </summary>
    void Save()
    {
        // Set values of serializedLevel's top-level properties
        serializedLevel.FindProperty("gridWidth").intValue = gridWidth;
        serializedLevel.FindProperty("gridHeight").intValue = gridHeight;
        serializedLevel.FindProperty("levelName").stringValue = name;

        // get the grid property from the serializedLevel and clear its contents
        SerializedProperty gridProperty = serializedLevel.FindProperty("grid");
        gridProperty.ClearArray();
        
        // fill the serializedLevel's grid with data from the window's grid[,]
        for (int i = 0; i < gridWidth * gridHeight; ++i)
        {
            gridProperty.InsertArrayElementAtIndex(i);
            SerializedProperty serializedCell = gridProperty.GetArrayElementAtIndex(i);

            // find x and y grid positions of the cell with he formula : i = x * gridHeight + y
            int gridX = i / gridHeight;
            int gridY = i % gridHeight;


            serializedCell.FindPropertyRelative("x").intValue = grid[gridX, gridY].x;
            serializedCell.FindPropertyRelative("y").intValue = grid[gridX, gridY].y;
            
            serializedCell.FindPropertyRelative("hole").boolValue = grid[gridX, gridY].hole;
        
            serializedCell.FindPropertyRelative("presetContent").boolValue = grid[gridX, gridY].presetContent;
        
            serializedCell.FindPropertyRelative("content").enumValueIndex = (int)grid[gridX, gridY].content;
        }

        // apply the modified properties to the original SerializedObject
        serializedLevel.ApplyModifiedProperties();
    }


    struct EditableGridCell
    {
        public int x, y;

        public bool hole;

        public bool presetContent;
        public LevelEditorCellContent content;

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
