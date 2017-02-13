using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class UtilsGUI {


    public static void drawSittingPersonGizmo(Transform transform, int controlID)
    {
        Handles.SphereCap(controlID, transform.position, transform.rotation, 0.05f);
        Vector3 vector = transform.position - transform.up * 0.02f + transform.forward * 0.078f - transform.right * 0.045f;
        Handles.SphereCap(controlID, vector, transform.rotation, 0.03f);
        Vector3 vector2 = transform.position - transform.up * 0.02f + transform.forward * 0.078f + transform.right * 0.045f;
        Handles.SphereCap(controlID, vector2, transform.rotation, 0.03f);
        Vector3 center = transform.position + transform.up * 0.305f + transform.forward * 0.03f;
        Handles.SphereCap(controlID, center, transform.rotation, 0.1f);
        Vector3 center2 = vector + transform.forward * 0.015f - transform.up * 0.07f;
        Handles.SphereCap(controlID, center2, transform.rotation, 0.02f);
        Vector3 center3 = vector2 + transform.forward * 0.015f - transform.up * 0.07f;
        Handles.SphereCap(controlID, center3, transform.rotation, 0.02f);
    }
}
