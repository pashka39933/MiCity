using Assets;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Assets.Helpers;
using System;
using System.Text;
using UnityEngine.UI;
using CI.WSANative.Dialogs;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;

public class World : MonoBehaviour
{
    public Vector2 Center, CenterLONLAT; // USER's COORDS (MERCATOR!)
    public int Zoom;
    public GameObject cam;  // CAMERA (USER POSITION)

    public int tilesRange = 3; // HOW MUCH TILES LOAD AT START (tilesRange^2)
    private float tileWidth, tileHeight;
    Vector2 centerRectPos;  //POOSITION OF CENTER TILE
    Vector2 userTilePos = Vector2.zero;
    List<Vector2> loadedTiles = new List<Vector2>();
    public int tilesLeft;

    /* MAP ELEMENTS VARIABLES */
	public bool enableOSMPois, enableGooglePois, enableRoads, enableLanduse, enableBuildings;

    public POITilesCreate POIS;
    public RoadsTilesCreate ROADS;
    public LanduseCreator LANDUSE;
	public BuildingCreator BUILDINGS;

    public bool worldInitialized = false;

    public Text INFO;

    public void Initialize(Vector2 coordsLONLAT)
    {
        #if UNITY_EDITOR
		if(Extensions.CloudSync() != null)
            Extensions.CloudSync().UserPathCoordinates.Add(coordsLONLAT);
		else
			SceneManager.LoadScene(0);
        #endif

        this.Center = coordsLONLAT;
		this.CenterLONLAT = new Vector2(coordsLONLAT.y, coordsLONLAT.x);
        /* TILES LOADING COUNTER AND CONVERSION lonlat -> mercator */
        tilesLeft = tilesRange * tilesRange;
        Center = (GM.LonLatToMercator(Center.x, Center.y, Zoom));

        /* CALCULATING CENTER TILE POSITION, TILE WIDTH AND TILE HEIGHT */
        Rect center = GM.TileBounds(Center, Zoom);
        centerRectPos = new Vector2(center.x, center.y);
        tileWidth = center.width;
        tileHeight = center.height;

        worldInitialized = true;

		StartCoroutine (MapUpdateCoroutine (3));
    }

	/* FUNCTION LOADING GOOGLE POIs */
	public Coroutine googlePoisLoadCoroutine = null;
	public void LoadGooglePOIs(Vector2 pos)
	{
		if (enableGooglePois) 
		{
			if (googlePoisLoadCoroutine == null)
				googlePoisLoadCoroutine = StartCoroutine (LoadGooglePOIsCoroutine (0, "", pos));
		}
	}

	/* COROUTINE TO LOAD AND PARSE GOOGLE POIs */
	private IEnumerator LoadGooglePOIsCoroutine(int loadIndex, string nextPageToken, Vector2 pos)
	{
		string url = "";

		if (loadIndex == 0)
			url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?types=amusement_park|aquarium|art_gallery|library|liquor_store|bar|food|bowling_alley|mosque|cafe|museum|night_club|park|casino|church|city_hall|restaurant|shopping_mall|stadium|synagogue|gym|hindu_temple|zoo|sport|drink|nightlife|club|music|meal_delivery|meal_takeaway|place_of_worship|nature|history|entertainment|opera&location=" + (pos.y) + "," + (pos.x) + "&radius=75&key=AIzaSyCzw1lpILgiUWEpUjCyXQDYi_-kJDdyJew";
		else
			url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?pagetoken=" + nextPageToken + "&key=AIzaSyCzw1lpILgiUWEpUjCyXQDYi_-kJDdyJew";

		UnityWebRequest www = UnityWebRequest.Get(System.Uri.EscapeUriString (url));
		yield return www.Send();

		/* TILE POSITION IN MERCATOR COORDS */
		Vector2 tilePosMER = GM.TileBounds (Center, Zoom).center;

		/* TILE POSITION IN UNITY COORDS */
		Vector2 tilePosUNITY = new Vector2 (tilePosMER.x - centerRectPos.x, tilePosMER.y - centerRectPos.y);

		JSONObject mapData = new JSONObject (www.downloadHandler.text);
		POIS.CreateGooglePOIs (mapData ["results"], tilePosMER, tilePosUNITY, "google:");

		Debug.Log ("### Google POIs downloaded! (part: " + loadIndex + ") ###\n\n\nRESPOND: " + www.downloadHandler.text + " \n\n\nURL: " + url);

		if (mapData.HasField ("next_page_token")) 
		{
			yield return new WaitForSeconds (2.25f);
			googlePoisLoadCoroutine = StartCoroutine (LoadGooglePOIsCoroutine (++loadIndex, mapData ["next_page_token"].str, pos));
		} 
		else 
		{
			googlePoisLoadCoroutine = null;
		}
	}

	/* FUNCTION LOADING TILE, i - INDEX OF TILE FROM TILE_RANGE SPECIFIED ARRAY */
	public void loadTile(int i)
	{
		Vector2 tileRealPos = new Vector2(Center.x - tilesRange / 2 + i % tilesRange, Center.y - tilesRange / 2 + i / tilesRange);
		CreateTileCoroutineVar = StartCoroutine(CreateTile(tileRealPos));
	}

    /* COROUTINE DOWNLOADING AND PARSING TILE */
	public Coroutine CreateTileCoroutineVar = null;
    public IEnumerator CreateTile(Vector2 realPos)
    {

        string tilename = realPos.x + "_" + realPos.y;
        string tileurl = realPos.x + "/" + realPos.y;

		string tileJSON = "";

		string url = "https://tile.mapzen.com/mapzen/vector/v1/";
        if (enableRoads)
            url += "roads,";
        if (enableOSMPois)
            url += "pois,";
        if (enableLanduse)
            url += "landuse,";
		if (enableBuildings)
			url += "buildings,";
        url = url.Remove(url.Length - 1, 1);
        url = url + "/" + Zoom + "/";

        JSONObject mapData;

		loadedTiles.Add (realPos);

        /* Caching */
		if (File.Exists (Application.persistentDataPath + "/" + tilename)) 
		{
			StreamReader r = new StreamReader (Application.persistentDataPath + "/" + tilename, Encoding.Default);
			tileJSON = r.ReadToEnd ();
		} 
		else 
		{
			UnityWebRequest www = UnityWebRequest.Get(System.Uri.EscapeUriString (url + tileurl + ".json?api_key=mapzen-h3UEcwL"));

			//Debug.Log ("Downloading tile with URL: " + url + tileurl + ".json?api_key=mapzen-h3UEcwL");

			yield return www.Send();

			tileJSON = www.downloadHandler.text;
		}

        /* IF TILE WAS DOWNLOADED SUCCESSFULLY */
		if (tileJSON.Length > 0)
        {

			/* Cache save */
			if (!File.Exists (Application.persistentDataPath + "/" + tilename)) 
			{
				StreamWriter sr = File.CreateText (Application.persistentDataPath + "/" + tilename);
				sr.Write (tileJSON);
				sr.Close ();

				/* Cache cleaning */
				DirectoryInfo info = new DirectoryInfo (Application.persistentDataPath);
				FileInfo[] filesInfo = info.GetFiles ();
				if (filesInfo.Length > 128) 
				{
					DateTime min = DateTime.UtcNow;
					FileInfo lruFile = null;
					foreach (FileInfo file in filesInfo) 
					{
						if (file.LastAccessTimeUtc < min) 
						{
							min = file.LastAccessTimeUtc;
							lruFile = file;
						}
					}
					Debug.Log ("Clearing cache: " + lruFile.FullName);
					File.Delete (lruFile.FullName);
				}
			}

			mapData = new JSONObject (tileJSON);

            /* TILE POSITION IN MERCATOR COORDS */
            Vector2 tilePosMER = GM.TileBounds(realPos, Zoom).center;

            /* TILE POSITION IN UNITY COORDS */
            Vector2 tilePosUNITY = new Vector2(tilePosMER.x - centerRectPos.x, tilePosMER.y - centerRectPos.y);

            /* IF MAP ELEMENT WAS ENABLED IN INSPECTOR, CREATE IT AND ADD TO HELP DICITIONARY */
            if (enableRoads)
            {
				if(mapData["roads"] != null)
                	ROADS.CreateRoads(mapData["roads"]["features"], tilePosMER, tilePosUNITY);
            }

            if (enableOSMPois)
            {
				if(mapData["pois"] != null)
                	POIS.CreatePOIs(mapData["pois"]["features"], tilePosMER, tilePosUNITY, "osm:");
            }

            if (enableLanduse)
            {
				if(mapData["landuse"] != null)
                	LANDUSE.CreateLanduses(mapData["landuse"]["features"], tilePosMER, tilePosUNITY);
            }
			
			if (enableBuildings)
			{
				if(mapData["buildings"] != null)
					BUILDINGS.CreateLanduses(mapData["buildings"]["features"], tilePosMER, tilePosUNITY);
			}

            /* INFORM ABOUT TILE LOAD */
            tilesLeft--;
        }
        /* IF TILE WAS NOT DOWNLOADED PROPERLY */
        else
        {
			loadedTiles.Remove (realPos);
        }
		CreateTileCoroutineVar = null;
    }

	/* Coroutine aktualizacji mapy */
	IEnumerator MapUpdateCoroutine(float updateTime)
	{
		yield return new WaitUntil (() => (loadedTiles.Count >= tilesRange * tilesRange));
		while (true) 
		{
			/* IF USER REACHES NEXT TILE, LOAD MORE TILES */
			userTilePos.x = (int)(cam.transform.position.x / tileWidth);
			userTilePos.y = (int)(cam.transform.position.y / tileHeight);
			for (int i = 0; i < 25; i++) 
			{
				Vector2 tileCoords = Center + userTilePos;
				tileCoords.x += ((i % 5) - 2);
				tileCoords.y += ((i / 5) - 2);
				if (loadedTiles.FindIndex (x => (x.x == tileCoords.x && x.y == tileCoords.y)) < 0) 
				{
					yield return new WaitUntil (() => (CreateTileCoroutineVar == null));
					CreateTileCoroutineVar = StartCoroutine (CreateTile (tileCoords));
				}
			}
			yield return new WaitForSeconds (updateTime);
		}
	}
}