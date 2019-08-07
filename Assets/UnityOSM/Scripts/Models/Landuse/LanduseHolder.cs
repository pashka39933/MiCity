using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LanduseHolder
{
    public GameObject GameObject { get; set; }
    public Vector3 Center { get; set; }
    public float Rotation { get; set; }
    public bool IsModelCreated;
    private List<Vector3> _verts;

    public LanduseHolder(Vector3 center, List<Vector3> verts)
    {
        IsModelCreated = false;
        Center = center;
        _verts = verts;
    }

    public GameObject CreateModel(JSONObject properties, float zIndex, Material landuseMaterial)
    {
        if (IsModelCreated)
            return null;

        var m = new GameObject().AddComponent<LandusePolygon>();
        m.gameObject.transform.position = Center;
        bool created = m.Initialize(_verts, zIndex, properties, landuseMaterial);
        IsModelCreated = true;
        if (!created)
            return null;
        return m.gameObject;
    }

}
