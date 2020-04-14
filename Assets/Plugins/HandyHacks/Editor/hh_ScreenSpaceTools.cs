using UnityEngine;
using UnityEditor;

namespace HandyHacks
{
    /// <summary>
    /// Aligns the transform gizmos to the orientation of the scene view camera.
    /// </summary>
    static class hh_ScreenSpaceTools
    {
        static hh_ScreenSpaceTools() { }
        static bool enabled = false;
        static bool resetRotation = false;
        static GUIStyle guiStyle = new GUIStyle();

        [MenuItem("Tools/Handy Hacks/Screen Space Tools " + hh_Hotkeys.keyBinding_ScreenSpaceTools)]
        static void Toggle()
        {
            enabled = !enabled;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            if (enabled)
            {
                SceneView.onSceneGUIDelegate += OnSceneGUI;

                guiStyle.normal.textColor = Handles.centerColor;
                //Tool rotation gets locked when in local pivot rotation mode.
                Tools.pivotRotation = PivotRotation.Global;
                if (Tools.current != Tool.Scale && Tools.current != Tool.Rect)
                {
                    Tools.handleRotation = Quaternion.LookRotation(SceneView.lastActiveSceneView.camera.transform.forward, SceneView.lastActiveSceneView.camera.transform.up);
                    SceneView.RepaintAll();
                }
            }
            else
            {
                Tools.handleRotation = Quaternion.identity;
            }
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            //If the pivot rotation mode changes, disable screen space tools.
            if (Tools.pivotRotation != PivotRotation.Global)
            {
                Toggle();
                return;
            }

            //Draw label showing that screen space tools are active
            Rect area = new Rect(sceneView.position.width - 140f, sceneView.position.height - 40, 120f, 16f);
            Handles.BeginGUI();
            GUI.Label(area, "Screen Space Handles", guiStyle);
            Handles.EndGUI();

            if (Event.current.isMouse)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    resetRotation = false;
                }
                else
                {
                    resetRotation = true;
                }
            }

            if (resetRotation && (Tools.current == Tool.Move || Tools.current == Tool.Rotate))
            {
                Tools.handleRotation = Quaternion.LookRotation(sceneView.camera.transform.forward, sceneView.camera.transform.up);
            }
        }
    }
}
