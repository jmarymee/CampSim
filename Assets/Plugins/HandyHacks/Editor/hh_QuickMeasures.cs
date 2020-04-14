using UnityEngine;
using UnityEditor;

namespace HandyHacks
{
    static class hh_QuickMeasures
    {
        private enum State { Disabled, SetFirstPoint, SetLastPoint, SetAnglePoint, Finished };
        static State state = State.Disabled;

        static int workViewID = -1;

        static Vector3 startPoint;
        static Vector3 endPoint;
        static Vector3 anglePoint;

        static float distance;

        static float angle;
        static float angleSign;
        static float snapAngleSign = 0.0f;
        static float snapAngle = 0.0f;

        [MenuItem("Tools/Handy Hacks/Quick Measure " + hh_Hotkeys.keyBinding_QuickMeasures)]
        static void Activate()
        {
            Init();

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        static void Init()
        {
            state = State.SetFirstPoint;
            workViewID = -1;
            startPoint = new Vector3();
            endPoint = new Vector3();
            anglePoint = new Vector3();

            distance = 0;
            angle = 0;
            angleSign = 0;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (state != State.Finished && state != State.Disabled)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                GUIUtility.hotControl = controlID;
            }

            switch (state)
            {
                case State.SetFirstPoint:

                    if (Event.current.type == EventType.MouseDown)
                    {
                        switch (Event.current.button)
                        {
                            //Set first point
                            case 0:

                                Event.current.Use();

                                if (workViewID == -1 && sceneView.orthographic)
                                {
                                    workViewID = sceneView.GetInstanceID();

                                    startPoint = sceneView.camera.ScreenToWorldPoint(
                                        new Vector3(
                                        Event.current.mousePosition.x,
                                        sceneView.position.height - Event.current.mousePosition.y - 18,
                                        (sceneView.camera.nearClipPlane + sceneView.camera.farClipPlane) / 2
                                        )
                                    );

                                    endPoint = startPoint;

                                    state = State.SetLastPoint;
                                }
                                else
                                {
                                    sceneView.ShowNotification(new GUIContent("Quick Measures only works in Orthographic Views"));
                                    state = State.Disabled;
                                    return;
                                }

                                break;
                            case 1:

                                Event.current.Use();
                                state = State.Disabled;

                                sceneView.Repaint();

                                break;
                            case 2:

                                break;
                        }
                    }

                    DrawCursorIcon(sceneView);

                    break;
                case State.SetLastPoint:

                    //Update the end point
                    if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                    {
                        if (workViewID != sceneView.GetInstanceID())
                        {
                            return;
                        }

                        endPoint = sceneView.camera.ScreenToWorldPoint(
                                new Vector3(
                                Event.current.mousePosition.x,
                                sceneView.position.height - Event.current.mousePosition.y - 18,
                                (sceneView.camera.nearClipPlane + sceneView.camera.farClipPlane) / 2
                                ));

                        if (Event.current.control)
                        {
                            AngleSnap(sceneView);
                        }
                    }

                    if (Event.current.type == EventType.MouseDown)
                    {
                        switch (Event.current.button)
                        {
                            case 0: //Set end point

                                Event.current.Use();

                                if (workViewID != sceneView.GetInstanceID())
                                {
                                    state = State.Disabled;
                                    return;
                                }
                                else
                                {
                                    state = State.Finished;
                                }

                                break;
                            case 1: //Switch to angle finder

                                Event.current.Use();

                                anglePoint = endPoint;
                                state = State.SetAnglePoint;

                                break;
                            case 2:

                                break;
                        }
                    }

                    DrawCursorIcon(sceneView);
                    DrawRuler();

                    break;
                case State.SetAnglePoint:

                    //Update the angle point
                    if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                    {
                        if (workViewID != sceneView.GetInstanceID())
                        {
                            return;
                        }

                        anglePoint = sceneView.camera.ScreenToWorldPoint(
                                new Vector3(
                                Event.current.mousePosition.x,
                                sceneView.position.height - Event.current.mousePosition.y - 18,
                                (sceneView.camera.nearClipPlane + sceneView.camera.farClipPlane) / 2
                                ));

                        if (Event.current.control)
                        {
                            AngleSnap(sceneView);
                        }
                    }

                    if (Event.current.type == EventType.MouseDown)
                    {
                        switch (Event.current.button)
                        {
                            case 0: //Set angle point

                                Event.current.Use();

                                if (workViewID != sceneView.GetInstanceID())
                                {
                                    state = State.Disabled;
                                    return;
                                }
                                else
                                {
                                    state = State.Finished;
                                }

                                break;
                            case 1:

                                Event.current.Use();

                                if (workViewID != sceneView.GetInstanceID())
                                {
                                    state = State.Disabled;
                                    return;
                                }
                                else
                                {
                                    state = State.Finished;
                                }

                                break;
                            case 2:

                                break;
                        }
                    }

                    DrawCursorIcon(sceneView);
                    DrawProtractor(sceneView);

                    break;
                case State.Finished:
                    //If the protractor was used, angleSign will = 1 or -1.
                    if (angleSign != 0)
                    {
                        DrawProtractor(sceneView);
                    }
                    else
                    {
                        DrawRuler();
                    }

                    if (workViewID == sceneView.GetInstanceID())
                    {
                        DisplayMeasurement();
                        sceneView.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown)
                    {
                        switch (Event.current.button)
                        {
                            case 0:

                                state = State.Disabled;

                                break;
                            case 1:

                                state = State.Disabled;

                                break;
                            case 2:
                                break;
                        }
                    }

                    break;
                case State.Disabled:

                    OnDisable();

                    break;
                default:

                    break;
            }
        }

        static void DrawCursorIcon(SceneView sceneView)
        {
            Handles.color = hh_Preferences.quickMeasures_colour;

            Rect area = new Rect(Event.current.mousePosition, new Vector2(45, 45));

            Camera camera = sceneView.camera;
            HandleUtility.PushCamera(camera);

            camera.orthographic = true;
            camera.orthographicSize = 100f;
            camera.cullingMask = 0;
            camera.transform.position = new Vector3(0f, 0f, -5f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;

            Handles.SetCamera(area, camera);

            Vector3 zero = new Vector3(2, 50f, 0);
            switch (state)
            {
                case State.SetFirstPoint:
                    //Edge
                    Handles.DrawLine(zero, new Vector3(98f, 50f, 0));
                    //Long tick
                    Handles.DrawLine(zero, new Vector3(2, 80f, 0));
                    //Short ticks
                    Handles.DrawLine(new Vector3(25f, 50f, 0), new Vector3(25f, 70f, 0));
                    Handles.DrawLine(new Vector3(50f, 50f, 0), new Vector3(50f, 70f, 0));
                    Handles.DrawLine(new Vector3(75f, 50f, 0), new Vector3(75f, 70f, 0));
                    break;
                case State.SetLastPoint:
                    //Edge
                    Handles.DrawLine(zero, new Vector3(98f, 50f, 0));
                    //Long tick
                    Handles.DrawLine(new Vector3(98f, 50f, 0), new Vector3(98f, 80f, 0));
                    //Short ticks
                    Handles.DrawLine(new Vector3(25f, 50f, 0), new Vector3(25f, 70f, 0));
                    Handles.DrawLine(new Vector3(50f, 50f, 0), new Vector3(50f, 70f, 0));
                    Handles.DrawLine(new Vector3(75f, 50f, 0), new Vector3(75f, 70f, 0));

                    //Measurement
                    DisplayMeasurement();
                    break;
                case State.SetAnglePoint:

                    Handles.DrawLine(new Vector3(2, 80, 0), new Vector3(98, 80, 0));
                    Handles.DrawLine(new Vector3(2, 80, 0), new Vector3(64, 24, 0));

                    Handles.DrawWireArc(new Vector3(2, 80, 0), Vector3.forward, Vector3.right, -45f, 40f);

                    //Measurement
                    DisplayMeasurement();

                    break;
                default:
                    break;
            }

            if (!sceneView.orthographic || (workViewID != -1 && workViewID != sceneView.GetInstanceID()))
            {
                Handles.color = Color.red;

                Handles.DrawLine(new Vector3(98, 85, 0), new Vector3(2, 45, 0));
            }

            HandleUtility.PopCamera(camera);
            Handles.SetCamera(camera);

            sceneView.Repaint();
        }

        static void DrawRuler()
        {

            Handles.color = hh_Preferences.quickMeasures_colour;

            Handles.DrawLine(startPoint, endPoint);

            distance = Vector3.Distance(startPoint, endPoint);
        }

        static void DrawProtractor(SceneView sceneView)
        {
            DrawRuler();

            Vector3 camForward = sceneView.camera.transform.forward;
            Vector3 heading = (endPoint - startPoint).normalized;

            if (Event.current.isMouse && !Event.current.control)
            {
                Vector3 heading2 = (anglePoint - startPoint).normalized;
                Vector3 perpHeading = Quaternion.AngleAxis(90, camForward) * heading;

                float dot = Vector3.Dot(heading, heading2);
                float dot2 = Vector3.Dot(perpHeading, heading2);

                float tempAngle = Vector3.Angle(startPoint - endPoint, startPoint - anglePoint);

                //If we're on the origin half of the circle, and under 90 degrees...
                if (dot >= 0 && Mathf.Abs(angle) < 180)
                {
                    //Check if we're a positive or negative angle
                    if (dot2 < 0)
                    {
                        angleSign = -1;
                    }
                    else
                    {
                        angleSign = 1;
                    }
                }
                //If we're on the other half of the circle, or the origin half and over 270 degrees...
                else if ((dot2 < 0 && angleSign > 0) || (dot2 > 0 && angleSign < 0))
                {
                    tempAngle = 180 + (180 - tempAngle);
                }

                angle = tempAngle * angleSign;
            }

            float distance1 = Vector3.Distance(startPoint, endPoint);
            float distance2 = Vector3.Distance(startPoint, anglePoint);

            float distance = (distance1 < distance2 ? distance1 : distance2) * 0.3f;

            Handles.DrawWireArc(startPoint, camForward, heading / heading.magnitude, angle, distance);
            Handles.DrawLine(startPoint, anglePoint);
        }

        static void DisplayMeasurement()
        {
            Rect area = new Rect(Event.current.mousePosition + new Vector2(15, 15), new Vector2(60, 30));

            string text;
            if (angleSign == 0)
            {
                text = distance.ToString() + "u";
            }
            else
            {
                text = angle.ToString() + "°";
            }

            GUIStyle guiStyle = new GUIStyle();
            guiStyle.alignment = TextAnchor.UpperLeft;
            guiStyle.normal.textColor = hh_Preferences.quickMeasures_colour;

            Handles.BeginGUI();
            GUI.Label(area, text, guiStyle);
            Handles.EndGUI();
        }

        static void AngleSnap(SceneView sceneView)
        {
            Vector3 referencePoint = startPoint + (state == State.SetLastPoint ? sceneView.camera.transform.right : (endPoint - startPoint).normalized);
            Vector3 camForward = sceneView.camera.transform.forward;
            Vector3 heading = (referencePoint - startPoint).normalized;

            Vector3 heading2 = ((state == State.SetLastPoint ? endPoint : anglePoint) - startPoint).normalized;
            Vector3 perpHeading = Quaternion.AngleAxis(90, camForward) * heading;

            float dot = Vector3.Dot(heading, heading2);
            float dot2 = Vector3.Dot(perpHeading, heading2);

            float tempAngle = Vector3.Angle(startPoint - referencePoint, startPoint - (state == State.SetLastPoint ? endPoint : anglePoint)); //Chnaged

            //If we're on the origin half of the circle, and under 90 degrees...
            if (dot >= 0 && Mathf.Abs(snapAngle) < 180)
            {
                //Check if we're a positive or negative angle
                if (dot2 < 0)
                {
                    snapAngleSign = -1;
                }
                else
                {
                    snapAngleSign = 1;
                }
            }
            //If we're on the other half of the circle, or the origin half and over 270 degrees...
            else if ((dot2 < 0 && snapAngleSign > 0) || (dot2 > 0 && snapAngleSign < 0))
            {
                tempAngle = 180 + (180 - tempAngle);
            }

            snapAngle = tempAngle * snapAngleSign; //Changed

            float rotationSnap = EditorPrefs.GetFloat("RotationSnap");
            snapAngle = (float)System.Math.Round(snapAngle / (double)rotationSnap, System.MidpointRounding.AwayFromZero) * rotationSnap;

            Vector3 result = Quaternion.AngleAxis(snapAngle, sceneView.camera.transform.forward) * 
                ((state == State.SetLastPoint ? sceneView.camera.transform.right : (endPoint - startPoint).normalized) * 
                Vector3.Distance(startPoint, (state == State.SetLastPoint ? endPoint : anglePoint)));

            if (state == State.SetLastPoint)
            {
                endPoint = startPoint + result;
            }
            else
            {
                anglePoint = startPoint + result;
                angle = snapAngle;
                angleSign = snapAngleSign;
            }
        }

        static void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.RepaintAll();
        }
    }
}
