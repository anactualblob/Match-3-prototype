using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

[CustomEditor(typeof(LevelScriptableObject))]
public class LevelScriptableObjectCustomInspector : Editor
{
    // this custom inspector should only display the grid and provide a button 
    //    to open the Level Editor window and pass it the inspected Level instance


    int width, height;

    string levelName;


    VisualTreeAsset tree;
    StyleSheet stylesheet;
    VisualElement root;

    LevelScriptableObject inspectedLevel;

    private void OnEnable()
    {
        inspectedLevel = target as LevelScriptableObject;


        root = new VisualElement();

        tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uxml");
        stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Custom Inspectors/LevelScriptableObjectCustomInspector.uss");
        
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


        return root;
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
