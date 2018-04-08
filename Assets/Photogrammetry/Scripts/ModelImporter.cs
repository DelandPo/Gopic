using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelImporter : MonoBehaviour {
    public GameObject modelBrowserWindow;

	// Use this for initialization
	void Start () {
		
	}

    public void importModel(string modelPath)
    {
        modelBrowserWindow.SetActive(false);
        OBJLoader.LoadOBJFile(modelPath);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
