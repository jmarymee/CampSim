using UnityEngine;
using UnityEditor;

namespace HandyHacks
{
    /// <summary>
    /// Creates a new object and parents all selected gameobjects to it.
    /// </summary>
    static class hh_CreateEmptyParent
    {
        static Vector3 SelectionCenter()
        {
            Vector3 value = new Vector3();

            for(int i=0; i<Selection.transforms.Length; i++)
            {
                value += Selection.transforms[i].position;
            }

            int total = Selection.transforms.Length;
            value = new Vector3(value.x / total, value.y / total, value.z/ total);

            return value;
        }

        /// <summary>
        /// Expands the new parent object in the hierarchy to display its children.
        /// </summary>
        static void Expand()
        {
            Event expand = new Event() { keyCode = KeyCode.RightArrow, type = EventType.KeyDown };

            //Focus on hierarchy window.
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");

            EditorWindow.focusedWindow.SendEvent(expand);

#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= Expand;
#else
            EditorApplication.hierarchyWindowChanged -= Expand;
#endif
        }

        [MenuItem("GameObject/Create Empty Parent " + hh_Hotkeys.keyBinding_CreateEmptyParent, false, 0)]
        static void CreateParent()
        {
            string undoMessage = "Create Empty Parent";
            if (Selection.transforms.Length != 0)
            {
#if UNITY_2018_1_OR_NEWER
                EditorApplication.hierarchyChanged -= Expand;
                EditorApplication.hierarchyChanged += Expand;
#else
                EditorApplication.hierarchyWindowChanged -= Expand;
                EditorApplication.hierarchyWindowChanged += Expand;
#endif

                GameObject go = new GameObject();
                go.name = "New Parent";

                //Move to the selected object's spot in the hierarchy
                go.transform.parent = Selection.activeTransform.parent;
                go.transform.SetSiblingIndex(Selection.activeTransform.GetSiblingIndex());

                Undo.RegisterCreatedObjectUndo(go, undoMessage);

                if (Selection.transforms.Length > 1)
                {
                    if (hh_Preferences.createEmptyParent_advancedMode)
                    {
                        go.transform.position = Tools.handlePosition;
                        if (Tools.pivotRotation == PivotRotation.Local)
                        {
                            go.transform.rotation = Selection.activeTransform.rotation;
                        }
                    }
                    else
                    {
                        go.transform.position = SelectionCenter();
                    }

                    //Parent the selected objects to the new gameobject. Gotta do it backwords to maintain hierarchy order of children
                    for (int i = Selection.transforms.Length - 1; i >= 0; i--)
                    {
                        Undo.SetTransformParent(Selection.transforms[i], go.transform, undoMessage);
                    }
                }
                else
                {
                    go.transform.position = Selection.activeTransform.position;

                    if (hh_Preferences.createEmptyParent_advancedMode && Tools.pivotRotation == PivotRotation.Local)
                    {
                        go.transform.rotation = Selection.activeTransform.rotation;
                    }

                    //Parent the selected object to the new gameobject.
                    Undo.SetTransformParent(Selection.activeTransform, go.transform, undoMessage);          
                }

                //Select the new parent game object.
                Selection.activeTransform = go.transform;

                //Changing the selection triggers the hierarchyWindowChanged callback. Expand() event is called.  

            }
            //If nothing is selected, just make an object at root.
            else
            {
                GameObject go = new GameObject();
                go.name = "GameObject";
                Undo.RegisterCreatedObjectUndo(go, undoMessage);
                Selection.activeTransform = go.transform;
            }
        }
    }
}
