using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using Assets.Helpers;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Assets;

[RequireComponent(typeof(Image), typeof(Animation))]

public class PoiPoint : MonoBehaviour
{

    public string Id;
    public string Kind, Name, CategoryName;
    public Vector2 Coords;
    public int categoryID = -1;

    Animation anim;
    Image img;
    Transform userPosition;
    PoiCollect poiCollect;
    int categoriesNum = 8;
    UiController uiController;
    RateButtonsController rtController;

    /* CATEGORIES ARRAYS */
	string[] culture = { "hindu_temple", "synagogue", "city_hall", "church", "movie_theater", "movie_rental", "mosque", "book_store", "art_gallery", "artwork", "books", "cinema", "community_centre", "educational_institution", "library", "museum", "place_of_worship", "religion", "theatre", "university" };
	string[] entertainment = { "amusement_park", "amusement_ride", "attraction", "beach", "beach_resort", "carousel", "hanami", "maze", "petting_zoo", "picnic_site", "playground", "resort", "roller_coaster", "summer_toboggan", "theme_park", "water_slide" };
	string[] food = { "bbq", "meal_takeaway", "meal_delivery", "bakery", "bed_and_breakfast", "confectionery", "fast_food", "ice_cream", "restaurant" };
	string[] history = { "battlefield", "alpine_hut", "archaeological_site", "lighthouse", "memorial", "wilderness_hut", "windmill" };
	string[] nightlife = { "bar", "hazard", "liquor_store", "convenience_store", "casino", "night_club", "pub", "music", "club" };
	string[] sports = { "dive_centre", "gym", "bowling_alley", "bicycle_rental", "fitness", "fitness_station", "ski", "ski_rental", "ski_school", "sports", "sports_centre", "stadium" };
	string[] drink = { "biergarten", "alcohol", "brewery", "cafe", "drinking_water", "winery", "wine" };
	string[] nature = { "zoo", "park", "animal", "aquarium", "aviary", "cave_entrance", "landmark", "peak", "spring", "viewpoint", "volcano", "wildlife_park" };


    /* UNICODE -> NORMAL TEXT */
    public static string ParseUnicodeEscapes(string escapedString)
    {
        const string literalBackslashPlaceholder = "\uf00b";
        const string unicodeEscapeRegexString = @"(?:\\u([0-9a-fA-F]{4}))|(?:\\U([0-9a-fA-F]{8}))";
        // Replace escaped backslashes with something else so we don't
        // accidentally expand escaped unicode escapes.
        string workingString = escapedString.Replace("\\\\", literalBackslashPlaceholder);

        // Replace unicode escapes with actual unicode characters.
        workingString = new Regex(unicodeEscapeRegexString).Replace(workingString,
            match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber))
            .ToString());

        // Replace the escaped backslash placeholders with non-escaped literal backslashes.
        workingString = workingString.Replace(literalBackslashPlaceholder, "\\");
        return workingString;
    }


    /* POIs STYLING */
    public bool Initialize(Vector2 coord, string id, Dictionary<string, string> properties, Vector2 pos, Sprite[] POIsprites, Canvas poiCanvas, Transform userPos, AnimationClip[] POIAnims, UiController uiController, RateButtonsController rtController)
    {

        this.Id = id;
        this.Coords = coord;
        this.userPosition = userPos;
        this.uiController = uiController;
        this.rtController = rtController;

		if (properties.ContainsKey ("kind")) 
		{
			this.Kind = properties ["kind"];
		} 
		else if (properties.ContainsKey ("types")) 
		{
			string[] splitted = properties ["types"].Split (',');
			this.Kind = "";
			foreach (string str in splitted) 
			{
				if (str != "point_of_interest" && str != "establishment")
					this.Kind = this.Kind + str + ",";
			}
			if(this.Kind.Length > 0)
				this.Kind = this.Kind.Remove (this.Kind.Length - 1);
		}

        if (properties.ContainsKey("name:en"))
        {
            this.Name = properties["name:en"];
        }
        else if (properties.ContainsKey("name"))
        {
            this.Name = properties["name"];
        }
        else if (properties.ContainsKey("cuisine"))
        {
            this.Name = properties["cuisine"].Replace("_", " ").ToUpperInvariant();
        }
        else if (properties.ContainsKey("attraction"))
        {
            this.Name = properties["attraction"].Replace("_", " ").ToUpperInvariant();
        }

		if (this.Name == null || this.Name.Length == 0) 
		{
			Destroy(this.gameObject);
			return false;
		}
			
        this.Name = ParseUnicodeEscapes(this.Name);

        anim = this.GetComponent<Animation>();
        anim.AddClip(POIAnims[0], "in");
        anim.AddClip(POIAnims[1], "out");
        anim.AddClip(POIAnims[2], "collect");
        img = this.GetComponent<Image>();
        Sprite[] poiSpriteSet = new Sprite[3];

        /* CHECKING POI CATEGORY */
		if (Array.IndexOf (culture, Kind) > -1 || culture.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 0;
			CategoryName = "Culture";
		} 
		else if (Array.IndexOf (history, Kind) > -1 || history.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 4;
			CategoryName = "History";
		} 
		else if (Array.IndexOf (nature, Kind) > -1 || nature.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 5;
			CategoryName = "Nature";
		} 
		else if (Array.IndexOf (sports, Kind) > -1 || sports.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 7;
			CategoryName = "Sport";
		}
		else if (Array.IndexOf (food, Kind) > -1 || food.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 3;
			CategoryName = "Food";
		} 
		else if (Array.IndexOf (nightlife, Kind) > -1 || nightlife.Intersect (Kind.Split (',')).Any ())
		{
			categoryID = 6;
			CategoryName = "Nightlife";
		} 
		else if (Array.IndexOf (drink, Kind) > -1 || drink.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 2;
			CategoryName = "Drink";
		} 
		else if (Array.IndexOf (entertainment, Kind) > -1 || entertainment.Intersect (Kind.Split (',')).Any ()) 
		{
			categoryID = 1;
			CategoryName = "Entertainment";
		} 

        /* IF POI CATEGORY WAS ASSIGNED */
        if (categoryID > -1)
        {

            poiSpriteSet[0] = POIsprites[categoryID];
            poiSpriteSet[1] = POIsprites[categoryID + categoriesNum];
            poiSpriteSet[2] = POIsprites[categoryID + categoriesNum * 2];

            /* NORMAL POI STYLE, SIZE, POSITION */
            img.sprite = poiSpriteSet[0];
            this.transform.position = new Vector3(pos.x, pos.y, -5);
            this.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            /* POI FILL */
            GameObject poiFill = (GameObject)Instantiate(this.gameObject, this.transform.position, this.transform.rotation);
            poiFill.isStatic = true;
            poiFill.transform.SetParent(this.transform);
            poiCollect = poiFill.AddComponent<PoiCollect>();
            poiCollect.Initialize(poiSpriteSet[1], poiSpriteSet[2], this.Id, this.CategoryName);

            /* ------------------------------------------------------------------------------CLOUD!!!---------------------------------------------------------------------- */
            CloudSync cs = Extensions.CloudSync();
            if (cs.AllCollectedPois.Contains(this.Id))
            {
                onScreen = true;
                poiCollect.instantCollectPOI();
            }
            /*-------------------------------------------------------------------------------------------------------------------------------------CLOUD!-------------------*/

            return true;

        }
        /* IF POI CATEGORY WASN'T ASSIGNED, JUST DESTROY IT */
        else
        {
            Destroy(this.gameObject);
            return false;
        }
    }

    public bool onScreen = false, onRate = false;

    void Update()
    {

        img.enabled = true;
        this.transform.GetChild(0).gameObject.SetActive(true);

        /* POI IS INVISIBLE */
        if (!onScreen)
        {
            if (Vector2.Distance(this.transform.position, userPosition.position) < 100)
            {
                anim.Play("in");
                onScreen = true;
            }
        }
        /* POI IS VISIBLE */
        else
        {
            this.transform.rotation = userPosition.rotation;

            if (!poiCollect.isCollected)
            {
                if (Vector2.Distance(this.transform.position, userPosition.position) > 100)
                {
                    anim.Play("out");
                    onScreen = false;
                }
                else if (Vector2.Distance(this.transform.position, userPosition.position) < 65)
                {
                    poiCollect.collectPOI(0.3f);
                }
                else
                {
                    poiCollect.cancelCollectPOI();
                }
            }
            else
            {
                PoiRate pr = this.GetComponentInChildren<PoiRate>();
                if (Vector2.Distance(this.transform.position, userPosition.position) < 65)
                {
                    if (!onRate)
                    {
                        pr.enableRating();
                        rtController.ratedPOIS[categoryID].Add(this);
                        onRate = true;
                    }
                }
                else
                {
					if (onRate && !uiController.isUIActive)
                    {
                        pr.disableRating();
                        rtController.ratedPOIS[categoryID].Remove(this);
                        onRate = false;
                    }
                }

            }
        }

        if (!uiController.poiFilters[categoryID])
        {
            rtController.ratedPOIS[categoryID].Remove(this);
            onRate = false;
            img.enabled = false;
            poiCollect.instantCancelCollectPOI();
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
