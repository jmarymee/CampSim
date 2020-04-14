using UnityEngine;
using System.Collections;

namespace HandyHacks
{
    public class hh_Constants
    {
        public static string prefsPrefix { get { return "Rippleware.HandyHacks."; } }

        //Create Empty Parent
        public  static bool CreateEmptyParent_advancedMode { get { return false; } }
        //Quick Measures
        public static Color QuickMeasures_colour { get { return Color.cyan; } }
        //Orientation Gizmo
        public static bool OrientationGizmo_active { get { return false; } }

        //Navigation Hotkeys
        public static KeyCode NavKeys_front { get { return KeyCode.Keypad1; } }
        public static KeyCode NavKeys_side { get { return KeyCode.Keypad3; } }
        public static KeyCode NavKeys_top { get { return KeyCode.Keypad7; } }

        public static KeyCode NavKeys_rollLeft { get { return KeyCode.Keypad4; } }
        public static KeyCode NavKeys_rollRight { get { return KeyCode.Keypad6; } }
        public static KeyCode NavKeys_rollUp { get { return KeyCode.Keypad8; } }
        public static KeyCode NavKeys_rollDown { get { return KeyCode.Keypad2; } }

        public static KeyCode NavKeys_ortho { get { return KeyCode.Keypad5; } }
    }
}
