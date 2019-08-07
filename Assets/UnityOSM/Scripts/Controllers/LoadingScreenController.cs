using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Helpers;

public class LoadingScreenController : MonoBehaviour
{

    public World worldObject;

    public UiController uiController;

    public LocationService GPS;

    float loadingPercentage = 0, alphaDest = 1;

    CanvasGroup thisCanvasGroup;

    public Image fillImg;

    public GameObject GPSState;

    int whichTileLoad = 0;

    CloudSync cs;

	bool GooglePoisLoadCalled = false;

    // Use this for initialization
    void Start()
    {
        thisCanvasGroup = this.GetComponent<CanvasGroup>();
        uiController.Initialize();

        /* SHOWING MENU TUTORIAL ONLY WHEN FIRST USED */
        if (!PlayerPrefs.HasKey("NotFirstLogin"))
        {
            uiController.menuTutorialShow();
            PlayerPrefs.SetInt("NotFirstLogin", 1);
        }

        cs = Extensions.CloudSync();

    }

    bool initialDataDownloading = false;

    // Update is called once per frame
    void Update()
    {

        if (!this.GetComponent<Animation>().isPlaying && !GPSState.GetComponent<Animation>().isPlaying)
            GPSState.GetComponent<Animation>().Play();

        if (worldObject.worldInitialized)
        {

            GPSState.SetActive(false);

			if (!GooglePoisLoadCalled) 
			{
				GooglePoisLoadCalled = true;
#if UNITY_EDITOR
				worldObject.LoadGooglePOIs (GPS.currentPos);
#else
				worldObject.LoadGooglePOIs (GPS.currentPosLONLAT);
#endif
			}

            /* LOADING TILES */
			if (whichTileLoad < worldObject.tilesRange * worldObject.tilesRange && worldObject.CreateTileCoroutineVar == null)
            {
                worldObject.loadTile(whichTileLoad);
                whichTileLoad++;
            }

            loadingPercentage = 1 - ((float)worldObject.tilesLeft / ((float)worldObject.tilesRange * (float)worldObject.tilesRange));

            fillImg.fillAmount = Mathf.Lerp(fillImg.fillAmount, loadingPercentage, Time.deltaTime * 2);

            thisCanvasGroup.alpha = Mathf.Lerp(thisCanvasGroup.alpha, alphaDest, Time.deltaTime * 5);

			if (fillImg.fillAmount > 0.99f)
            {
                alphaDest = 0;
            }

            if (this.thisCanvasGroup.alpha < 0.05f)
            {
                uiController.enabled = true;
                Destroy(this.gameObject);
            }
        }
    }
}
