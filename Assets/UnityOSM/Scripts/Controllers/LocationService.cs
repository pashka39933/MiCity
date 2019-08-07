using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Helpers;
using CI.WSANative.Dialogs;

public class LocationService : MonoBehaviour
{
    public Text txt;

    public World world;

    public Vector2 currentPos, currentPosLONLAT;

    public double unitySpeed;

    CloudSync cs;

    IEnumerator Start()
    {

        cs = Extensions.CloudSync();

        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            txt.text = "Location service disabled! Enable location in settings!";

#if UNITY_EDITOR
            world.Initialize(currentPos);
#endif

            // Windows store implementation here
#if NETFX_CORE
            WSANativeDialog.ShowDialog("Wrong settings!", "Enable localization in settings!");
#endif

#if UNITY_ANDROID
            AndroidNativeFunctions.ShowAlert("Please enable localization in settings.", "Alert!", "OK", null, null, null);
#endif

            yield break;
        }

        // Start service before querying location
        Input.location.Start(0.5f, 0.5f);
        txt.text = "Initializing";

        // Wait until service initializes
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            txt.text += ".";
            if (txt.text.Contains("...."))
                txt.text = "Initializing";
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            txt.text = "Positioning failed, check settings and relaunch application!";
            // Windows store implementation here
#if NETFX_CORE
                WSANativeDialog.ShowDialog("GPS error!", "Please check Your settings!");
#endif

#if UNITY_ANDROID
            AndroidNativeFunctions.ShowAlert("Localization failed, please check Your settings.", "Alert!", "OK", null, null, null);
#endif
            yield break;
        }

        // Wait for fix
        while (Input.location.lastData.horizontalAccuracy > 70)
        {
            txt.text = "Waiting for GPS fix..., " + "current GPS accuracy (needed < 70): " + Input.location.lastData.horizontalAccuracy + ", if it takes too long, try restarting MiCity!";
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            world.Initialize(new Vector2(Input.location.lastData.longitude, Input.location.lastData.latitude));
			currentPosLONLAT = new Vector2 (Input.location.lastData.longitude, Input.location.lastData.latitude);
            cs.UserPathCoordinates.Add(new Vector2(Input.location.lastData.longitude, Input.location.lastData.latitude));
            lastTimestamp = System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
            txt.text = "Initialized!";
            yield break;
        }
    }

    //public float fakeLON, fakeLAT;

    double lastTimestamp = 0, lastGPSTime = 0;

    public bool readPosition()
    {
        if (world.worldInitialized && Input.location.status == LocationServiceStatus.Running)
        {

            if (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds - lastTimestamp < 0.1f)
                return false;

            if (Input.location.lastData.timestamp - lastGPSTime < 0.1f)
                return false;

            /* IF NEW POSITION AVAILABLE */
            /* COORDS CONVERSION */
            Vector2 previousPos = currentPos;
			currentPosLONLAT = new Vector2 (Input.location.lastData.longitude, Input.location.lastData.latitude);
            currentPos = GM.LatLonToMeters(Input.location.lastData.latitude, Input.location.lastData.longitude);
            Rect center = GM.TileBounds(world.Center, world.Zoom);
            Vector2 centerRectPos = new Vector2(center.x, center.y);
            Vector2 tileRealPos = new Vector2(world.Center.x, world.Center.y);
            Vector2 tilePosMER = GM.TileBounds(tileRealPos, world.Zoom).center;
            Vector2 tilePosUNITY = new Vector2(tilePosMER.x - centerRectPos.x, tilePosMER.y - centerRectPos.y);
            currentPos = new Vector2(currentPos.x - tilePosMER.x, currentPos.y - tilePosMER.y) + tilePosUNITY;

            unitySpeed = Vector2.Distance(currentPos, previousPos) / (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds - lastTimestamp);

            txt.text = "LAT: " + Input.location.lastData.latitude.ToString() + ", LON: " + Input.location.lastData.longitude.ToString() + ", CONTROL: " + Random.Range(0, 100).ToString() + ", SPEED: " + unitySpeed;

            /* NEW TIMESTAMP */
            lastTimestamp = System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
            lastGPSTime = Input.location.lastData.timestamp;

            /*--------------CLOUD!!!!*/
            if(Vector2.Distance(previousPos, currentPos) > 25)
                cs.UserPathCoordinates.Add(new Vector2(Input.location.lastData.longitude, Input.location.lastData.latitude));

            return true;
        }
        return false;
    }
}