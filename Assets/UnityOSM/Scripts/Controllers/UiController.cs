using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Helpers;
using UnityEngine.SceneManagement;

public class UiController : MonoBehaviour
{

    public RectTransform statsScreen, filtersScreen, addPlaceTutorial;

    public CanvasGroup helpTutorials, menuTutorial;

    public float xDragLength, yDragLength, swipeThreshold, holdToAddThreshold;

    public Camera mainCamera;

    float menuTutorialAlphaDest, helpTutorialsAlphaDest, statsScreenXDest, filtersScreenXDest, addPlaceTutorialXDest, ratePlaceTutorialXDest;
    float statsScreenXStart, filtersScreenXStart, addPlaceTutorialXStart, ratePlaceTutorialXStart;

    Vector2 swipeStart;

    float swipeStartTime;

    float xDrag = 0, yDrag = 0;

    Vector2 addPlacePosition;

    public World world;

    public bool isUIActive = true;
    public bool[] poiFilters;

    public Toggle[] filtersToggles;

    public Text[] statsTexts;

    CloudSync cs;

    public void Initialize()
    {
        cs = Extensions.CloudSync();
        poiFilters = cs.Filters;

        statsScreen.sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
        filtersScreen.sizeDelta = this.GetComponent<RectTransform>().sizeDelta;

		statsScreen.anchoredPosition = new Vector2(-statsScreen.sizeDelta.x, 0);
        filtersScreen.anchoredPosition = new Vector2(filtersScreen.sizeDelta.x, 0);

        helpTutorialsAlphaDest = helpTutorials.alpha;

		statsScreenXDest = statsScreen.anchoredPosition.x;
        filtersScreenXDest = filtersScreen.anchoredPosition.x;
        addPlaceTutorialXDest = addPlaceTutorial.anchoredPosition.x;

		statsScreenXStart = statsScreenXDest;
        filtersScreenXStart = filtersScreenXDest;
        addPlaceTutorialXStart = addPlaceTutorialXDest;
        ratePlaceTutorialXStart = ratePlaceTutorialXDest;
    }

    public void helpTutorialsShow()
    {
        addPlaceTutorial.anchoredPosition = new Vector2(addPlaceTutorialXStart, addPlaceTutorial.anchoredPosition.y);
        addPlaceTutorialXDest = addPlaceTutorialXStart;
        ratePlaceTutorialXDest = ratePlaceTutorialXStart;
        helpTutorials.gameObject.SetActive(true);
        isUIActive = true;
        helpTutorialsAlphaDest = 1;
        helpTutorials.blocksRaycasts = true;
    }

    public void helpTutorialsSwitch()
    {
        ratePlaceTutorialXDest = 0;
        addPlaceTutorialXDest = -ratePlaceTutorialXStart;
    }

    public void helpTutorialsHide()
    {
        isUIActive = false;
        helpTutorialsAlphaDest = 0;
        helpTutorials.blocksRaycasts = false;
    }

    public void menuTutorialShow()
    {
        menuTutorial.gameObject.SetActive(true);
        isUIActive = true;
        menuTutorial.blocksRaycasts = true;
        menuTutorialAlphaDest = 1;
        menuTutorial.alpha = 1;
    }

    public void menuTutorialHide()
    {
        isUIActive = false;
        menuTutorialAlphaDest = 0;
        menuTutorial.blocksRaycasts = false;
    }

    public void filtersChange(int index)
    {
        poiFilters[index] = !poiFilters[index];
        PlayerPrefs.SetString("Filters", Extensions.BoolArrayToString(poiFilters));
    }

    void filtersSet()
    {
        for (int i = 0; i < poiFilters.Length; i++)
        {
            if (filtersToggles[i].isOn != poiFilters[i])
            {
                filtersToggles[i].isOn = poiFilters[i];
                filtersChange(i);
            }
        }
    }

    void statsSet()
    {
        if (cs.LatestDiscoveryCategory.Length > 0)
            statsTexts[0].text = cs.LatestDiscoveryCategory;
        else
            statsTexts[0].text = "-";

        statsTexts[1].text = cs.NumberOfDiscoveredPlaces.ToString();

        statsTexts[2].text = cs.TraveledDistance.ToString() + " m";
    }

    public void Logout()
    {
        Destroy(cs.gameObject);
        PlayerPrefs.DeleteKey("UserUID");
        PlayerPrefs.DeleteKey("Username");
        SceneManager.LoadScene("Scenes/firstscene");
    }

    void Update()
    {
        if (!isUIActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                swipeStart = Input.mousePosition;
                swipeStartTime = Time.timeSinceLevelLoad;
            }
            if (Input.GetMouseButton(0))
            {
                xDrag = (swipeStart.x - Input.mousePosition.x) / Screen.width;
                yDrag = (swipeStart.y - Input.mousePosition.y) / Screen.height;
                if (Mathf.Abs(xDrag) > Mathf.Abs(yDrag))
                {
                    if (xDrag < 0)
                    {
						statsScreen.gameObject.SetActive(true);
						Vector2 tmp = statsScreen.anchoredPosition;
						tmp.x = statsScreenXDest * (1 + xDrag);
						statsScreen.anchoredPosition = tmp;
                    }
                    else if (xDrag > 0)
                    {
                        filtersScreen.gameObject.SetActive(true);
                        Vector2 tmp = filtersScreen.anchoredPosition;
                        tmp.x = filtersScreenXDest * (1 - xDrag);
                        filtersScreen.anchoredPosition = tmp;
                    }
                }

				/* Add form showing on long press */
                //if (Time.timeSinceLevelLoad - swipeStartTime > holdToAddThreshold && Mathf.Abs(xDrag) < 0.05f && Mathf.Abs(yDrag) < 0.05f)
                //{
                //    addForm.gameObject.SetActive(true);
                //    addFormShow(mainCamera.ScreenToWorldPoint(Input.mousePosition));
                //}

            }
            if (Input.GetMouseButtonUp(0))
            {
                if (Time.timeSinceLevelLoad - swipeStartTime < swipeThreshold)
                {
                    if (Mathf.Abs(xDrag) > Mathf.Abs(yDrag))
                    {
                        if (xDrag < -xDragLength)
                        {
							statsScreenXDest = 0;
							statsScreen.gameObject.SetActive(true);
							statsSet ();
                            isUIActive = true;
                        }
                        if (xDrag > xDragLength)
                        {
                            filtersScreenXDest = 0;
                            filtersSet();
                            isUIActive = true;
                        }
                    }
                    xDrag = 0;
                    yDrag = 0;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                swipeStart = Input.mousePosition;
                swipeStartTime = Time.timeSinceLevelLoad;
            }
            if (Input.GetMouseButton(0))
            {
                xDrag = (swipeStart.x - Input.mousePosition.x) / Screen.width;
                yDrag = (swipeStart.y - Input.mousePosition.y) / Screen.height;
				if (statsScreenXDest == 0 && xDrag > 0)
                {
					Vector2 tmp = statsScreen.anchoredPosition;
					tmp.x = statsScreenXStart * Mathf.Abs(xDrag) / 5;
					statsScreen.anchoredPosition = tmp;
                }
                if (filtersScreenXDest == 0 && xDrag < 0)
                {
                    Vector2 tmp = filtersScreen.anchoredPosition;
                    tmp.x = filtersScreenXStart * Mathf.Abs(xDrag) / 5;
                    filtersScreen.anchoredPosition = tmp;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (Time.timeSinceLevelLoad - swipeStartTime < swipeThreshold)
                {
					if (xDrag > xDragLength && statsScreenXDest == 0 && Mathf.Abs(yDrag) < 0.1f)
                    {
						statsScreenXDest = statsScreenXStart;
                        isUIActive = false;
                    }
                    if (xDrag < -xDragLength && filtersScreenXDest == 0 && Mathf.Abs(yDrag) < 0.1f)
                    {
                        filtersScreenXDest = filtersScreenXStart;
                        isUIActive = false;
                    }
                    xDrag = 0;
                    yDrag = 0;
                }
            }
        }

        if (!Input.GetMouseButton(0))
        {
			statsScreen.anchoredPosition = Vector2.Lerp(statsScreen.anchoredPosition, new Vector2(statsScreenXDest, statsScreen.anchoredPosition.y), Time.deltaTime * 15);
            filtersScreen.anchoredPosition = Vector2.Lerp(filtersScreen.anchoredPosition, new Vector2(filtersScreenXDest, filtersScreen.anchoredPosition.y), Time.deltaTime * 15);

			if (Mathf.Abs(statsScreen.anchoredPosition.x - statsScreenXStart) < 1)
				statsScreen.gameObject.SetActive(false);
            if (Mathf.Abs(filtersScreen.anchoredPosition.x - filtersScreenXStart) < 1)
                filtersScreen.gameObject.SetActive(false);
        }

        helpTutorials.alpha = Mathf.Lerp(helpTutorials.alpha, helpTutorialsAlphaDest, Time.deltaTime * 5);

        menuTutorial.alpha = Mathf.Lerp(menuTutorial.alpha, menuTutorialAlphaDest, Time.deltaTime * 5);

        if (helpTutorials.alpha < 0.05f)
            helpTutorials.gameObject.SetActive(false);

        if (menuTutorial.alpha < 0.05f)
            menuTutorial.gameObject.SetActive(false);

        addPlaceTutorial.anchoredPosition = Vector2.Lerp(addPlaceTutorial.anchoredPosition, new Vector2(addPlaceTutorialXDest, addPlaceTutorial.anchoredPosition.y), Time.deltaTime * 4);
    }
}
