using UnityEditor;
using UnityEngine;

public class RealtimeStuff : EditorWindow
{
    [MenuItem("Tools/RealtimeStuff")]
    static void OpenWindow()
    {
        GetWindow<RealtimeStuff>();
    }
    GameObject oldact;
    void OnSelectionChange()
    {
        var wasname = "null";
        if (oldact)
        {
            wasname = oldact.name;
        }
        var newname = "null";
        if (Selection.activeGameObject)
        {
            newname = Selection.activeGameObject.name;
        }
        Debug.Log("RTS.OnSelectionChange - was:" + wasname + "  now:" + newname);
        oldact = Selection.activeGameObject;
    }
    void OnGUI()
    {
        GUILayout.Label("This is some window content");
    }
}