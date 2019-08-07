using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using System;

public class POITilesCreate : MonoBehaviour
{

    public Sprite[] POIsprites;
    public Transform userPos;
    public AnimationClip[] poiAnims;
    public UiController uiController;
    public RateButtonsController rtController;

	public List<string> POIsInfo = new List<string> ();

	private List<PoiPoint> POIs = new List<PoiPoint> ();

    /* POIs PARSING (returns true if at least one poi was created) */
    public GameObject CreatePOIs(JSONObject mapData, Vector2 posMER, Vector2 posUNITY, string preID)
    {

        /* CREATE POI TILE */
        GameObject POITile = new GameObject("Tile_POIs");
        POITile.isStatic = true;
        POITile.transform.parent = this.transform;

        /* FOR EACH POI */
        foreach (JSONObject geo in mapData.list)
        {
            /* CALCULATE POI POSITION */
            JSONObject coords = geo["geometry"]["coordinates"];
            Vector2 coordsMeters = GM.LatLonToMeters(coords[1].f, coords[0].f);
            Vector2 POI = new Vector2(coordsMeters.x - posMER.x, coordsMeters.y - posMER.y) + posUNITY; // UNITY POI COORDS

            /* PUT POI IN TILE */
            PoiPoint newPoi = new GameObject("POI").AddComponent<PoiPoint>();
            newPoi.gameObject.isStatic = true;
            newPoi.transform.SetParent(POITile.transform);
            try
            {
                if (newPoi.Initialize(new Vector2(coords[0].f, coords[1].f), preID + geo["properties"]["id"].n.ToString(), geo["properties"].ToDictionary(), POI, POIsprites, this.GetComponent<Canvas>(), userPos, poiAnims, uiController, rtController))
				{
					int idx = POIs.FindIndex(x => Extensions.CalcLevenshteinDistance(x.Name, newPoi.Name) == 0);
					if(idx == -1)
					{
						POIs.Add(newPoi);
						POIsInfo.Add(newPoi.Name);
					}
					else
					{
						Destroy(newPoi.gameObject);
					}
				}
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        return POITile;
    }

	public GameObject CreateGooglePOIs(JSONObject mapData, Vector2 posMER, Vector2 posUNITY, string preID)
	{

		/* CREATE POI TILE */
		GameObject POITile = new GameObject("Tile_POIs_Google");
		POITile.isStatic = true;
		POITile.transform.parent = this.transform;

		if (mapData == null || mapData.list == null)
			return POITile;

		/* FOR EACH POI */
		foreach (JSONObject geo in mapData.list)
		{

			/* CALCULATE POI POSITION */
			JSONObject coords = geo["geometry"]["location"];
			Vector2 coordsMeters = GM.LatLonToMeters(coords["lat"].f, coords["lng"].f);
			Vector2 POI = new Vector2(coordsMeters.x - posMER.x, coordsMeters.y - posMER.y) + posUNITY; // UNITY POI COORDS

			/* PUT POI IN TILE */
			PoiPoint newPoi = new GameObject("POI").AddComponent<PoiPoint>();
			newPoi.gameObject.isStatic = true;
			newPoi.transform.SetParent(POITile.transform);
			try
			{
				
				string types = geo["types"].ToString().Remove(0, 1);
				types = types.Remove(types.Length - 1).Replace("\"", "");
				Dictionary<string, string> poiDic = geo.ToDictionary();
				if(poiDic.ContainsKey("types"))
					poiDic["types"] = types;
				else
					poiDic.Add("types", types);
				
				if (newPoi.Initialize(new Vector2(coords[0].f, coords[1].f), preID + geo["place_id"].str, poiDic, POI, POIsprites, this.GetComponent<Canvas>(), userPos, poiAnims, uiController, rtController))
				{
					int idx = POIs.FindIndex(x => Extensions.CalcLevenshteinDistance(x.Name, newPoi.Name) == 0);
					if(idx == -1)
					{
						POIs.Add(newPoi);
						POIsInfo.Add(newPoi.Name);
					}
					else
					{
						Destroy(newPoi.gameObject);
					}
				}
				
			}
			catch (Exception ex)
			{
				Debug.Log(ex);
			}
		}
		return POITile;
	}
}
