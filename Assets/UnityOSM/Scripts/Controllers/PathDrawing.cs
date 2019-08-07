using UnityEngine;
using System.Collections;
using Assets.Helpers;
using System.Collections.Generic;

public class PathDrawing : MonoBehaviour
{

    public UserMovement user;

    public float pathWidth = 3.5f;
    public Sprite vSprite;
    public Color pathColor;
    public Material pathMaterial;

    LineRenderer currentLR;

	List<LineRenderer> lineRenderersBuffer = new List<LineRenderer>();

    public void RedrawPath()
    {
        CreateVertex(user.pathPoints[user.pathPoints.Count - 1]);
    }

	public void ClearPath()
	{
		foreach (LineRenderer lr in lineRenderersBuffer) 
		{
			GameObject.Destroy (lr.gameObject);
		}
		lineRenderersBuffer.Clear ();
	}

    void Update()
    {
        if (currentLR)
            currentLR.SetPosition(1, new Vector3(user.transform.position.x, user.transform.position.y, -5));
    }

    private GameObject CreateVertex(Vector2 pos)
    {

		if (lineRenderersBuffer.Count > 80)
		{
			StartCoroutine (PathFadeOut (lineRenderersBuffer[0]));
			lineRenderersBuffer.RemoveAt (0);
		}

        Vector3 pos3 = new Vector3(pos.x, pos.y, -5);
        GameObject vertex = new GameObject("vertex");
        vertex.transform.parent = this.transform;
        SpriteRenderer sr = vertex.AddComponent<SpriteRenderer>();
        sr.sprite = vSprite;
        sr.color = pathColor;
        vertex.transform.localScale = new Vector2(pathWidth, pathWidth);
        vertex.transform.position = pos3;
        currentLR = vertex.AddComponent<LineRenderer>();
        currentLR.SetPosition(0, pos3);
        currentLR.SetPosition(1, pos3);
        currentLR.SetWidth(pathWidth, pathWidth);
        currentLR.material = pathMaterial;
        currentLR.SetColors(pathColor, pathColor);
		lineRenderersBuffer.Add (currentLR);

        return vertex;
    }

	private IEnumerator PathFadeOut(LineRenderer lr)
	{
		SpriteRenderer sprite = lr.GetComponent<SpriteRenderer> ();
		while (lr.startColor.a > 0 || lr.endColor.a > 0 || sprite.color.a > 0) 
		{
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - Time.deltaTime); 
			lr.startColor = new Color (lr.startColor.r, lr.startColor.g, lr.startColor.b, lr.startColor.a - Time.deltaTime);
			lr.endColor = new Color (lr.endColor.r, lr.endColor.g, lr.endColor.b, lr.endColor.a - Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}
		Destroy (lr.gameObject);
	}

}
