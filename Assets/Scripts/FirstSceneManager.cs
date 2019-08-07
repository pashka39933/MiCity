using UnityEngine;
using System.Collections;
using Assets.Helpers;
using UnityEngine.SceneManagement;

public class FirstSceneManager : MonoBehaviour
{

    void Start()
    {
		Application.targetFrameRate = 60;
        SceneManager.LoadScene("UnityOSM/main");
    }
}
