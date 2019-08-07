using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Helpers;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CI.WSANative.Dialogs;

public class CloudSync : MonoBehaviour
{

    public bool SyncFriends, SyncPois;
    public float FriendsSyncInterval, PoisSyncInterval;
    float CounterStartTimeFriends = 0, CouterStartTimePOIs = 0;

    /* INITIALIZING Cloudsync OBJECT */
    void Start()
    {
        CounterStartTimeFriends = Time.timeSinceLevelLoad;
        CouterStartTimePOIs = Time.timeSinceLevelLoad;

        if (GameObject.FindObjectsOfType<CloudSync>().Length > 1)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    /*
     *      ###############################################################################
     *#######                               VARIABLES                                     ################################################################################################################
     *      ###############################################################################
     *                          DON'T MODIFICATE THESE VARIABLES !
     */

    /* ######################################## LOGIN, REGISTER ######################################## */
    public Dictionary<string, string>
            RegisteringData = new Dictionary<string, string>(),
            LoginData = new Dictionary<string, string>()
    ;



    public string

                UserUID,

    /* ######################################## STATS ######################################## */
                LatestDiscoveryCategory
    ;



    public int
                NumberOfDiscoveredPlaces,
                TraveledDistance
   ;



    /* ######################################## FILTERS ######################################## */
    public bool[] Filters = {                                       
                                true, 
                                true, 
                                true, 
                                true, 
                                true, 
                                true, 
                                true, 
                                true 
    };



    public List<string>

    /* ######################################## PLACES ######################################## */
                AllCollectedPois = new List<string>(),
                NewCollectedPois = new List<string>()
    ;

    /* ######################################## USER DATA ######################################## */

    public List<Vector2>
                UserPathCoordinates = new List<Vector2>()
    ;
    /*
    *      ###############################################################################
    *#######                               END OF VARIABLES                              ################################################################################################################
    *      ###############################################################################
    */

}
