using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Helpers;
using CI.WSANative.Dialogs;

public class ListController : MonoBehaviour
{

    public Canvas mainCanvas;
    public RectTransform content;
    public RectTransform itemTemplate;

    public List<RectTransform> elements = new List<RectTransform>();
    public List<string> elementsNames = new List<string>();

    public float sensivity, speed, flexibility;

    public InputField newElementName;

    RectTransform rt;
    float minX, maxX, minY, maxY;
    float contentMaxY = 0, contentMinY = 0, contentDest = 0;

    float swipeStartY, swipeStartContentY, swipeStartTime;
    bool swipeStarted = false, interactive = false;

    int elementsCount = 0;

    CloudSync cs;

    public void addElement(string text)
    {

        rt = this.GetComponent<RectTransform>();

        RectTransform element = null;
        element = (RectTransform)Instantiate(itemTemplate, Vector3.zero, this.transform.rotation);
        element.gameObject.SetActive(true);
        float elementHeight = element.sizeDelta.y;
        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + elementHeight);
        element.SetParent(content);
        element.pivot = new Vector2(element.pivot.x, 0f);
        element.anchorMin = new Vector2(element.anchorMin.x, 0f);
        element.anchorMax = new Vector2(element.anchorMax.x, 0f);
        element.anchoredPosition = new Vector3(content.anchoredPosition.x, elementHeight * elementsCount, 0);
        element.localScale = new Vector3(1, 1, 1);

        element.GetComponentInChildren<Text>().text = text;

        elements.Add(element);

        contentMinY = -content.sizeDelta.y;
        contentDest = contentMinY;
        contentMaxY = contentMinY + content.sizeDelta.y - rt.sizeDelta.y;
        if (contentMaxY < contentMinY)
            contentMaxY = contentMinY;
        else
            interactive = true;

        elementsNames.Add(text);

        elementsCount++;
    }

    public void addElementsFromList(List<string> elementsList)
    {
        for (int i = 0; i < elementsList.Count; i++)
        {
            this.addElement(elementsList[i]);
        }
    }

    public void removeElement()
    {
        if (((Mathf.Abs(Input.mousePosition.y / mainCanvas.scaleFactor - swipeStartY) < 25 && interactive) || !interactive) && (Time.timeSinceLevelLoad - swipeStartTime) > 1)
        {
            RectTransform removedElement = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();
            removeElement(removedElement);
        }
    }

    public void removeElement(RectTransform removing)
    {
        float elementHeight = removing.sizeDelta.y;

        for (int i = elements.Count - 1; i > -1; i--)
        {
            if (elements[i] == removing)
                break;
            elements[i].anchoredPosition = new Vector2(elements[i].anchoredPosition.x, elements[i].anchoredPosition.y - elementHeight);
        }

        elements.Remove(removing);

        elementsNames.Remove(removing.GetComponentInChildren<Text>().text);

        Destroy(removing.gameObject);

        elementsCount--;

        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y - elementHeight);

        contentMinY = -content.sizeDelta.y;
        contentDest = contentMinY;
        contentMaxY = contentMinY + content.sizeDelta.y - rt.sizeDelta.y;
        if (contentMaxY < contentMinY)
        {
            interactive = false;
            contentMaxY = contentMinY;
        }
        else
        {
            interactive = true;
        }
    }

    public void clearList()
    {
        elementsCount = 0;
        elements.Clear();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
        Start();
    }

    // Use this for initialization
    void Start()
    {

        rt = this.GetComponent<RectTransform>();

        Vector2 pos = rt.anchoredPosition * mainCanvas.scaleFactor;

        Vector2 size = rt.sizeDelta * mainCanvas.scaleFactor;

        minX = Screen.width / 2 - pos.x - size.x / 2;
        maxX = Screen.width / 2 + pos.x + size.x / 2;
        minY = Screen.height / 2 - pos.y - size.y / 2;
        maxY = Screen.height / 2 + pos.y + size.y / 2;

        cs = Extensions.CloudSync();
    }

    // Update is called once per frame
    void Update()
    {

        if (interactive)
        {
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > minX && Input.mousePosition.x < maxX && Input.mousePosition.y > minY && Input.mousePosition.y < maxY)
            {
                swipeStartY = Input.mousePosition.y / mainCanvas.scaleFactor;
                swipeStartContentY = content.anchoredPosition.y;
                swipeStarted = true;
                swipeStartTime = Time.timeSinceLevelLoad;

				DelayedDisableButtonsCoroutineVar = StartCoroutine (DelayedDisableButtons (Input.mousePosition));
            }
            if (Input.GetMouseButton(0) && swipeStarted)
            {
                float dist = Input.mousePosition.y / mainCanvas.scaleFactor - swipeStartY;

                content.anchoredPosition = new Vector2(content.anchoredPosition.x, swipeStartContentY + dist);
                contentDest = content.anchoredPosition.y;

                if (dist < 0 && content.anchoredPosition.y < contentMinY)
                {
                    contentDest = contentMinY;
                }

                if (dist > 0 && content.anchoredPosition.y > contentMaxY)
                {
                    contentDest = contentMaxY;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                swipeStarted = false;
                float dist = Input.mousePosition.y / mainCanvas.scaleFactor - swipeStartY;
                float threshold = Time.timeSinceLevelLoad - swipeStartTime;
                int swipePower = (int)(dist / threshold / 10);
                if (threshold > 0.15f)
                    swipePower = 0;

                if (content.anchoredPosition.y < contentMaxY && content.anchoredPosition.y > contentMinY)
                    contentDest += swipePower * sensivity;

				if (DelayedDisableButtonsCoroutineVar != null)
					StopCoroutine (DelayedDisableButtonsCoroutineVar);
				foreach (RectTransform element in elements)
					element.GetComponent<Button> ().interactable = true;

            }
        }
        else if (Input.GetMouseButtonDown(0))
            swipeStartTime = Time.timeSinceLevelLoad;

        if (!Input.GetMouseButton(0))
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, new Vector2(content.anchoredPosition.x, contentDest), Time.deltaTime * speed);
            if (content.anchoredPosition.y <= contentMinY - flexibility)
                contentDest = contentMinY;
            if (content.anchoredPosition.y >= contentMaxY + flexibility)
                contentDest = contentMaxY;
        }

    }

	Coroutine DelayedDisableButtonsCoroutineVar = null;
	IEnumerator DelayedDisableButtons(Vector2 swipeStartPos)
	{
		yield return new WaitUntil (() => (Vector2.Distance(swipeStartPos, Input.mousePosition) > 10));
		foreach (RectTransform element in elements)
			element.GetComponent<Button> ().interactable = false;
		DelayedDisableButtonsCoroutineVar = null;
		
	}
}
