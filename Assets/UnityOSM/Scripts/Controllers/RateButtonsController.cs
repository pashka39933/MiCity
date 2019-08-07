using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Assets.Helpers;
using System.Xml;
using System.IO;
using UnityEngine.Networking;

public class RateButtonsController : MonoBehaviour
{

    public List<PoiPoint>[] ratedPOIS = {
                                            new List<PoiPoint>(), // CULTURE
                                            new List<PoiPoint>(), // ENTERTAINMENT
                                            new List<PoiPoint>(), // DRINK
                                            new List<PoiPoint>(), // FOOD
                                            new List<PoiPoint>(), // HISTORY
                                            new List<PoiPoint>(), // NATURE
                                            new List<PoiPoint>(), // NIGHTLIFE
                                            new List<PoiPoint>(), // SPORT
                                        };

    public Image[] buttons;
    public Color[] poiColors;
    int[] buttonsIDs = { 0, 1, 2, 3, 4, 5, 6, 7 };
    Vector2[] buttonsPositions = new Vector2[8];
    float[] buttonsAlphas = new float[8];
    bool[] activeCategories = { false, false, false, false, false, false, false, false };

    public RectTransform ratePlaceForm;
    public CanvasGroup placesList, ratingForm;
    public Text ratedPlaceName, ratedPlaceTypes, ratedPlaceOpen, ratedPlaceRating, ratedPlaceWebsite, ratedPlaceNumber;
    Vector2 ratePlaceFormDest;
    Vector2 ratePlaceFormStartPos;
    float placesListAlphaDest, ratingFormAlphaDest;

    public UiController uiController;

    CloudSync cs;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttonsPositions[i] = buttons[i].GetComponent<RectTransform>().anchoredPosition;
            buttonsAlphas[i] = buttons[i].GetComponent<Image>().color.a;
        }
        ratePlaceFormDest = ratePlaceForm.anchoredPosition;
        ratePlaceFormStartPos = ratePlaceFormDest;

        cs = Extensions.CloudSync();
    }

    void addRateCategory(int categoryID)
    {
        for (int i = 0; i < buttonsAlphas.Length; i++)
        {
            if (buttonsAlphas[i] == 0)
            {
                buttons[i].color = poiColors[categoryID];
                buttonsAlphas[i] = 1;
                break;
            }
        }
    }

    void removeRateCategory(int categoryID)
    {
        for (int i = 0; i < buttonsAlphas.Length; i++)
        {
            if (buttons[i].color == poiColors[categoryID])
            {
                buttonsAlphas[i] = 0;
                Image tmpButton = buttons[i];
                float tmpAlpha = buttonsAlphas[i];
                for (int j = i; j < buttonsIDs.Length - 1; j++)
                {
                    buttons[j] = buttons[j + 1];
                    buttonsAlphas[j] = buttonsAlphas[j + 1];
                }
                buttons[buttons.Length - 1] = tmpButton;
                buttonsAlphas[buttonsAlphas.Length - 1] = tmpAlpha;
                break;
            }
        }
    }

    public void rateButtonClick()
    {
        int catID = Array.IndexOf(poiColors, EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color);
        rateFormShow(catID);
    }

    int currentRatingCat = -1;

    public void placeClick(string placeName)
    {
        if (placeName.Length == 0)
            placeName = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;

		PoiPoint poi = ratedPOIS [currentRatingCat].Find (x => x.Name == placeName);

		ratedPlaceName.text = poi.Name.Replace('_', ' ');
		ratedPlaceTypes.text = poi.Kind.Replace('_', ' ');
		ratedPlaceRating.text = "-";
		ratedPlaceOpen.text = "-";
		ratedPlaceOpen.color = Color.white;
		ratedPlaceNumber.text = "-";
		ratedPlaceWebsite.text = "-";

        placesList.blocksRaycasts = false;
        placesListAlphaDest = 0;
        ratingForm.blocksRaycasts = true;
        ratingFormAlphaDest = 1;

		if(poi.Id.Split(':')[0] == "osm")
			StartCoroutine (GetPOIOSMDataCoroutine ("https://api.openstreetmap.org/api/0.6/node/" + poi.Id.Split(':')[1]));

		if(poi.Id.Split(':')[0] == "google")
			StartCoroutine (GetPOIGoogleDataCoroutine ("https://maps.googleapis.com/maps/api/place/details/json?placeid=" + poi.Id.Split(':')[1] + "&key=AIzaSyCzw1lpILgiUWEpUjCyXQDYi_-kJDdyJew"));
    }

	IEnumerator GetPOIOSMDataCoroutine(string url)
	{
		UnityWebRequest www = UnityWebRequest.Get(System.Uri.EscapeUriString (url));
		yield return www.Send();

		Debug.Log ("[POI OSM XML]\n" + www.downloadHandler.text);

		if (www.downloadHandler.text.Length > 0) 
		{

			using (XmlReader reader = XmlReader.Create (new StringReader (www.downloadHandler.text))) {
				while (reader.ReadToFollowing ("tag")) {
					string key, value;
					reader.MoveToFirstAttribute ();
					key = reader.Value;
					reader.MoveToNextAttribute ();
					value = reader.Value;

					switch (key) 
					{
					case "cuisine":
						ratedPlaceTypes.text = ratedPlaceTypes.text + ", " + value.Replace('_', ' ');
						break;
					case "":
						
						break;
					}
				}
			}
		}
	}

	IEnumerator GetPOIGoogleDataCoroutine(string url)
	{
		UnityWebRequest www = UnityWebRequest.Get(System.Uri.EscapeUriString (url));
		yield return www.Send();

		Debug.Log ("[POI GOOGLE JSON]\n" + www.downloadHandler.text);

		if (www.downloadHandler.text.Length > 0) 
		{
			JSONObject placeJson = new JSONObject (www.downloadHandler.text)["result"];

			if (placeJson.HasField ("opening_hours") && placeJson ["opening_hours"].HasField ("open_now")) 
			{
				if (placeJson ["opening_hours"] ["open_now"].b) 
				{
					ratedPlaceOpen.text = "Open now!";
					ratedPlaceOpen.color = Color.green;
				} 
				else 
				{
					ratedPlaceOpen.text = "Closed.";
					ratedPlaceOpen.color = Color.red;
				}
			} 
				
			if (placeJson.HasField ("rating"))
				ratedPlaceRating.text = placeJson ["rating"].f.ToString();

			if (placeJson.HasField ("international_phone_number"))
				ratedPlaceNumber.text = placeJson ["international_phone_number"].str;
			else if (placeJson.HasField ("formatted_phone_number"))
				ratedPlaceNumber.text = placeJson ["formatted_phone_number"].str;

			if (placeJson.HasField ("website"))
				ratedPlaceWebsite.text = placeJson ["website"].str;
		}
	}

    public void rateFormShow(int catID)
    {
        if (ratedPOIS[catID].Count == 0)
            return;
        uiController.isUIActive = true;
        ratePlaceForm.gameObject.SetActive(true);

        currentRatingCat = catID;

        if (ratedPOIS[catID].Count == 1)
        {
            placeClick(ratedPOIS[catID][0].Name);
        }
        else
        {
            List<string> poiNames = new List<string>();
            for (int i = 0; i < ratedPOIS[catID].Count; i++)
            {
                poiNames.Add(ratedPOIS[catID][i].Name);
            }
            ratePlaceForm.GetComponentInChildren<ListController>().clearList();
            ratePlaceForm.GetComponentInChildren<ListController>().addElementsFromList(poiNames);
            ratingForm.blocksRaycasts = false;
            ratingFormAlphaDest = 0;
            placesList.blocksRaycasts = true;
            placesListAlphaDest = 1;
        }
        ratePlaceFormDest = Vector2.zero;
    }

    public void rateFormHide()
    {
        ratePlaceFormDest = ratePlaceFormStartPos;
        ratingForm.blocksRaycasts = false;
        ratingFormAlphaDest = 0;
        placesList.blocksRaycasts = false;
        placesListAlphaDest = 0;
        uiController.isUIActive = false;
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < ratedPOIS.Length; i++)
        {
            if (ratedPOIS[i].Count > 0)
            {
                if (!activeCategories[i])
                {
                    addRateCategory(i);
                    activeCategories[i] = true;
                }
            }
            else
            {
                if (activeCategories[i])
                {
                    removeRateCategory(i);
                    activeCategories[i] = false;
                }
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].CrossFadeAlpha(buttonsAlphas[i], Time.deltaTime * 5, false);
            buttons[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(buttons[i].GetComponent<RectTransform>().anchoredPosition, buttonsPositions[i], Time.deltaTime * 5);
        }

        ratePlaceForm.anchoredPosition = Vector2.Lerp(ratePlaceForm.anchoredPosition, ratePlaceFormDest, Time.deltaTime * 12);
        if (Vector2.Distance(ratePlaceForm.anchoredPosition, ratePlaceFormStartPos) < 0.1f)
            ratePlaceForm.gameObject.SetActive(false);

        placesList.alpha = Mathf.Lerp(placesList.alpha, placesListAlphaDest, Time.deltaTime * 7);
        ratingForm.alpha = Mathf.Lerp(ratingForm.alpha, ratingFormAlphaDest, Time.deltaTime * 7);

    }
}
