using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavBar : MonoBehaviour {
    Scene activeScene;
    int activeSceneIndex;
    int browserSceneIndex = 0; //Index of visualization scene in build settings
    int captureSceneIndex = 1; //Index of photogrammetry scene in build settings
    public GameObject modelBrowserWindow;
    public ImageCapture imageCapture; //Used for deleting image files before scene is switched

    // Use this for initialization
    void Start () {
        activeScene = SceneManager.GetActiveScene();
        activeSceneIndex = activeScene.buildIndex;
	}

    public void browserIconClicked()
    {
        if (activeSceneIndex == browserSceneIndex) //In visualization scene
        {
            if (modelBrowserWindow.activeSelf) //Browser window active (disable it)
            {
                modelBrowserWindow.SetActive(false);
            }
            else //Browser window not active (make it active)
            {
                modelBrowserWindow.SetActive(true);
            }
        }
        else //Load visualization scene
        {
            imageCapture.deleteFiles(); //Deleting saved image files
            SceneManager.LoadSceneAsync(browserSceneIndex);
        }
    }

    public void captureIconClicked()
    {
        if (activeSceneIndex == captureSceneIndex) //In visualization scene
        {
            Debug.Log("Capture Icon clicked");
        }
        else //Load visualization scene
        {
            SceneManager.LoadSceneAsync(captureSceneIndex);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
