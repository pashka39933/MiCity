using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Helpers;
using UnityEngine;

class RoadPolygon : MonoBehaviour
{
    public double Id;
    private LineRenderer lr;

    /* INITIALIZING AND DRAWING ROAD (verts.Length == 2) */
    public void Initialize(double id, Dictionary<string, string> properties, Vector2 pos, List<Vector3> verts)
    {
        Id = id;

        float streetWidth = 0.0f;
        Color streetColor = Color.white;

        List<string> roads = new List<string>(new string[] { "motorway", "trunk", "primary", "secondary", "tertiary", "unclassified", "motorway_link", "trunk_link", "secondary_link", "tertiary_link", "residential", "service" });
        List<string> paths = new List<string>(new string[] { "living_street", "pedestrian", "footway", "steps", "path" });
        List<string> rekts = new List<string>(new string[] { "cycleway" });

        // RAIL I FERRY ITP

        string HW = properties["kind_detail"];
        
		if (roads.Contains(HW))
        {
            streetWidth = 5;
            streetColor = new Color(0.8f, 0.8f, 0.8f);
        }
        else if (paths.Contains(HW))
        {
            streetWidth = 1.5f;
            streetColor = new Color(0.9f, 0.9f, 0.9f);
        }
        else if (rekts.Contains(HW))
        {
            streetWidth = 0.0f;
        }

        int zIndex = 0;
        int.TryParse(properties["sort_rank"], out zIndex);

        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 position = (Vector3)pos + verts[i];
            position.z = - (float)zIndex / 10000f;
            GameObject tmp = new GameObject("Vertex" + i.ToString());
            tmp.isStatic = true;
            tmp.transform.parent = this.transform;
            tmp.transform.position = position;
            if (i > 0 && verts[i].z == verts[i - 1].z)
            {
                LineRenderer lr = tmp.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.SetVertexCount(2);
                lr.SetWidth(streetWidth, streetWidth);
                lr.SetColors(streetColor, streetColor);
				lr.numCapVertices = 6;

                Vector3 pos1 = (Vector3)pos + verts[i];
                Vector3 pos2 = (Vector3)pos + verts[i - 1];
                pos1.z = -(float)zIndex / 10000f;
                pos2.z = -(float)zIndex / 10000f;
                lr.SetPosition(0, pos1);
                lr.SetPosition(1, pos2);
            }
        }
    }
}
