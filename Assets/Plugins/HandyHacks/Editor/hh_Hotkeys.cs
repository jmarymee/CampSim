using UnityEngine;

namespace HandyHacks
{
    public static class hh_Hotkeys
    {
        //------------------------------------------------------------------------
        //Key Bindings. Modify the values here to update hotkeys. 
        public const string keyBinding_QuickMeasures = "&M";
        public const string keyBinding_ScreenSpaceTools = "&X";
        public const string keyBinding_CreateEmptyParent = "&N";
        public const string keyBinding_DeselectAll = "%#A";
        public const string keyBinding_OrientationGizmo = "";
        public const string keyBinding_BatchReplace = "";
        public const string keyBinding_BatchRename = "";
        //------------------------------------------------------------------------


        public enum Keys
        {
            A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
            Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,
            Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,
            F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
        }

        public static KeyCode ConvertKeyToKeyCode(Keys key)
        {
            KeyCode value = new KeyCode();

            switch (key)
            {
                case Keys.A: value = KeyCode.A; break;
                case Keys.B: value = KeyCode.B; break;
                case Keys.C: value = KeyCode.C; break;
                case Keys.D: value = KeyCode.D; break;
                case Keys.E: value = KeyCode.E; break;
                case Keys.F: value = KeyCode.F; break;
                case Keys.G: value = KeyCode.G; break;
                case Keys.H: value = KeyCode.H; break;
                case Keys.I: value = KeyCode.I; break;
                case Keys.J: value = KeyCode.J; break;
                case Keys.K: value = KeyCode.K; break;
                case Keys.L: value = KeyCode.L; break;
                case Keys.M: value = KeyCode.M; break;
                case Keys.N: value = KeyCode.N; break;
                case Keys.O: value = KeyCode.O; break;
                case Keys.P: value = KeyCode.P; break;
                case Keys.Q: value = KeyCode.Q; break;
                case Keys.R: value = KeyCode.R; break;
                case Keys.S: value = KeyCode.S; break;
                case Keys.T: value = KeyCode.T; break;
                case Keys.U: value = KeyCode.U; break;
                case Keys.V: value = KeyCode.V; break;
                case Keys.W: value = KeyCode.W; break;
                case Keys.X: value = KeyCode.X; break;
                case Keys.Y: value = KeyCode.Y; break;
                case Keys.Z: value = KeyCode.Z; break;
                //------------------------------------------------
                case Keys.Alpha0: value = KeyCode.Alpha0; break;
                case Keys.Alpha1: value = KeyCode.Alpha1; break;
                case Keys.Alpha2: value = KeyCode.Alpha2; break;
                case Keys.Alpha3: value = KeyCode.Alpha3; break;
                case Keys.Alpha4: value = KeyCode.Alpha4; break;
                case Keys.Alpha5: value = KeyCode.Alpha5; break;
                case Keys.Alpha6: value = KeyCode.Alpha6; break;
                case Keys.Alpha7: value = KeyCode.Alpha7; break;
                case Keys.Alpha8: value = KeyCode.Alpha8; break;
                case Keys.Alpha9: value = KeyCode.Alpha9; break;
                //------------------------------------------------
                case Keys.Keypad0: value = KeyCode.Keypad0; break;
                case Keys.Keypad1: value = KeyCode.Keypad1; break;
                case Keys.Keypad2: value = KeyCode.Keypad2; break;
                case Keys.Keypad3: value = KeyCode.Keypad3; break;
                case Keys.Keypad4: value = KeyCode.Keypad4; break;
                case Keys.Keypad5: value = KeyCode.Keypad5; break;
                case Keys.Keypad6: value = KeyCode.Keypad6; break;
                case Keys.Keypad7: value = KeyCode.Keypad7; break;
                case Keys.Keypad8: value = KeyCode.Keypad8; break;
                case Keys.Keypad9: value = KeyCode.Keypad9; break;
                //------------------------------------------------
                case Keys.F1: value = KeyCode.F1; break;
                case Keys.F2: value = KeyCode.F2; break;
                case Keys.F3: value = KeyCode.F3; break;
                case Keys.F4: value = KeyCode.F4; break;
                case Keys.F5: value = KeyCode.F5; break;
                case Keys.F6: value = KeyCode.F6; break;
                case Keys.F7: value = KeyCode.F7; break;
                case Keys.F8: value = KeyCode.F8; break;
                case Keys.F9: value = KeyCode.F9; break;
                case Keys.F10: value = KeyCode.F10; break;
                case Keys.F11: value = KeyCode.F11; break;
                case Keys.F12: value = KeyCode.F12; break;
            }

            return value;
        }

        public static Keys ConvertKeyCodeToKey(KeyCode keyCode)
        {
            Keys value = new Keys();

            switch (keyCode)
            {
                case KeyCode.A: value = Keys.A; break;
                case KeyCode.B: value = Keys.B; break;
                case KeyCode.C: value = Keys.C; break;
                case KeyCode.D: value = Keys.D; break;
                case KeyCode.E: value = Keys.E; break;
                case KeyCode.F: value = Keys.F; break;
                case KeyCode.G: value = Keys.G; break;
                case KeyCode.H: value = Keys.H; break;
                case KeyCode.I: value = Keys.I; break;
                case KeyCode.J: value = Keys.J; break;
                case KeyCode.K: value = Keys.K; break;
                case KeyCode.L: value = Keys.L; break;
                case KeyCode.M: value = Keys.M; break;
                case KeyCode.N: value = Keys.N; break;
                case KeyCode.O: value = Keys.O; break;
                case KeyCode.P: value = Keys.P; break;
                case KeyCode.Q: value = Keys.Q; break;
                case KeyCode.R: value = Keys.R; break;
                case KeyCode.S: value = Keys.S; break;
                case KeyCode.T: value = Keys.T; break;
                case KeyCode.U: value = Keys.U; break;
                case KeyCode.V: value = Keys.V; break;
                case KeyCode.W: value = Keys.W; break;
                case KeyCode.X: value = Keys.X; break;
                case KeyCode.Y: value = Keys.Y; break;
                case KeyCode.Z: value = Keys.Z; break;
                //------------------------------------------------
                case KeyCode.Alpha0: value = Keys.Alpha0; break;
                case KeyCode.Alpha1: value = Keys.Alpha1; break;
                case KeyCode.Alpha2: value = Keys.Alpha2; break;
                case KeyCode.Alpha3: value = Keys.Alpha3; break;
                case KeyCode.Alpha4: value = Keys.Alpha4; break;
                case KeyCode.Alpha5: value = Keys.Alpha5; break;
                case KeyCode.Alpha6: value = Keys.Alpha6; break;
                case KeyCode.Alpha7: value = Keys.Alpha7; break;
                case KeyCode.Alpha8: value = Keys.Alpha8; break;
                case KeyCode.Alpha9: value = Keys.Alpha9; break;
                //------------------------------------------------
                case KeyCode.Keypad0: value = Keys.Keypad0; break;
                case KeyCode.Keypad1: value = Keys.Keypad1; break;
                case KeyCode.Keypad2: value = Keys.Keypad2; break;
                case KeyCode.Keypad3: value = Keys.Keypad3; break;
                case KeyCode.Keypad4: value = Keys.Keypad4; break;
                case KeyCode.Keypad5: value = Keys.Keypad5; break;
                case KeyCode.Keypad6: value = Keys.Keypad6; break;
                case KeyCode.Keypad7: value = Keys.Keypad7; break;
                case KeyCode.Keypad8: value = Keys.Keypad8; break;
                case KeyCode.Keypad9: value = Keys.Keypad9; break;
                //------------------------------------------------
                case KeyCode.F1: value = Keys.F1; break;
                case KeyCode.F2: value = Keys.F2; break;
                case KeyCode.F3: value = Keys.F3; break;
                case KeyCode.F4: value = Keys.F4; break;
                case KeyCode.F5: value = Keys.F5; break;
                case KeyCode.F6: value = Keys.F6; break;
                case KeyCode.F7: value = Keys.F7; break;
                case KeyCode.F8: value = Keys.F8; break;
                case KeyCode.F9: value = Keys.F9; break;
                case KeyCode.F10: value = Keys.F10; break;
                case KeyCode.F11: value = Keys.F11; break;
                case KeyCode.F12: value = Keys.F12; break;
            }

            return value;
        }

    }
}
