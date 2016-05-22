using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BoundingBox
{
    public Bounds bounds;
    private Bounds liveBounds;
}