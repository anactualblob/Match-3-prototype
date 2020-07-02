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

    string name;



    LevelScriptableObject editedLevel;

    // Serialized Object, used to serialize the scriptable object we're editing
    SerializedObject serializedLevel;



    VisualTreeAsset tree;
    StyleSheet stylesheet;


    private void OnEnable()
    {
        VisualElement root = rootVisualElement;

        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Editor Windows/Level Editor/LevelEditorWindow.uss");

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
        });
        
        // toggle autosave bool
        root.Q<Toggle>("autosave-toggle").RegisterValueChangedCallback(evt =>
        {
            root.Q<Button>(name = "save-button").visible = !evt.newValue;
            autosave = evt.newValue;
        });
        
        // save changes to serializedLevel when clicking the save button
        root.Q<Button>(name = "save-button").clickable.clicked += () => Save();
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



    public static void EditLevel(LevelScriptableObject levelToEdit)
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>();

        window.serializedLevel = new SerializedObject(levelToEdit);

        window.SetupEditor();

    }
}
