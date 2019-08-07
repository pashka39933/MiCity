using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Helpers;

public class PoiRate : MonoBehaviour
{

    Image img;
    public string ID;

    public void Initialize(Sprite sprite, string poiId)
    {
        Destroy(this.GetComponent<PoiCollect>());
        this.name = "rate";
        this.ID = poiId;
        img = this.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillAmount = 0;
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
    }

    public void enableRating()
    {
        StopAllCoroutines();
        StartCoroutine(enableRatingCoroutine());
    }

    public void disableRating()
    {
        StopAllCoroutines();
        StartCoroutine(disableRatingCoroutine());
    }

    IEnumerator enableRatingCoroutine()
    {
        while (img.fillAmount < 1)
        {
            img.fillAmount += 0.05f;
            yield return new WaitForSeconds(0.005f);
        }
    }

    IEnumerator disableRatingCoroutine()
    {
        while (img.fillAmount > 0)
        {
            img.fillAmount -= 0.05f;
            yield return new WaitForSeconds(0.005f);
        }
    }
}
