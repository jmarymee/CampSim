using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HandyHacks
{
    public class hh_BatchReplace : EditorWindow
    {
        static hh_BatchReplace window;


        public GameObject replaceWith;
        public Transform[] objectsToReplace;
        public bool matchRotation;
        public bool matchScale;

        private Vector2 scrollPos;
        
        //Create/ popup window
        [MenuItem("Tools/Handy Hacks/Batch Replace " + hh_Hotkeys.keyBinding_BatchReplace)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (hh_BatchReplace)EditorWindow.GetWindow(typeof(hh_BatchReplace), true, "Batch Replace");
            window.minSize = new Vector2(256, 128);
            window.maxSize = new Vector2(256, 2048);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Space(5);

            GUILayout.Label("Batch Replace a group of objects \nwith copies of a Scene Object or a Prefab.", EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Replace With: ");
            replaceWith = EditorGUILayout.ObjectField(replaceWith, typeof(GameObject), true) as GameObject;
            
            GUILayout.EndHorizontal();
            //------------------------------------------------------
            GUILayout.BeginHorizontal();

            GUILayout.Label("Match Rotation?");
            matchRotation = EditorGUILayout.Toggle(matchRotation);

            GUILayout.FlexibleSpace();

            GUILayout.Label("Match Scale?");
            matchScale = EditorGUILayout.Toggle(matchScale);

            GUILayout.EndHorizontal();
            //------------------------------------------------------
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Replace", GUILayout.Width(80)) && replaceWith != null)
            {
                if(objectsToReplace == null || objectsToReplace.Length == 0)
                {
                    return;
                }

                //After re-compile, window may not be recognized. (if it was open during the recompile.)
                if (window == null)
                    Init();

                Undo.RegisterCompleteObjectUndo(window, "Replace Objects");

                for (int i = 0; i < objectsToReplace.Length; i++)
                {
                    if (objectsToReplace[i] != null)
                    {
                        // Prevent this event from destroying the replaceWith object if it happens to be in the objectsToReplace list.
                        // if it is the same object, skip this iteration of the loop.
                        if(objectsToReplace[i] == replaceWith.transform)
                        {
                            continue;
                        }

                        GameObject replacement;

                        if (PrefabUtility.GetPrefabObject(replaceWith) != null)
                        {
                            replacement = PrefabUtility.InstantiatePrefab(replaceWith) as GameObject;
                            Undo.RegisterCreatedObjectUndo(replacement, "object create");
                        }
                        else
                        {
                            replacement = Instantiate(replaceWith) as GameObject;
                            Undo.RegisterCreatedObjectUndo(replacement, "object create");
                        }

                        if (replacement != null)
                        {
                            //Move to the selected object's spot in the hierarchy
                            replacement.transform.parent = objectsToReplace[i].parent;
                            replacement.transform.SetSiblingIndex(objectsToReplace[i].GetSiblingIndex());

                            replacement.transform.position = objectsToReplace[i].position;
                            if (matchRotation)
                            {
                                replacement.transform.rotation = objectsToReplace[i].rotation;
                            }
                            if (matchScale)
                            {
                                replacement.transform.localScale = objectsToReplace[i].localScale;
                            }

                            replacement.name = replaceWith.name;

                            Undo.DestroyObjectImmediate(objectsToReplace[i].gameObject);

                            objectsToReplace[i] = replacement.transform;
                        }
                    }
                }

                Undo.SetCurrentGroupName("Replace Object(s)");

                //Select all new objects.
                GameObject[] newSelection = new GameObject[objectsToReplace.Length];
                for(int i=0; i<newSelection.Length; i++)
                {
                    if (objectsToReplace[i] != null)
                    {
                        newSelection[i] = objectsToReplace[i].gameObject;
                    }
                }

                Selection.objects = newSelection;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //---------------------------------------------------------------------

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty objectsProperty = so.FindProperty("objectsToReplace");

            EditorGUILayout.PropertyField(objectsProperty, true);

            so.ApplyModifiedProperties();

            GUILayout.EndScrollView();
        }
    }
}
