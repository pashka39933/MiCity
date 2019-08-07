using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Helpers;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingPolygon : MonoBehaviour
{

    public string Id, Kind;

    public List<Vector3> verts;

    string[] landusesKinds = { "building", "beach", "bridge", "footway", "pedestrian", "place_of_worship", "playground", "recreation_ground", "sports_centre", "theme_park" };

    public bool Initialize(List<Vector3> verts, float zIndex, JSONObject properties, Material landuseMaterial)
    {
        this.verts = verts;
        this.Id = properties["id"].ToString();
        this.Kind = properties["kind"].ToString().Replace("\"", "");
        if (Array.IndexOf(landusesKinds, this.Kind) > -1)
        {
            this.GetComponent<MeshRenderer>().material = landuseMaterial;
            GetComponent<MeshFilter>().mesh = CreateMesh(verts, zIndex);
            return true;
        }
        else
            Destroy(this.gameObject);

        return false;
    }

    private static Mesh CreateMesh(List<Vector3> verts, float zIndex)
    {
        var tris = new Triangulator(verts.Select(x => new Vector2(x.x, x.y)).ToArray());
        var mesh = new Mesh();

        

        var vertices = verts.Select(x => new Vector3(x.x, x.y, zIndex)).ToList();
        var indices = tris.Triangulate().ToList();
        //var uv = new List<Vector2>();

        var n = vertices.Count;
        for (int index = 0; index < n; index++)
        {
            var v = vertices[index];
            vertices.Add(new Vector3(v.x, v.y, zIndex));
        }

        for (int i = 0; i < n - 1; i++)
        {
            indices.Add(i);
            indices.Add(i + n);
            indices.Add(i + n + 1);
            indices.Add(i);
            indices.Add(i + n + 1);
            indices.Add(i + 1);
        }

        indices.Add(n - 1);
        indices.Add(n);
        indices.Add(0);

        indices.Add(n - 1);
        indices.Add(n + n - 1);
        indices.Add(n);



        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
