using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HandyHacks
{
    public class hh_BatchRename : EditorWindow
    {
        public string newName;
        public GameObject[] objectsToRename;

        public bool addSuffix;
        public enum SuffixStyle { Underscore, Dash, Brackets, SquareBrackets}
        public SuffixStyle suffixStyle;

        private Vector2 scrollPos;

        //Create/ popup window
        [MenuItem("Tools/Handy Hacks/Batch Rename " + hh_Hotkeys.keyBinding_BatchRename)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            hh_BatchRename window = (hh_BatchRename)EditorWindow.GetWindow(typeof(hh_BatchRename), true, "Batch Rename");
            window.minSize = new Vector2(256, 256);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Space(5);

            GUILayout.Label("Batch Rename a group of objects.", EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(5);

            newName = EditorGUILayout.TextField("New Name:", newName);

            GUILayout.Space(5);

            addSuffix = EditorGUILayout.Toggle("Add Suffix?", addSuffix);

            if (addSuffix)
            {
                suffixStyle = (SuffixStyle)EditorGUILayout.EnumPopup("Suffix Style:", suffixStyle);
                GUILayout.Space(5);

                string preview = "";
                switch (suffixStyle)
                {
                    case SuffixStyle.Underscore:
                        preview = "Object_1\nObject_2\n...";
                        break;
                    case SuffixStyle.Dash:
                        preview = "Object - 1\nObject - 2\n...";
                        break;
                    case SuffixStyle.Brackets:
                        preview = "Object(1)\nObject(2)\n...";
                        break;
                    case SuffixStyle.SquareBrackets:
                        preview = "Object[1]\nObject[2]\n...";
                        break;
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Preview:\n\n" + preview, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }

            // Generate button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Rename", GUILayout.Width(80)))
            {
                if(objectsToRename == null || objectsToRename.Length == 0)
                {
                    return;
                }

                Undo.RecordObjects(objectsToRename, "Rename Objects");
                for (int i = 0; i < objectsToRename.Length; i++)
                {
                    if (objectsToRename[i] != null)
                    {
                        if (addSuffix)
                        {
                            switch (suffixStyle)
                            {
                                case SuffixStyle.Underscore:
                                    objectsToRename[i].name = newName + "_" + i;
                                    break;
                                case SuffixStyle.Dash:
                                    objectsToRename[i].name = newName + " - " + i;
                                    break;
                                case SuffixStyle.Brackets:
                                    objectsToRename[i].name = newName + "(" + i + ")";
                                    break;
                                case SuffixStyle.SquareBrackets:
                                    objectsToRename[i].name = newName + "[" + i + "]";
                                    break;
                            }
                        }
                        else
                        {
                            objectsToRename[i].name = newName;
                        }
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //---------------------------------------------------------------------
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty objectsProperty = so.FindProperty("objectsToRename");

            EditorGUILayout.PropertyField(objectsProperty, true);

            so.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }
    }
}
