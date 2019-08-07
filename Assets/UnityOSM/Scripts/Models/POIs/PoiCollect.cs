using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Helpers;

public class PoiCollect : MonoBehaviour
{

    public bool beingCollected = false, beingCancelled = false, isCollected = false;
    Image img;
    public string ID, categoryName;

    public void Initialize(Sprite sprite, Sprite rateSprite, string poiID, string categoryName)
    {
        Destroy(this.GetComponent<PoiPoint>());
        this.name = "collect";
        this.ID = poiID;
        this.categoryName = categoryName;
        img = this.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillAmount = 0;
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);

        GameObject poiRate = (GameObject)Instantiate(this.gameObject, this.transform.position, this.transform.rotation);
        poiRate.isStatic = true;
        poiRate.transform.SetParent(this.transform);
        PoiRate pr = poiRate.AddComponent<PoiRate>();
        pr.Initialize(rateSprite, ID);
    }

    /* FUNCTION COLLECTING POI */
    public void collectPOI(float collectTime)
    {
        if (!beingCancelled && !beingCollected && !isCollected)
        {
            StopAllCoroutines();
            StartCoroutine(poiCollecting(collectTime));
            beingCollected = true;
        }
    }

    /* INSTANT COLLECT*/
    public void instantCollectPOI()
    {
        StopAllCoroutines();
        img.fillAmount = 1;
        isCollected = true;
    }

    /* CANCELLING POI COLLECTING */
    public void cancelCollectPOI()
    {
        if (beingCollected)
        {
            StopAllCoroutines();
            StartCoroutine(poiCancelation());
            beingCollected = false;
            beingCancelled = true;
        }
    }

    /* INSTANT CANCEL */
    public void instantCancelCollectPOI()
    {
        if (!isCollected)
        {
            StopAllCoroutines();
            beingCollected = false;
            beingCancelled = false;
            img.fillAmount = 0f;
        }
    }

    IEnumerator poiCollecting(float collectTime)
    {
        while (true && img.fillAmount < 1)
        {
            img.fillAmount += 0.007f;
            yield return new WaitForSeconds(collectTime * 0.005f);
        }
        img.fillAmount = 1;
        isCollected = true;
        beingCollected = false;
        this.GetComponent<Animation>().Play("collect");

        /*---------------------------------------------------------------------------------------------------------------------------------------------------CLOUD!-------------------*/
        CloudSync cs = Extensions.CloudSync();
        cs.NewCollectedPois.Add(ID);
        cs.LatestDiscoveryCategory = this.categoryName;
        PlayerPrefs.SetString("LatestDiscoveryCategory", cs.LatestDiscoveryCategory);
        cs.NumberOfDiscoveredPlaces++;
        PlayerPrefs.SetInt("NumberOfDiscoveredPlaces", cs.NumberOfDiscoveredPlaces);
        /*--------------------------------------------------------------------------------------------------------------------------------------------------CLOUD!-------------------*/

    }

    IEnumerator poiCancelation()
    {
        while (true && img.fillAmount > 0)
        {
            img.fillAmount -= 0.01f;
            yield return new WaitForSeconds(0.001f);
        }
        img.fillAmount = 0;
        beingCancelled = false;
    }
}
