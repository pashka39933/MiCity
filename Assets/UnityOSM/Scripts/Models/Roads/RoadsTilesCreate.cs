using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using System;

public class RoadsTilesCreate : MonoBehaviour {

    /* ROADS PARSING */
    public GameObject CreateRoads(JSONObject mapData, Vector3 posMER, Vector2 posUNITY)
    {

        /* CREATE ROADS TILE */
        GameObject roadsTile = new GameObject("Tile_Roads");
        roadsTile.isStatic = true;
        roadsTile.transform.parent = this.transform;

        /* FOR EVERY ROAD FROM JSON */
        foreach (JSONObject geo in mapData.list)
        {
            /* LIST CONTAINING EACH ROAD VERTICES */
            List<Vector3> verticesList = new List<Vector3>();
            /* FOR EVERY ROAD */
            for (int i = 0; i < geo["geometry"]["coordinates"].list.Count; i++)
            {
                /* COUNTING COORDS */
                JSONObject JSON = geo["geometry"]["coordinates"][i];
                if (JSON[0].type == JSONObject.Type.NUMBER)
                {
                    Vector2 coordsMeters = GM.LatLonToMeters(JSON[1].f, JSON[0].f);
                    Vector3 coordsUnity = new Vector3(coordsMeters.x - posMER.x, coordsMeters.y - posMER.y);
                    verticesList.Add(coordsUnity);
                }
                else
                {
                    float zPos = 0;
                    if (verticesList.Count > 0)
                    {
                        zPos = verticesList[verticesList.Count - 1].z + 1;
                    }
                    for (int j = 0; j < JSON.list.Count; j++)
                    {
                        JSONObject coords = JSON[j];
                        Vector3 coordsMeters = GM.LatLonToMeters(coords[1].f, coords[0].f);
                        Vector3 coordsUnity = new Vector3(coordsMeters.x - posMER.x, coordsMeters.y - posMER.y);
                        coordsUnity.z = zPos;
                        verticesList.Add(coordsUnity);
                    }
                }
            }


            string roadID = geo["properties"]["id"].n.ToString();
            GameObject tmp = GameObject.Find(roadID);
            if (!tmp)
            {
                tmp = new GameObject(roadID);
                tmp.isStatic = true;
                tmp.AddComponent<RoadPolygon>();
            }

            RoadPolygon roadPoly = tmp.GetComponent<RoadPolygon>();
            roadPoly.transform.parent = roadsTile.transform;

            try
            {
                roadPoly.Initialize(geo["properties"]["id"].n, geo["properties"].ToDictionary(), posUNITY, verticesList);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        if (roadsTile.transform.childCount == 0)
        {
            Destroy(roadsTile.gameObject);
            return null;
        }
        return roadsTile;
    }
}
