using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

//Disables default vuforia AR camera
public class DisableVuforia : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Camera mainCamera = Camera.main;
        if (mainCamera)
        {
            if (mainCamera.GetComponent<VuforiaBehaviour>() != null)
            {
                mainCamera.GetComponent<VuforiaBehaviour>().enabled = false;
            }
            if (mainCamera.GetComponent<VideoBackgroundBehaviour>() != null)
            {
                mainCamera.GetComponent<VideoBackgroundBehaviour>().enabled = false;
            }
            if (mainCamera.GetComponent<DefaultInitializationErrorHandler>() != null)
            {
                mainCamera.GetComponent<DefaultInitializationErrorHandler>().enabled = false;
            }
        }
    }
}
