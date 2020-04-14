using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.AnimatedValues;

namespace HandyHacks
{
    [InitializeOnLoad]
    static class hh_OrientationGizmo
    {
        static hh_OrientationGizmo()
        {
            if (hh_Preferences.orientationGizmo_active && !enabled)
            {
                Toggle();
            }
        }

        static bool enabled = false;

        static Matrix4x4 handlesMatrix = Matrix4x4.identity;
        static AnimFloat alphaMaster = new AnimFloat(0);
        static float size = 1f;

        static SceneView sceneView;

        static GenericMenu contextMenu = new GenericMenu();
        static GUIContent projection = new GUIContent("Perspective");

        static GenericMenu contextMenu2D = new GenericMenu();

        static AxisControl[] axisControls = new AxisControl[]
        {
        new AxisControl(AxisControl.ControlName.right,  Quaternion.Euler(0, 270, 0),   Quaternion.Euler(0, 0, 270),     Handles.xAxisColor),
        new AxisControl(AxisControl.ControlName.top,    Quaternion.Euler(90, 0, 0),    Quaternion.identity,             Handles.yAxisColor),
        new AxisControl(AxisControl.ControlName.front,  Quaternion.Euler(0, 180, 0),   Quaternion.identity,             Handles.zAxisColor),

        new AxisControl(AxisControl.ControlName.left,   Quaternion.Euler(0, 90, 0),    Quaternion.Euler(0, 0, 90),      Color.white),
        new AxisControl(AxisControl.ControlName.bottom, Quaternion.Euler(-90, 0, 0),   Quaternion.Euler(0, 0, 180),     Color.white),
        new AxisControl(AxisControl.ControlName.back,   Quaternion.Euler(0, 0, 0),     Quaternion.identity,             Color.white)
        };

        public class AxisControl
        {
            public AxisControl(ControlName name, Quaternion rot, Quaternion rot2D, Color color)
            {
                this._controlName = name;
                this.rotation = rot;
                this.rotation2D = rot2D;
                this.colour = color;

                this.alpha.speed = 5f;
                this.alpha.value = 1f;
            }

            public enum ControlName { top, bottom, left, right, front, back };
            ControlName _controlName;
            public ControlName controlName
            {
                get { return _controlName; }
            }

            public Quaternion rotation = new Quaternion();
            public Quaternion rotation2D = new Quaternion();

            public AnimFloat alpha = new AnimFloat(1);
            public Color colour = new Color();
        }

        [MenuItem("Tools/Handy Hacks/Orientation Gizmo " + hh_Hotkeys.keyBinding_OrientationGizmo)]
        static void Toggle()
        {
            enabled = !enabled;

            if (enabled)
            {
                Construct();
            }
            else
            {
                Hide();
            }
        }

        static void SaveState(bool active)
        {
            hh_Preferences.orientationGizmo_active = active;
            hh_Preferences.SavePreferences();
        }

        static void CreateContextMenus()
        {
            //3D context menu
            contextMenu.AddItem(projection, false, UseCenterControl);
            contextMenu.AddSeparator(string.Empty);

            object o;
            string name = string.Empty;
            for (int i = 0; i < axisControls.Length; i++)
            {
                o = i;
                name = axisControls[i].controlName.ToString();
                name = char.ToUpper(name[0]) + name.Substring(1);
                contextMenu.AddItem(new GUIContent(name), false, MenuAxisControl, o);
            }

            contextMenu.AddSeparator(string.Empty);

            bool clockwise = true;
            o = clockwise;
            contextMenu.AddItem(new GUIContent("Rotate Clockwise"), false, MenuRotationControl, o);

            clockwise = false;
            o = clockwise;
            contextMenu.AddItem(new GUIContent("Rotate Counter-Clockwise"), false, MenuRotationControl, o);

            contextMenu.AddSeparator(string.Empty);
            contextMenu.AddItem(new GUIContent("Disable"), false, Hide);

            //2D context menu
            o = new object();
            name = string.Empty;
            for (int i = 0; i < axisControls.Length; i++)
            {
                if (i == 0 || i == 1 || i == 3 || i == 4)
                {
                    o = i;
                    name = axisControls[i].controlName.ToString();
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    contextMenu2D.AddItem(new GUIContent(name), false, MenuAxisControl2D, o);
                }
            }

            contextMenu2D.AddSeparator(string.Empty);

            clockwise = true;
            o = clockwise;
            contextMenu2D.AddItem(new GUIContent("Rotate Clockwise"), false, MenuRotationControl, o);

            clockwise = false;
            o = clockwise;
            contextMenu2D.AddItem(new GUIContent("Rotate Counter-Clockwise"), false, MenuRotationControl, o);

            contextMenu2D.AddSeparator(string.Empty);
            contextMenu2D.AddItem(new GUIContent("Disable"), false, Hide);
        }

        static void Construct()
        {
            //Save state
            SaveState(true);

            //Register Events. 
            SceneView.onSceneGUIDelegate -= Draw;
            SceneView.onSceneGUIDelegate += Draw;
            //If any of the alphas are animating, keep updating the scene views.
            alphaMaster.valueChanged.AddListener(new UnityAction(SceneView.RepaintAll));
            for (int i = 0; i < axisControls.Length; i++)
            {
                axisControls[i].alpha.valueChanged.AddListener(new UnityAction(SceneView.RepaintAll));
            }

            CreateContextMenus();

            //Fade in
            alphaMaster.speed = 2f;
            alphaMaster.target = 1f;
        }

        static void Destruct()
        {
            //Save State
            SaveState(false);

            SceneView.onSceneGUIDelegate -= Draw;

            alphaMaster.valueChanged.RemoveAllListeners();
            for (int i = 0; i < axisControls.Length; i++)
            {
                axisControls[i].alpha.valueChanged.RemoveAllListeners();
            }

            contextMenu = new GenericMenu();
            contextMenu2D = new GenericMenu();
        }

        static void Hide()
        {
            enabled = false;
            alphaMaster.target = 0f;
        }

        static Color GetAxisColour(int index)
        {
            return new Color
                (
                axisControls[index].colour.r,
                axisControls[index].colour.g,
                axisControls[index].colour.b,
                axisControls[index].alpha.value * alphaMaster.value
                );
        }

        #region Draw Functions

        static void SetupCamera(Rect area, Camera camera)
        {
            //Steal the scene view camera and set it up to draw our handles. 
            HandleUtility.PushCamera(camera);
            if (camera.orthographic)
            {
                camera.orthographicSize = 0.5f;
            }
            camera.cullingMask = 0;
            camera.transform.position = camera.transform.rotation * new Vector3(0f, 0f, -5f);
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;

            Handles.SetCamera(area, camera);
        }

        static void Draw(SceneView view)
        {
            //After Hide() menthod, listen for destruct.
            if (!enabled && alphaMaster.value <= 0)
            {
                Destruct();
            }

            //don't draw if view is in 2D mode; don't draw if master alpha is 0
            if (alphaMaster.value <= 0f)
            {
                return;
            }

            sceneView = view;


            if (view.in2DMode)
            {
                Draw2D();
                return;
            }

            Rect area = new Rect(sceneView.position.width - 180f, -16f, 100f, 100f);
            Camera camera = sceneView.camera;
            SetupCamera(area, camera);

            //handle context menu
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (area.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();

                    if (sceneView.orthographic)
                    {
                        projection.text = "Perspective";
                    }
                    else
                    {
                        projection.text = "Orthographic";
                    }

                    contextMenu.ShowAsContext();
                }
            }

            //Make our handles matrix local to the active transform.
            if (Selection.activeTransform != null)
            {
                handlesMatrix.SetColumn(0, Selection.activeTransform.right);
                handlesMatrix.SetColumn(1, Selection.activeTransform.up);
                handlesMatrix.SetColumn(2, Selection.activeTransform.forward);
            }
            else if (!handlesMatrix.isIdentity)
            {
                handlesMatrix = Matrix4x4.identity;
            }
            Handles.matrix = handlesMatrix;

            //Get size for handles
            size = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;

            //Axis Handles behind Center Handle
            DrawAxisControls(false);

            //Center Handle
            Handles.color = Handles.centerColor * new Color(1, 1, 1, alphaMaster.value);
            if (DrawHandleButton(Vector3.zero, Quaternion.identity, size * 0.8f, size, HandleCaps.Cube))
            {
                UseCenterControl();
            }

            //Axis Handles in front of Center Handle
            DrawAxisControls(true);

            //Put the camera back where you found it.
            HandleUtility.PopCamera(camera);
            Handles.SetCamera(camera);
        }

        static void DrawAxisControls(bool inFront)
        {
            string direction = "local";
            for (int i = 0; i < axisControls.Length; i++)
            {
                float dot = Vector3.Dot(sceneView.camera.transform.forward.normalized, handlesMatrix.GetColumn((i < 3 ? i : i - 3)) * (i < 3 ? -1 : 1));

                //Check if we're aligned to an axis. Allow for minor floating point deviation when gizmo's matrix isn't identity.
                if (dot > 0.999f)
                {
                    if (!inFront)
                    {
                        DrawRotationControls(i);
                    }
                    direction = axisControls[i].controlName.ToString();
                }

                if (inFront && dot >= 0.0f || !inFront && dot < 0.0f)
                {
                    //Fade/Show Handle in active view
                    if (SceneView.focusedWindow == sceneView)// || SceneView.mouseOverWindow == sceneView)
                    {
                        if ((dot >= 0.9f || dot <= -0.9f) && axisControls[i].alpha.target != 0f)
                        {
                            axisControls[i].alpha.target = 0f;
                        }
                        else if ((dot < 0.9f && dot > -0.9f) && axisControls[i].alpha.target != 1f)
                        {
                            axisControls[i].alpha.target = 1f;
                        }

                        Handles.color = GetAxisColour(i);
                    }
                    else //just snap to w/e colour fits the bill.
                    {
                        if ((dot >= 0.9f || dot <= -0.9f))
                        {
                            Handles.color = Color.clear;
                        }
                        else
                        {
                            Handles.color = axisControls[i].colour * new Color(1, 1, 1, alphaMaster.value);
                        }
                    }

                    //Draw
                    if (Handles.color.a > 0.0f)
                    {
                        if (DrawHandleButton(axisControls[i].rotation * Vector3.forward * -size * 1.2f, axisControls[i].rotation, size, size * 0.7f, HandleCaps.Cone))
                        {
                            UseAxisControl(i);
                        }
                    }
                }
            }

            //assign alignment status
            if (inFront)
            {
                DrawLabel(direction, new Rect(sceneView.position.width - 160, 80, 60, 20));
            }
        }

        static void DrawRotationControls(int i)
        {
            if (alphaMaster.value != 1)
            {
                Handles.color = new Color(Handles.centerColor.r, Handles.centerColor.g, Handles.centerColor.b, alphaMaster.value);
            }
            else
            {
                Handles.color = Handles.centerColor;
            }
            //counter-clockwise
            //ui
            Handles.DrawWireArc(Vector3.zero, axisControls[i].rotation * Vector3.forward, axisControls[i].rotation * new Vector3(1, 0.4f, 0), 35, 2 * size);
            DrawHandleCap(HandleCaps.Cone, 0, axisControls[i].rotation * new Vector3(1, 1.65f, 0) * size, axisControls[i].rotation * Quaternion.Euler(-30, -90, 0), size * 0.4f);

            //button
            if (DrawHandleButton(axisControls[i].rotation * new Vector3(1, 1, 0) * size * 1.2f, sceneView.camera.transform.rotation, 0, size * 0.5f, HandleCaps.Circle))
            {
                UseRotationControl(false);
            }

            //clockwise
            //ui
            Handles.DrawWireArc(Vector3.zero, axisControls[i].rotation * Vector3.forward, axisControls[i].rotation * new Vector3(-0.4f, -1, 0), -35, 2 * size);
            DrawHandleCap(HandleCaps.Cone, 0, axisControls[i].rotation * new Vector3(-1.65f, -1f, 0) * size, axisControls[i].rotation * Quaternion.Euler(-60, -90, 0), size * 0.4f);

            //button
            if (DrawHandleButton(axisControls[i].rotation * new Vector3(-1, -1, 0) * size * 1.2f, sceneView.camera.transform.rotation, 0, size * 0.5f, HandleCaps.Circle))
            {
                UseRotationControl(true);
            }
        }

        static void DrawLabel(string text, Rect drawArea, bool button = true)
        {
            string Text;
            if (sceneView.in2DMode)
            {
                Text = "2D " + text;
            }
            else if (sceneView.orthographic)
            {
                Text = "= " + text;
            }
            else
            {
                Text = "< " + text;
            }

            GUIStyle guiStyle = new GUIStyle();
            guiStyle.alignment = TextAnchor.LowerCenter;
            if (alphaMaster.value != 1)
            {
                guiStyle.normal.textColor = new Color(Handles.centerColor.r, Handles.centerColor.g, Handles.centerColor.b, alphaMaster.value);
            }
            else
            {
                guiStyle.normal.textColor = Handles.centerColor;
            }


            GUILayout.BeginArea(drawArea);
            if (button)
            {
                if (GUILayout.Button(Text, guiStyle))
                {
                    sceneView.LookAt(sceneView.pivot, sceneView.rotation, sceneView.size, !sceneView.orthographic);
                }
            }
            else
            {
                GUILayout.Label(Text, guiStyle);
            }
            GUILayout.EndArea();
        }

        enum HandleCaps { Cube, Cone, Circle };
        static void DrawHandleCap(HandleCaps capType, int controlID, Vector3 position, Quaternion rotation, float size)
        {
            #if UNITY_5_5_OR_NEWER
            switch (capType)
            {
                case HandleCaps.Cube:

                    Handles.CubeHandleCap(controlID, position, rotation, size, EventType.Repaint);

                    break;
                case HandleCaps.Cone:
                    
                    Handles.ConeHandleCap(controlID, position, rotation, size, EventType.Repaint);

                    break;

                case HandleCaps.Circle:

                    Handles.CircleHandleCap(controlID, position, rotation, size, EventType.Repaint);

                    break;
            }
            #else
            switch (capType)
            {
                case HandleCaps.Cube:

                    Handles.CubeCap(controlID, position, rotation, size);

                    break;
                case HandleCaps.Cone:

                    Handles.ConeCap(controlID, position, rotation, size);

                    break;
                case HandleCaps.Circle:

                    Handles.CircleCap(controlID, position, rotation, size);

                    break;
            }
            #endif
        }

        static bool DrawHandleButton(Vector3 position, Quaternion direction, float size, float pickSize, HandleCaps capType)
        {

            bool result = false;

            #if UNITY_5_5_OR_NEWER
            switch (capType)
            {
                case HandleCaps.Cube:

                    result = Handles.Button(position, direction, size, pickSize, Handles.CubeHandleCap);

                    break;
                case HandleCaps.Cone:

                    result = Handles.Button(position, direction, size, pickSize, Handles.ConeHandleCap);

                    break;

                case HandleCaps.Circle:

                    result = Handles.Button(position, direction, size, pickSize, Handles.CircleHandleCap);

                    break;
            }
            #else
            switch (capType)
            {
                case HandleCaps.Cube:

                    result = Handles.Button(position, direction, size, pickSize, Handles.CubeCap);

                    break;
                case HandleCaps.Cone:

                    result = Handles.Button(position, direction, size, pickSize, Handles.ConeCap);

                    break;
                case HandleCaps.Circle:

                    result = Handles.Button(position, direction, size, pickSize, Handles.CircleCap);

                    break;
            }
            #endif

            return result;
        }

        #endregion

        #region Draw 2D

        static void Draw2D()
        {
            Rect area = new Rect(sceneView.position.width - 80f, -20f, 100f, 100f);
            Camera camera = sceneView.camera;
            SetupCamera(area, camera);

            //handle context menu
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (area.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    contextMenu2D.ShowAsContext();
                }
            }

            //Make our handles matrix local to the active transform.
            if (Selection.activeTransform != null)
            {
                Quaternion q = Quaternion.LookRotation(camera.transform.forward, Selection.activeTransform.up);

                Vector3 v = q * Vector3.right;
                handlesMatrix.SetColumn(0, v);

                v = q * Vector3.up;
                handlesMatrix.SetColumn(1, v);

                v = q * Vector3.forward;
                handlesMatrix.SetColumn(2, v);
            }
            else if (!handlesMatrix.isIdentity)
            {
                handlesMatrix = Matrix4x4.identity;
            }
            Handles.matrix = handlesMatrix;

            //Get size for handles
            size = HandleUtility.GetHandleSize(Vector3.zero) * 0.2f;

            DrawAxisControls2D();
            //DrawRotationControls(5);

            //Center Handle
            Handles.color = Handles.centerColor * new Color(1, 1, 1, alphaMaster.value);

#if UNITY_5_6_OR_NEWER
            Handles.SphereHandleCap(0, Vector3.zero, Quaternion.identity, size, EventType.Ignore);
#else
            Handles.SphereCap(0, Vector3.zero, Quaternion.identity, size);
#endif

            //Draw Label
            float angle = Vector3.Angle(Vector3.up, camera.transform.up);
            
            string sign = "";
            float z = camera.transform.localEulerAngles.z;
            if (z > 180) z -= 360;
            if (z < 0) sign = "-";
            string text = sign + string.Format("{0:0.0}", angle) + "°";

            DrawLabel(text, new Rect(sceneView.position.width - 70, 80, 60, 20), false);

            //Put the camera back where you found it.
            HandleUtility.PopCamera(camera);
            Handles.SetCamera(camera);
        }

        static void DrawAxisControls2D()
        {

            for (int i = 0; i < axisControls.Length; i++)
            {
                //0 = right
                //1 = top
                //3 = left
                //4 = bottom
                if (i == 0 || i == 1 || i == 3 || i == 4)
                {

                    Handles.color = axisControls[i].colour * new Color(1, 1, 1, alphaMaster.value);

                    //get opposite rotation for cones
                    int j = i + 3;
                    if (j > 5) j -= 6;

                    if (DrawHandleButton(axisControls[i].rotation * Vector3.forward * -size, axisControls[j].rotation, size, size * 0.7f, HandleCaps.Cone))
                    {
                        UseAxisControl2D(i);
                    }

                }
            }
        }

        #endregion

        #region UseActions

        static void UseCenterControl()
        {
            sceneView.LookAt(sceneView.pivot, sceneView.rotation, sceneView.size, !sceneView.orthographic);
        }

        static void MenuAxisControl(object o)
        {
            UseAxisControl((int)o);
        }

        static void UseAxisControl(int i)
        {
            sceneView.LookAt(sceneView.pivot, Quaternion.LookRotation(handlesMatrix.GetColumn(2), handlesMatrix.GetColumn(1)) * axisControls[i].rotation, sceneView.size, sceneView.orthographic);
        }

        static void MenuRotationControl(object o)
        {
            UseRotationControl((bool)o);
        }

        static void UseRotationControl(bool clockwise)
        {
            sceneView.LookAt(sceneView.pivot, Quaternion.LookRotation(sceneView.camera.transform.forward, sceneView.camera.transform.right * (clockwise ? -1 : 1)));
        }

        #endregion

        #region 2D Actions

        static void MenuAxisControl2D(object o)
        {
            UseAxisControl2D((int)o);
        }

        static void UseAxisControl2D(int i)
        {

            sceneView.LookAt(sceneView.pivot, Quaternion.LookRotation(handlesMatrix.GetColumn(2), handlesMatrix.GetColumn(1)) * axisControls[i].rotation2D, sceneView.size, sceneView.orthographic);
        }

        #endregion

    }
}
