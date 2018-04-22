using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ModelImporter : MonoBehaviour {
    public GameObject modelBrowserWindow;
    public Transform groundPlane;
    PlaneFinderBehaviour planeFinder;
    ContentPositioningBehaviour contentPositioning;
    GameObject selectedModel, importedModel;
    public Shader modelShader;
    public GameObject test;
    bool modelSelected = false;
    bool modelPlaced = false;
    Vector3 offset;
    PivotAdjuster pivotAdjuster;
    public float targetScale = 2f;

    //temp
    public UnityEngine.UI.Text debugText;

    //DEMO
    public GameObject tobasco;
    public GameObject houses;

    // Use this for initialization
    void Start () {
        pivotAdjuster = FindObjectOfType<PivotAdjuster>();
        planeFinder = FindObjectOfType<PlaneFinderBehaviour>();
        contentPositioning = planeFinder.GetComponent<ContentPositioningBehaviour>();
        //debugText.text = Application.persistentDataPath;
        //selectedModel = test;
        ////modelSelected = true;
        //selectedModel = tobasco;
        //modelSelected = true;
    }

    //Import the obj model
    public IEnumerator importModel(string modelPath)
    {
        modelPlaced = false;
        modelBrowserWindow.SetActive(false);
        selectedModel = OBJLoader.LoadOBJFile(modelPath, modelShader, out offset);
        if (selectedModel.transform.childCount > 0)
        {
            //selectedModel.GetComponentsInChildren<Transform>()[1].Translate(-offset); //Looks in the parent as well
            pivotAdjuster.adjustPivot(selectedModel.GetComponentsInChildren<Transform>()[1]);
            selectedModel.GetComponentsInChildren<Transform>()[1].localPosition = Vector3.zero;
        }
        else
        {
            //selectedModel.transform.Translate(-offset);
            pivotAdjuster.adjustPivot(selectedModel.transform);
            selectedModel.transform.position = Vector3.zero;
        }

        //Calculating mesh size
        Renderer rend = selectedModel.GetComponentsInChildren<Renderer>()[0];
        float diameter = rend.bounds.extents.magnitude * 2;
        float scale = targetScale / diameter; //Calculates model scale to make it's diameter match the target scale (makes all model the same size)
        selectedModel.transform.localScale = new Vector3(scale, scale, scale); //x is negative because obj's flipped on x-axis apparently

        modelSelected = true;
        yield return null;
    }

    public void demoImportModel(string modelName)
    {
        modelPlaced = false;
        modelBrowserWindow.SetActive(false);
        if (modelName == "Tobasco")
        {
            tobasco.SetActive(true);
            selectedModel = tobasco;
            StartCoroutine(modelSelectedDelay());
        }
        else if (modelName == "Houses")
        {
            houses.SetActive(true);
            selectedModel = houses;
            StartCoroutine(modelSelectedDelay());
        }
    }

    //Places object more accurately (time to think)
    IEnumerator modelSelectedDelay()
    {
        yield return new WaitForSeconds(0.5f);
        modelSelected = true;
    }

    //Place the selected model on the ground plane
    public void placeOnGroundPlane(HitTestResult result)
    {
        if (!modelPlaced)
        {
            debugText.text = "modelSelected: " + modelSelected + " modelBrowserWindow: " + modelBrowserWindow.activeSelf;
        }
        else
        {
            debugText.text = "Model Placed!" + " modelSelected: " + modelSelected + " modelBrowserWindow: " + modelBrowserWindow.activeSelf;
        }

        if (!modelPlaced && modelSelected && !modelBrowserWindow.activeSelf)
        {
            debugText.text = selectedModel.name + ": " + selectedModel.transform.position;
            selectedModel.SetActive(true);
            selectedModel.transform.SetParent(groundPlane); //Make ground plane parent of model
            contentPositioning.PositionContentAtPlaneAnchor(result); //Place model
            modelPlaced = true;
            modelSelected = false;
            selectedModel.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }
	
	// Update is called once per frame
	void Update () {

	}
}
