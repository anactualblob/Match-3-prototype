using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
#pragma warning disable 0649

    // core level variables
    int gridHeight;
    int gridWidth;

    bool autosave;
    new string name;

    EditableGridCell[,] grid;

    List<LevelEditorCellContent> presetColorsList = new List<LevelEditorCellContent>();


    // tools
    enum Tools
    {
        none,
        hole,
        reset,
        candy_blue,
        candy_red,
        candy_green,
        candy_yellow,
        candy_orange
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

    #region Editor Setup
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


        // Setup the hole toolbar button
        ToolbarButton holeToolButton = root.Q<ToolbarButton>("tool-hole");
        SetupToolButton(holeToolButton, Tools.hole);

        // Setup the blue candy toolbar button
        ToolbarButton blueCandyToolbutton = root.Q<ToolbarButton>("tool-candy-blue");
        SetupToolButton(blueCandyToolbutton, Tools.candy_blue);

        // Setup the red candy toolbar button
        ToolbarButton redCandyToolbutton = root.Q<ToolbarButton>("tool-candy-red");
        SetupToolButton(redCandyToolbutton, Tools.candy_red);

        // Setup the green candy toolbar button
        ToolbarButton greenCandyToolbutton = root.Q<ToolbarButton>("tool-candy-green");
        SetupToolButton(greenCandyToolbutton, Tools.candy_green);

        // Setup the yellow candy toolbar button
        ToolbarButton yellowCandyToolbutton = root.Q<ToolbarButton>("tool-candy-yellow");
        SetupToolButton(yellowCandyToolbutton, Tools.candy_yellow);

        // Setup the orange candy toolbar button
        ToolbarButton orangeCandyToolbutton = root.Q<ToolbarButton>("tool-candy-orange");
        SetupToolButton(orangeCandyToolbutton, Tools.candy_orange);


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
    /// Sets up a "homemade" toggle button in the toolbar. 
    /// <para>
    /// sets the "inactive" class on all the toolbar's buttons, and sets the given tool as the active one if it isn't active, otherwise sets the active tool to none
    /// </para>
    /// </summary> 
    void SetupToolButton(ToolbarButton button, Tools tool)
    {
        button.clickable.clicked += () =>
        {
            Toolbar toolbar = rootVisualElement.Q<Toolbar>("toolbar");
            foreach (VisualElement e in toolbar.Children())
            {
                e.RemoveFromClassList("tool-active");
                e.AddToClassList("tool-inactive");
            }

            if (activeTool != tool)
            {
                button.AddToClassList("tool-active");
                button.RemoveFromClassList("tool-inactive");
                activeTool = tool;
            }
            else
            {
                activeTool = Tools.none;
            }
        };
    }
    #endregion



    #region Grid Setup, loading and saving
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
                grid[i, j].content = (LevelEditorCellContent)serializedCell.FindPropertyRelative("content").intValue;
                
                // if the cell we loaded is a hole, update its display
                if (grid[i, j].hole)
                {
                    VisualElement bg = cell.Q<VisualElement>("Cell");
                    bg.style.visibility = Visibility.Hidden;
                }

                VisualElement cellBackground = cell.Q<VisualElement>("Cell");

                RemoveCandyClassesFromCell(cellBackground);

                switch (grid[i, j].content)
                {
                    case LevelEditorCellContent.random:
                        cellBackground.AddToClassList("empty");
                        break;

                    case LevelEditorCellContent.candy_blue:
                        cellBackground.AddToClassList("candy-blue");
                        break;

                    case LevelEditorCellContent.candy_red:
                        cellBackground.AddToClassList("candy-red");
                        break;

                    case LevelEditorCellContent.candy_green:
                        cellBackground.AddToClassList("candy-green");
                        break;

                    case LevelEditorCellContent.candy_orange:
                        cellBackground.AddToClassList("candy-orange");
                        break;

                    case LevelEditorCellContent.candy_yellow:
                        cellBackground.AddToClassList("candy-yellow");
                        break;
                }
            }
            cellContainer.Add(row);
        }
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

        // reset the presetColor_ bools
        serializedLevel.FindProperty("presetColor_blue").boolValue = false;


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

            serializedCell.FindPropertyRelative("content").intValue = (int)grid[gridX, gridY].content;


            // if we find a cell that has a preset candy color, we set the corresponding bool on the seriliazedlevel
            if (grid[gridX, gridY].content != LevelEditorCellContent.random)
            {
                switch (grid[gridX, gridY].content)
                {
                    case LevelEditorCellContent.candy_blue:
                        serializedLevel.FindProperty("presetColor_blue").boolValue = true;
                        break;
                    case LevelEditorCellContent.candy_red:
                        break;
                    case LevelEditorCellContent.candy_green:
                        break;
                    case LevelEditorCellContent.candy_orange:
                        break;
                    case LevelEditorCellContent.candy_yellow:
                        break;
                }

                // add the GridManager.CellContent to the array
                if (!presetColorsList.Contains(grid[gridX, gridY].content)) 
                    presetColorsList.Add(grid[gridX, gridY].content);
            }

        }

        SerializedProperty presetColorsArray = serializedLevel.FindProperty("presetColors");
        presetColorsArray.ClearArray();

        for (int i = 0; i < presetColorsList.Count; ++i)
        {
            presetColorsArray.InsertArrayElementAtIndex(i);
            presetColorsArray.GetArrayElementAtIndex(i).intValue = (int)presetColorsList[i];
        }

        // apply the modified properties to the original SerializedObject
        serializedLevel.ApplyModifiedProperties();
    }
    #endregion


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
            // if the clicked cell is a hole, we can only interact with it if the active tool is the hole tool
            if (grid[x, y].hole)
            {
                if (activeTool == Tools.hole)
                {
                    VisualElement bg = cell.Q<VisualElement>("Cell");
                    bg.style.visibility = bg.style.visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

                    grid[x, y].hole = (bg.style.visibility == Visibility.Hidden);
                }
            }
            else
            {

                VisualElement cellBackground = cell.Q<VisualElement>("Cell");

                switch (activeTool)
                {
                    case Tools.none:
                        break;

                    case Tools.hole: // toggle cell visibility
                        cellBackground.style.visibility = cellBackground.style.visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

                        grid[x, y].hole = (cellBackground.style.visibility == Visibility.Hidden);
                        break;

                    case Tools.reset:
                        break;

                    // toggle between "empty" and "candy-blue" classes on cell
                    case Tools.candy_blue:
                        if (!cellBackground.ClassListContains("candy-blue"))
                        {
                            RemoveCandyClassesFromCell(cellBackground);

                            cellBackground.AddToClassList("candy-blue");
                            grid[x, y].content = LevelEditorCellContent.candy_blue;
                        }
                        else
                        {
                            cellBackground.RemoveFromClassList("candy-blue");

                            cellBackground.AddToClassList("empty");
                            grid[x, y].content = LevelEditorCellContent.random;
                        }
                        break;

                    // toggle between "empty" and "candy-red" classes on cell
                    case Tools.candy_red:
                        if (!cellBackground.ClassListContains("candy-red"))
                        {
                            RemoveCandyClassesFromCell(cellBackground);

                            cellBackground.AddToClassList("candy-red");
                            grid[x, y].content = LevelEditorCellContent.candy_red;
                        }
                        else
                        {
                            cellBackground.RemoveFromClassList("candy-red");

                            cellBackground.AddToClassList("empty");
                            grid[x, y].content = LevelEditorCellContent.random;
                        }
                        break;

                    // toggle between "empty" and "candy-red" classes on cell
                    case Tools.candy_green:
                        if (!cellBackground.ClassListContains("candy-green"))
                        {
                            RemoveCandyClassesFromCell(cellBackground);

                            cellBackground.AddToClassList("candy-green");
                            grid[x, y].content = LevelEditorCellContent.candy_green;
                        }
                        else
                        {
                            cellBackground.RemoveFromClassList("candy-green");

                            cellBackground.AddToClassList("empty");
                            grid[x, y].content = LevelEditorCellContent.random;
                        }
                        break;

                    // toggle between "empty" and "candy-red" classes on cell
                    case Tools.candy_yellow:
                        if (!cellBackground.ClassListContains("candy-yellow"))
                        {
                            RemoveCandyClassesFromCell(cellBackground);

                            cellBackground.AddToClassList("candy-yellow");
                            grid[x, y].content = LevelEditorCellContent.candy_yellow;
                        }
                        else
                        {
                            cellBackground.RemoveFromClassList("candy-yellow");

                            cellBackground.AddToClassList("empty");
                            grid[x, y].content = LevelEditorCellContent.random;
                        }
                        break;

                    // toggle between "empty" and "candy-red" classes on cell
                    case Tools.candy_orange:
                        if (!cellBackground.ClassListContains("candy-orange"))
                        {
                            RemoveCandyClassesFromCell(cellBackground);

                            cellBackground.AddToClassList("candy-orange");
                            grid[x, y].content = LevelEditorCellContent.candy_orange;
                        }
                        else
                        {
                            cellBackground.RemoveFromClassList("candy-orange");

                            cellBackground.AddToClassList("empty");
                            grid[x, y].content = LevelEditorCellContent.random;
                        }
                        break;
                }
            }

        }

        if (autosave) Save();
    }



    /// <summary>
    /// Removes all candy-related uss classes from the given cell Visual Element
    /// </summary>
    /// <param name="cell"></param>
    void RemoveCandyClassesFromCell(VisualElement cell)
    {
        cell.RemoveFromClassList("candy-blue");
        cell.RemoveFromClassList("candy-red");
        cell.RemoveFromClassList("candy-green");
        cell.RemoveFromClassList("candy-yellow");
        cell.RemoveFromClassList("candy-orange");
        cell.RemoveFromClassList("empty");
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
