using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HandyHacks
{
    [InitializeOnLoad]
    public class hh_SceneNavigationShortcuts
    {
        static hh_SceneNavigationShortcuts()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView view)
        {
            if(view != SceneView.lastActiveSceneView || view.in2DMode)
            {
                return;
            }

            Event current = Event.current;

            if (current.type == EventType.KeyDown)
            {
                //Snap View ------------------------------------------------------------------------------------------
                if (current.keyCode == hh_Preferences.navKeys_front)
                {
                    if (Quaternion.LookRotation(view.camera.transform.forward, view.camera.transform.up) != Quaternion.LookRotation(-Vector3.forward, Vector3.up))
                    {
                        //Front View
                        view.LookAt(view.pivot, Quaternion.LookRotation(-Vector3.forward, Vector3.up), view.size, view.orthographic);
                    }
                    else
                    {
                        //Back View
                        view.LookAt(view.pivot, Quaternion.LookRotation(Vector3.forward, Vector3.up), view.size, view.orthographic);
                    }

                    Event.current.Use();
                }

                if (current.keyCode == hh_Preferences.navKeys_side)
                {
                    if (Quaternion.LookRotation(view.camera.transform.forward, view.camera.transform.up) != Quaternion.LookRotation(Vector3.right, Vector3.up))
                    {
                        //Left View
                        view.LookAt(view.pivot, Quaternion.LookRotation(Vector3.right, Vector3.up), view.size, view.orthographic);
                    }
                    else
                    {
                        //Right View
                        view.LookAt(view.pivot, Quaternion.LookRotation(-Vector3.right, Vector3.up), view.size, view.orthographic);
                    }

                    Event.current.Use();
                }

                if (current.keyCode == hh_Preferences.navKeys_top)
                {
                    if (Quaternion.LookRotation(view.camera.transform.forward, view.camera.transform.up) != Quaternion.LookRotation(-Vector3.up, Vector3.forward))
                    {
                        //Top View
                        view.LookAt(view.pivot, Quaternion.LookRotation(-Vector3.up, Vector3.forward), view.size, view.orthographic);
                    }
                    else
                    {
                        //Bottom View
                        view.LookAt(view.pivot, Quaternion.LookRotation(Vector3.up, -Vector3.forward), view.size, view.orthographic);
                    }

                    Event.current.Use();
                }

                //Roll View -------------------------------------------------------------------------------------------------------
                float rollAmount = 11.25f;

                if (current.keyCode == hh_Preferences.navKeys_rollLeft)
                {
                    view.rotation = Quaternion.AngleAxis(rollAmount, Vector3.up) * view.rotation;
                }
                if (current.keyCode == hh_Preferences.navKeys_rollRight)
                {
                    view.rotation = Quaternion.AngleAxis(-rollAmount, Vector3.up) * view.rotation;
                }
                if (current.keyCode == hh_Preferences.navKeys_rollUp)
                {
                    view.rotation *= Quaternion.AngleAxis(rollAmount, Vector3.right);
                }
                if (current.keyCode == hh_Preferences.navKeys_rollDown)
                {
                    view.rotation *= Quaternion.AngleAxis(-rollAmount, Vector3.right);
                }

                //Perspective/ Orthographic ---------------------------------------------------------------------------------------
                if (current.keyCode == hh_Preferences.navKeys_ortho)
                {
                    view.orthographic = !view.orthographic;
                }
            }
        }
    }
}