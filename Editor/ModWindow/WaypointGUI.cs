using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class WaypointGUI {


    public static void OnWaypointGUI(ModWindow Window)
    {
        string caption = "Enable Editing Waypoints";
        if (Window.enableEditing)
            caption = "Disable Editing Waypoints";
        if (Window.enableEditing)
            GUI.color = Color.green;
        bool currentEnableEditing = Window.enableEditing;
        if (GUILayout.Button(caption))
        {
            Window.selectedWaypoint = null;
            Window.enableEditing = !Window.enableEditing;
        }
        if (Window.enableEditing)
            GUI.color = Color.white;
        if (currentEnableEditing != Window.enableEditing)
        {
            if (Window.enableEditing)
            {
                Window.currentTool = Tools.current;
                Tools.current = Tool.None;
            }
            else
                Tools.current = Window.currentTool;
        }
        if (Window.enableEditing)
        {
            GUILayout.Label("S - Snap to axis of connected waypoints");
            Window.helperPlaneY = EditorGUILayout.FloatField("Helper Plane Y", Window.helperPlaneY);

            if (GUILayout.Button("Generate outer grid"))
                generateOuterGrid(Window);
            if (GUILayout.Button("(A)dd Waypoint"))
                addWaypoint(Window);
            if (GUILayout.Button("Rotate 90°"))
                rotateWaypoints(Window);
            if (GUILayout.Button("Clear all"))
                Window.ModManager.asset.waypoints.Clear();
        }

    }

    public static void OnSceneGUI(ModWindow Window)
    {
        EditorUtility.SetDirty(Window.ModManager);
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.black;


        int controlID = GUIUtility.GetControlID(Window.GetHashCode(), FocusType.Passive);
        Tools.current = Tool.None;


        switch (Event.current.type)
        {
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.mouseUp:
                if (Event.current.button == 0)
                {
                    handleClick(Window);
                }
                break;
            case EventType.keyDown:
                if (Event.current.keyCode == KeyCode.S)
                {
                    Window.ModManager.asset.snap = true;
                }
                break;
            case EventType.keyUp:
                if (Event.current.keyCode == KeyCode.C)
                {
                    if (Window.state != ModWindow.State.CONNECT)
                    {
                        Window.state = ModWindow.State.CONNECT;
                    }
                    else
                    {
                        Window.state = ModWindow.State.NONE;
                    }
                }
                else if (Event.current.keyCode == KeyCode.R)
                {
                    removeSelectedWaypoint(Window);
                }
                else if (Event.current.keyCode == KeyCode.A)
                {
                    addWaypoint(Window);
                }
                else if (Event.current.keyCode == KeyCode.O && Window.selectedWaypoint != null)
                {
                    Window.selectedWaypoint.isOuter = !Window.selectedWaypoint.isOuter;
                }
                else if (Event.current.keyCode == KeyCode.I && Window.selectedWaypoint != null)
                {
                    Window.selectedWaypoint.isRabbitHoleGoal = !Window.selectedWaypoint.isRabbitHoleGoal;
                }
                else if (Event.current.keyCode == KeyCode.S)
                {
                    Window.ModManager.asset.snap = false;
                }

                SceneView.RepaintAll();
                HandleUtility.Repaint();
                break;
        }

        int i = 0;
        foreach (Waypoint waypoint in Window.ModManager.asset.waypoints)
        {
            if (waypoint == Window.selectedWaypoint)
            {
                Handles.color = Color.red;
            }
            else if (waypoint.isOuter)
            {
                Handles.color = Color.green;
            }
            else if (waypoint.isRabbitHoleGoal)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.yellow;
            }
            Vector3 worldPos = waypoint.localPosition + Window.ModManager.asset.gameObject.transform.position;
            Handles.SphereCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.2f);

            Handles.color = Color.blue;
            foreach (int connectedIndex in waypoint.connectedTo)
            {
                Handles.DrawLine(worldPos, Window.ModManager.asset.waypoints[connectedIndex].localPosition + Window.ModManager.asset.gameObject.transform.position);
            }

            Handles.Label(worldPos, "#" + i, labelStyle);
            i++;
        }

        if (Window.selectedWaypoint != null)
        {
            Vector3 worldPos = Window.selectedWaypoint.localPosition + Window.ModManager.asset.gameObject.transform.position;
            Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity) - Window.ModManager.asset.gameObject.transform.position;
            Window.selectedWaypoint.localPosition = handleSnap(newPos, Window.selectedWaypoint, Window);

            if (Window.state == ModWindow.State.CONNECT)
            {
                Handles.Label(worldPos, "\nConnecting...", labelStyle);
            }
            else
            {
                Handles.Label(worldPos, "\n(C)onnect\n(R)emove\n(O)uter\nRabb(i)t Hole", labelStyle);
            }
        }
    }
    public static void handleClick(ModWindow Window)
    {

        Waypoint closestWaypoint = null;
        EditorGUI.BeginChangeCheck();
        foreach (Waypoint waypoint in Window.ModManager.asset.waypoints)
        {
            Vector2 guiPosition = HandleUtility.WorldToGUIPoint(waypoint.localPosition + Window.ModManager.asset.gameObject.transform.position);
            if ((guiPosition - Event.current.mousePosition).magnitude < 15)
            {
                closestWaypoint = waypoint;
                break;
            }
        }



        if (Window.state == ModWindow.State.NONE && closestWaypoint != null)
        {
            Window.selectedWaypoint = closestWaypoint;
        }
        else if (Window.state == ModWindow.State.CONNECT && Window.selectedWaypoint != null)
        {
            int closestWaypointIndex = Window.ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
            {
                return wp == closestWaypoint;
            });
            int selectedWaypointIndex = Window.ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
            {
                return wp == Window.selectedWaypoint;
            });
            if (closestWaypointIndex >= 0 && selectedWaypointIndex >= 0)
            {
                if (!Window.selectedWaypoint.connectedTo.Contains(closestWaypointIndex))
                {
                    Window.selectedWaypoint.connectedTo.Add(closestWaypointIndex);
                    closestWaypoint.connectedTo.Add(selectedWaypointIndex);
                }
                else
                {
                    Window.selectedWaypoint.connectedTo.Remove(closestWaypointIndex);
                    closestWaypoint.connectedTo.Remove(selectedWaypointIndex);
                }
            }
        }

    }

    public static void addWaypoint(ModWindow Window)
    {
        Vector3 pos = Window.ModManager.asset.gameObject.transform.position;
        Window.selectedWaypoint = new Waypoint();

        if (Camera.current != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, Window.helperPlaneY, 0));
            float enter = 0;
            plane.Raycast(ray, out enter);
            Window.selectedWaypoint.localPosition = ray.GetPoint(enter) - pos;
        }
        else
        {
            Window.selectedWaypoint.localPosition = new Vector3(0, Window.helperPlaneY, 0);
        }

        Window.ModManager.asset.waypoints.Add(Window.selectedWaypoint);
    }

    public static void rotateWaypoints(ModWindow Window)
    {
        Vector3 pos = Window.ModManager.asset.gameObject.transform.position;

        foreach (Waypoint waypoint in Window.ModManager.asset.waypoints)
        {
            Vector3 dir = waypoint.localPosition - pos;
            dir.y = 0;
            float phi = Mathf.Atan2(dir.z, dir.x);
            phi += Mathf.PI / 2;
            float x = pos.x + dir.magnitude * Mathf.Cos(phi);
            float z = pos.z + dir.magnitude * Mathf.Sin(phi);
            waypoint.localPosition = new Vector3(x, waypoint.localPosition.y, z);
        }

    }

    public static void removeSelectedWaypoint(ModWindow Window)
    {

        int selectedWaypointIndex = Window.ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
        {
            return wp == Window.selectedWaypoint;
        });
        foreach (Waypoint waypoint in Window.ModManager.asset.waypoints)
        {
            waypoint.connectedTo.Remove(selectedWaypointIndex);
        }
        Window.ModManager.asset.waypoints.Remove(Window.selectedWaypoint);

        foreach (Waypoint waypoint in Window.ModManager.asset.waypoints)
        {
            for (int i = 0; i < waypoint.connectedTo.Count; i++)
            {
                if (waypoint.connectedTo[i] > selectedWaypointIndex)
                {
                    waypoint.connectedTo[i]--;
                }
            }
        }

        Window.selectedWaypoint = null;

    }

    public static Vector3 handleSnap(Vector3 newPos, Waypoint waypoint, ModWindow Window)
    {
        Vector3 oldPos = waypoint.localPosition;

        if (Window.ModManager.asset.snap && (newPos - oldPos).magnitude > Mathf.Epsilon)
        {
            if (Mathf.Abs(newPos.x - oldPos.x) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 0, Window);
            }
            if (Mathf.Abs(newPos.y - oldPos.y) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 1, Window);
            }
            if (Mathf.Abs(newPos.z - oldPos.z) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 2, Window);
            }
        }

        return newPos;
    }

    public static Vector3 handleAxisSnap(Vector3 newPos, Waypoint waypoint, int axisIndex, ModWindow Window)
    {

        foreach (int connectedIndex in waypoint.connectedTo)
        {
            Waypoint connectedWaypoint = Window.ModManager.asset.waypoints[connectedIndex];
            if (Mathf.Abs(newPos[axisIndex] - connectedWaypoint.localPosition[axisIndex]) < 0.1f)
            {
                newPos[axisIndex] = connectedWaypoint.localPosition[axisIndex];
            }
        }

        return newPos;
    }

    public static void generateOuterGrid(ModWindow Window)
    {
        float minX = -Window.ModManager.asset.XSize / 2;
        float maxX = Window.ModManager.asset.XSize / 2;
        float minZ = -Window.ModManager.asset.ZSize / 2;
        float maxZ = Window.ModManager.asset.ZSize / 2;
        for (int xi = 0; xi < Mathf.RoundToInt(maxX - minX); xi++)
        {
            for (int zi = 0; zi < Mathf.RoundToInt(maxZ - minZ); zi++)
            {
                float x = minX + xi;
                float z = minZ + zi;
                if (!(x == minX || x == maxX - 1) && !(z == minZ || z == maxZ - 1))
                {
                    continue;
                }
                Waypoint newWaypoint = new Waypoint();
                newWaypoint.localPosition = new Vector3(x + 0.5f, Window.helperPlaneY, z + 0.5f);
                newWaypoint.isOuter = true;
                //if (waypoints.waypoints.Count > 0) {
                //newWaypoint.connectedTo.Add(waypoints.waypoints.Count - 1);
                //}
                Window.ModManager.asset.waypoints.Add(newWaypoint);
            }
        }

    }
}
