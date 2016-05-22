using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ObjectType
{

    public virtual void DrawGUI()
    {

    }
    public virtual void DrawSceneGUI()
    {

    }
    public virtual List<object> Export()
    {
        return null;
    }
}
