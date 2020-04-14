using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HandyHacks
{
    [InitializeOnLoad]
    public class hh_Preferences
    {
        private static bool prefsLoaded = false;

        //Create Empty Parent
        public static bool createEmptyParent_advancedMode;
        //Quick Measures
        public static Color quickMeasures_colour;
        //Orientation Gizmo
        public static bool orientationGizmo_active;

        //Navigation Shortcuts
        public static KeyCode navKeys_front;
        public static KeyCode navKeys_side;
        public static KeyCode navKeys_top;
        public static KeyCode navKeys_rollLeft;
        public static KeyCode navKeys_rollRight;
        public static KeyCode navKeys_rollUp;
        public static KeyCode navKeys_rollDown;
        public static KeyCode navKeys_ortho;

        static hh_Hotkeys.Keys navKeys_internal_front;
        static hh_Hotkeys.Keys navKeys_internal_side;
        static hh_Hotkeys.Keys navKeys_internal_top;
        static hh_Hotkeys.Keys navKeys_internal_rollLeft;
        static hh_Hotkeys.Keys navKeys_internal_rollRight;
        static hh_Hotkeys.Keys navKeys_internal_rollUp;
        static hh_Hotkeys.Keys navKeys_internal_rollDown;
        static hh_Hotkeys.Keys navKeys_internal_ortho;

        static Vector2 scrollPos;

        static hh_Preferences()
        {
            if (!prefsLoaded)
            {
                LoadPreferences();
                LoadNavigationShortcuts();
                prefsLoaded = true;
            }
        }

        static bool LoadBool(string prefsKey, bool defaultValue)
        {
            if (!EditorPrefs.HasKey(prefsKey))
            {
                EditorPrefs.SetBool(prefsKey, defaultValue);
            }

            return EditorPrefs.GetBool(prefsKey);
        }

        static Color LoadColour(string prefsKey, Color defaultValue)
        {
            Color c = defaultValue;

            if (!EditorPrefs.HasKey(prefsKey + "_R"))
            {
                EditorPrefs.SetFloat(prefsKey + "_R", defaultValue.r);
            }
            else
            {
                c.r = EditorPrefs.GetFloat(prefsKey + "_R");
            }

            if (!EditorPrefs.HasKey(prefsKey + "_G"))
            {
                EditorPrefs.SetFloat(prefsKey + "_G", defaultValue.g);
            }
            else
            {
                c.g = EditorPrefs.GetFloat(prefsKey + "_G");
            }

            if (!EditorPrefs.HasKey(prefsKey + "_B"))
            {
                EditorPrefs.SetFloat(prefsKey + "_B", defaultValue.b);
            }
            else
            {
                c.b = EditorPrefs.GetFloat(prefsKey + "_B");
            }

            if (!EditorPrefs.HasKey(prefsKey + "_A"))
            {
                EditorPrefs.SetFloat(prefsKey + "_A", defaultValue.a);
            }
            else
            {
                c.a = EditorPrefs.GetFloat(prefsKey + "_A");
            }

            return c;
        }

        static void SaveColour(string prefsKey, Color colour)
        {
            EditorPrefs.SetFloat(prefsKey + "_R", colour.r);
            EditorPrefs.SetFloat(prefsKey + "_G", colour.g);
            EditorPrefs.SetFloat(prefsKey + "_B", colour.b);
            EditorPrefs.SetFloat(prefsKey + "_A", colour.a);
        }

        static void DeleteColour(string prefsKey)
        {
            EditorPrefs.DeleteKey(prefsKey + "_R");
            EditorPrefs.DeleteKey(prefsKey + "_G");
            EditorPrefs.DeleteKey(prefsKey + "_B");
            EditorPrefs.DeleteKey(prefsKey + "_A");
        }

        static KeyCode LoadKeyCode(string prefsKey, KeyCode defaultValue)
        {
            KeyCode value = defaultValue;

            if (!EditorPrefs.HasKey(prefsKey))
            {
                EditorPrefs.SetString(prefsKey, defaultValue.ToString());
            }
            else
            {
                value = (KeyCode)System.Enum.Parse(typeof(KeyCode), EditorPrefs.GetString(prefsKey));
            }

            return value;
        }

        static void LoadPreferences()
        {
            //Create Empty Parent
            createEmptyParent_advancedMode = LoadBool(hh_Constants.prefsPrefix + "createEmptyParent_advancedMode", hh_Constants.CreateEmptyParent_advancedMode);

            //Quick Measures
            quickMeasures_colour = LoadColour(hh_Constants.prefsPrefix + "quickMeasures_colour", hh_Constants.QuickMeasures_colour);

            //Orientation Gizmo
            orientationGizmo_active = LoadBool(hh_Constants.prefsPrefix + "orientationGizmo_active", hh_Constants.OrientationGizmo_active);
        }

        public static void SavePreferences()
        {
            //Create Empty Parent
            EditorPrefs.SetBool(hh_Constants.prefsPrefix + "createEmptyParent_advancedMode", createEmptyParent_advancedMode);

            //Quick Measures
            SaveColour(hh_Constants.prefsPrefix + "quickMeasures_colour", quickMeasures_colour);

            //Orientation Gizmo
            EditorPrefs.SetBool(hh_Constants.prefsPrefix + "orientationGizmo_active", orientationGizmo_active);
        }

        static void DeletePreferences()
        {
            //Create Empty Parent
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "createEmptyParent_advancedMode");
            //Quick Measures
            DeleteColour(hh_Constants.prefsPrefix + "quickMeasures_colour");
            //Orientation Gizmo
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "orientationGizmo_active");
        }

        static void LoadNavigationShortcuts()
        {
            //Navigation Shortcuts
            navKeys_front = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_front", hh_Constants.NavKeys_front);
            navKeys_side = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_side", hh_Constants.NavKeys_side);
            navKeys_top = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_top", hh_Constants.NavKeys_top);

            navKeys_rollLeft = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_rollLeft", hh_Constants.NavKeys_rollLeft);
            navKeys_rollRight = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_rollRight", hh_Constants.NavKeys_rollRight);
            navKeys_rollUp = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_rollUp", hh_Constants.NavKeys_rollUp);
            navKeys_rollDown = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_rollDown", hh_Constants.NavKeys_rollDown);

            navKeys_ortho = LoadKeyCode(hh_Constants.prefsPrefix + "navKeys_ortho", hh_Constants.NavKeys_ortho);

            //-------------------------------------------------------------------------------------------------------------------

            navKeys_internal_front = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_front);
            navKeys_internal_side = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_side);
            navKeys_internal_top = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_top);

            navKeys_internal_rollLeft = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_rollLeft);
            navKeys_internal_rollRight = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_rollRight);
            navKeys_internal_rollUp = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_rollUp);
            navKeys_internal_rollDown = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_rollDown);

            navKeys_internal_ortho = hh_Hotkeys.ConvertKeyCodeToKey(navKeys_ortho);
        }

        static void SaveNavigationShotrcuts()
        {
            //Navigation Shortcuts
            navKeys_front = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_front);
            navKeys_side = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_side);
            navKeys_top = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_top);

            navKeys_rollLeft = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_rollLeft);
            navKeys_rollRight = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_rollRight);
            navKeys_rollUp = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_rollUp);
            navKeys_rollDown = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_rollDown);

            navKeys_ortho = hh_Hotkeys.ConvertKeyToKeyCode(navKeys_internal_ortho);

            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_front", navKeys_front.ToString());
            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_side", navKeys_side.ToString());
            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_top", navKeys_top.ToString());

            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_rollLeft", navKeys_rollLeft.ToString());
            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_rollRight", navKeys_rollRight.ToString());
            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_rollUp", navKeys_rollUp.ToString());
            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_rollDown", navKeys_rollDown.ToString());

            EditorPrefs.SetString(hh_Constants.prefsPrefix + "navKeys_ortho", navKeys_ortho.ToString());
        }

        static void DeleteNavigationShortcuts()
        {
            //Navigation Shortcuts
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_front");
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_side");
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_top");

            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_rollLeft");
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_rollRight");
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_rollUp");
            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_rollDown");

            EditorPrefs.DeleteKey(hh_Constants.prefsPrefix + "navKeys_ortho");
        }

        [PreferenceItem("Handy Hacks")]
        static void PreferencesGUI()
        {
            if (!prefsLoaded)
            {
                LoadPreferences();
                LoadNavigationShortcuts();
                prefsLoaded = true;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(5) });

            EditorGUI.BeginChangeCheck();

            //Create Empty Parent
            GUILayout.Space(10);
            GUILayout.Label("Create Empty Parent", EditorStyles.largeLabel);
            GUILayout.Label("Mode:", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            createEmptyParent_advancedMode = !GUILayout.Toggle(!createEmptyParent_advancedMode, "Basic", EditorStyles.radioButton);
            createEmptyParent_advancedMode = GUILayout.Toggle(createEmptyParent_advancedMode, "Advanced", EditorStyles.radioButton);

            GUILayout.BeginVertical();
            if (createEmptyParent_advancedMode)
            {
                EditorGUILayout.HelpBox("Creates the New Parent at the position and rotation of the tool handles. See manual for details.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Always creates the New Parent at your selections mid-point, and with a rotation of 0, 0, 0 in world space. See manual for details.", MessageType.None);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });

            //Quick Measures
            GUILayout.Space(20);
            GUILayout.Label("Quick Measures", EditorStyles.largeLabel);
            quickMeasures_colour = EditorGUILayout.ColorField("Color", quickMeasures_colour);

            GUILayout.Space(20);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });

            //Restore Defaults
            GUILayout.Space(20);
            if (GUILayout.Button("Restore Default Settings"))
            {
                if (EditorUtility.DisplayDialog("Restore Defaults?", "Restore default settings for Handy Hacks? This action cannot be undone.", "OK", "Cancel"))
                {
                    DeletePreferences();
                    LoadPreferences();
                }
            }

            GUILayout.Space(20);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(5) });

            //Navigation Shortcuts
            GUILayout.Space(10);
            GUILayout.Label("Navigation Shortcuts", EditorStyles.largeLabel);
            navKeys_internal_front = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Front/ Back View:", navKeys_internal_front);
            navKeys_internal_side = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Left/ Right View:", navKeys_internal_side);
            navKeys_internal_top = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Top/ Bottom View:", navKeys_internal_top);
            GUILayout.Space(5);
            navKeys_internal_rollLeft = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Roll View Left:", navKeys_internal_rollLeft);
            navKeys_internal_rollRight = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Roll View Right:", navKeys_internal_rollRight);
            navKeys_internal_rollUp = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Roll View Up:", navKeys_internal_rollUp);
            navKeys_internal_rollDown = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Roll View Down:", navKeys_internal_rollDown);
            GUILayout.Space(5);
            navKeys_internal_ortho = (hh_Hotkeys.Keys)EditorGUILayout.EnumPopup("Orthographic/ Perspective:", navKeys_internal_ortho);

            //Restore Navigation Key Defaults
            GUILayout.Space(10);
            if (GUILayout.Button("Restore Default Navigation Shortcuts"))
            {
                if (EditorUtility.DisplayDialog("Restore Defaults?", "Restore default Navigation Keys? This action cannot be undone.", "OK", "Cancel"))
                {
                    DeleteNavigationShortcuts();
                    LoadNavigationShortcuts();
                }
            }

            GUILayout.Space(20);

            if (EditorGUI.EndChangeCheck())
            {
                SavePreferences();
                SaveNavigationShotrcuts();
            }

            EditorGUILayout.EndScrollView();

        }
    }
}
