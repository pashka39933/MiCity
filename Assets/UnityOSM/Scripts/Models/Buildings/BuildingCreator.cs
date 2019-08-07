using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Helpers;
using UnityEngine;

public class BuildingCreator : MonoBehaviour {

    public Material landuseMaterial;

    Dictionary<Vector3, LanduseHolder> BuildingDictionary = new Dictionary<Vector3, LanduseHolder>();

    public GameObject CreateLanduses(JSONObject mapData, Vector2 posMER, Vector2 posUNITY)
    {

        GameObject landuseTile = new GameObject("Tile_Buildings");
        landuseTile.isStatic = true;
        landuseTile.transform.parent = this.transform;

        bool landuseCreated = false;

        foreach (var geo in mapData.list.Where(x => x["geometry"]["type"].str == "Polygon"))
        {
            var l = new List<Vector3>();
            for (int i = 0; i < geo["geometry"]["coordinates"][0].list.Count - 1; i++)
            {
                var c = geo["geometry"]["coordinates"][0].list[i];
                var bm = GM.LatLonToMeters(c[1].f, c[0].f);
                var pm = new Vector2(bm.x - posMER.x, bm.y - posMER.y) + posUNITY;
                l.Add(pm);
            }

            try
            {
                var center = l.Aggregate((acc, cur) => acc + cur) / l.Count;
                if (!BuildingDictionary.ContainsKey(center))
                {
                    var bh = new LanduseHolder(center, l);
                    for (int i = 0; i < l.Count; i++)
                    {
                        l[i] = l[i] - bh.Center;
                    }
                    BuildingDictionary.Add(center, bh);

                    var m = bh.CreateModel(geo["properties"], this.transform.position.z, landuseMaterial);
                    if (m)
                    {
                        m.name = "Building";
                        m.transform.parent = landuseTile.transform;
                        m.transform.localPosition = center;
                        landuseCreated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        if (!landuseCreated)
        {
            Destroy(landuseTile.gameObject);
            return null;
        }

        return landuseTile;
    }
}
