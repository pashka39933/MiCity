using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Assets.Helpers;
using Kalman;

public class UserMovement : MonoBehaviour
{

    Vector3 destinationPoint;
    float userZIndex;
    float destinationAngle;
    float rotationSpeed = 15f;

    public bool drawPath;
    public float pathWidth = 3.5f;
    public Transform userPath;
    public Sprite vSprite;
    public Color pathColor;

    public LocationService location;
    LineRenderer helpLR;

    /* OPTIMIZED PATH POINTS */
    public List<Vector2> pathPoints = new List<Vector2>();
    public PathDrawing pd;

    CloudSync cs;
	IKalmanWrapper kalman;
	float kalmanTurnOnTiemout = 30;
	float kalmanTimer = 0;

	World worldObject;
	Vector2 latestGooglePoisDownloadPosition;

    // Use this for initialization
    void Start()
    {
        // Set FPS
        Application.targetFrameRate = 60;

		//Kalman filter
		kalman = new MatrixKalmanWrapper();

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        destinationPoint = this.transform.position;
        destinationAngle = this.transform.localEulerAngles.z;
        userZIndex = destinationPoint.z;

        cs = Extensions.CloudSync();

		worldObject = GameObject.FindObjectOfType<World> ();

		latestGooglePoisDownloadPosition = destinationPoint;

		kalmanTimer = Time.time;

    }

    bool firstGPSRead = true;

    // Update is called once per frame
	float DoubleClickTime = 0;
    void Update()
    {


        /* MOUSE NAV IN EDITOR */

#if UNITY_EDITOR
		if(Input.GetMouseButtonUp(0) && Time.time - DoubleClickTime >= 0.2f)
		{
			DoubleClickTime = Time.time;
		}
		else if (Input.GetMouseButtonUp(0) && Time.time - DoubleClickTime < 0.2f)
        {

			destinationPoint = this.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
			destinationPoint.z = userZIndex;

			if(Vector2.Distance(this.transform.position, destinationPoint) > 90)
			{
				this.transform.position = destinationPoint;
				pd.ClearPath();
			}
			else
			{
				pathPoints.Add(this.transform.position);
				pd.RedrawPath();
			}

            /*--------------------------------------------------------------CLOUD!!!!*/
            cs.TraveledDistance += (int)(Vector2.Distance(this.transform.position, destinationPoint) / 2.7f);
            PlayerPrefs.SetInt("TraveledDistance", cs.TraveledDistance);
            /*--------------------------------------------------------------CLOUD!!!!*/

            Vector3 rot = destinationPoint - this.transform.position;
            destinationAngle = (rot.x < 0 ? Vector3.Angle(rot, Vector3.up) : 360 - Vector3.Angle(rot, Vector3.up));
            location.unitySpeed = 40;
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(this.transform.localEulerAngles.z, destinationAngle));
            rotationSpeed = (angleDiff / 360) * 400;
        }

#else

        /* MOVEMENT WITH GPS */
        if (location.readPosition())
        {
            /* IF USER MOVED */
            destinationPoint = location.currentPos;
            destinationPoint.z = userZIndex;

			/* Kalman filtering */
			if(Time.time - kalmanTimer > kalmanTurnOnTiemout)
			{
				destinationPoint = kalman.Update(destinationPoint);
			}
			else
			{
				kalman.Update(destinationPoint);
			}

            /*--------------------------------------------------------------CLOUD!!!!*/
            cs.TraveledDistance += (int)(Vector2.Distance(this.transform.position, destinationPoint) / 2.75f);
            PlayerPrefs.SetInt("TraveledDistance", cs.TraveledDistance);
            /*--------------------------------------------------------------CLOUD!!!!*/

			if (firstGPSRead)
            {
                firstGPSRead = false;
                this.transform.position = destinationPoint;
            }
            else
            {
                Vector3 rot = destinationPoint - this.transform.position;
                destinationAngle = (rot.x < 0 ? Vector3.Angle(rot, Vector3.up) : 360 - Vector3.Angle(rot, Vector3.up));
                rotationSpeed = (Mathf.Abs(Mathf.DeltaAngle(this.transform.localEulerAngles.z, destinationAngle)) / 360) * 144;
            }

			if(Vector2.Distance(this.transform.position, destinationPoint) > 90)
			{
				this.transform.position = destinationPoint;
				pd.ClearPath();
			}
			else
			{
				pathPoints.Add(this.transform.position);
				pd.RedrawPath();
			}
        }

#endif

        this.transform.position = Vector3.MoveTowards(this.transform.position, destinationPoint, (float)(location.unitySpeed) * 1.44f * Time.deltaTime);

        this.transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(this.transform.localEulerAngles.z, destinationAngle, Time.deltaTime * rotationSpeed));
   
		if (Vector2.Distance (destinationPoint, latestGooglePoisDownloadPosition) > 125) 
		{
			DownloadGooglePOIs (destinationPoint);
		}
	}

	/* Metoda pobrania nowych poi od google */
	private void DownloadGooglePOIs(Vector2 userPosition)
	{

		latestGooglePoisDownloadPosition = userPosition;

		/* CALCULATING COORDS */
		Rect center = GM.TileBounds(worldObject.Center, worldObject.Zoom);
		Vector2 clickTilePos = new Vector2((int)(userPosition.x / center.width), (int)(userPosition.x / center.height)) + worldObject.Center;

		Vector2 tilePosMER = GM.TileBounds(clickTilePos, worldObject.Zoom).center;
		Vector2 tilePosUNITY = new Vector2(tilePosMER.x - center.x, tilePosMER.y - center.y);

		userPosition = userPosition - tilePosUNITY;
		userPosition = new Vector2(userPosition.x + tilePosMER.x, userPosition.y + tilePosMER.y);
		userPosition = GM.MetersToLatLon(userPosition);
		worldObject.LoadGooglePOIs (userPosition);
	}
}
