using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ModelBrowser : MonoBehaviour {
    string localJsonFileName = "/localModels.json";
    string cloudJsonFileName = "/cloudModels.json";

    AllModels localModels; //All local model objects
    List<ModelPanel> localModelPanels;
    AllModels cloudModels; //All cloud model objects
    List<ModelPanel> cloudModelPanels;
    bool modelPanelsLoaded = false;

    public GameObject localModelBrowser;
    public RectTransform localScrollArea; //The scroll area for the local panels
    public GameObject cloudModelBrowser;
    public RectTransform cloudScrollArea; //The scroll area for the cloud panels
    public GameObject modelPanelPrefab; //Model panel to be instantiated for each model

    //Model panel icons
    public Texture2D cloudIcon;
    public Texture2D localIcon;

    //Browser tabs
    public RawImage leftTab;
    public RawImage rightTab;
    public Texture2D lightLeft;
    public Texture2D darkLeft;
    public Texture2D lightRight;
    public Texture2D darkRight;
    int activeTab = 1; //0 = left; 1 = right

    // Use this for initialization
    void Start () {
        localModelPanels = new List<ModelPanel>();
        cloudModelPanels = new List<ModelPanel>();
        readLocalJSON(); //Load local model data
        readCloudJSON(); //Load cloud model data
        checkLoadingProgress();
    }

    //Gets local model data from local json file
    void readLocalJSON()
    {
        string filePath = Application.streamingAssetsPath + localJsonFileName;
        Debug.Log(filePath);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            localModels = JsonUtility.FromJson<AllModels>(dataAsJson);
            populateModelBrowser(localModels, true);
        }
        else
        {
            Debug.LogError("Cannot load model data!");
        }
    }

    //Gets cloud model data from cloud json file
    void readCloudJSON()
    {
        string filePath = Application.streamingAssetsPath + cloudJsonFileName;
        Debug.Log(filePath);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            cloudModels = JsonUtility.FromJson<AllModels>(dataAsJson);
            populateModelBrowser(cloudModels, false);
        }
        else
        {
            Debug.LogError("Cannot load model data!");
        }
    }

    void populateModelBrowser(AllModels models, bool local)
    {
        for (int i = 0; i < models.Models.Length; i++)
        {
            //Popluate the model panel
            if (local) //Local model
            {
                GameObject modelPanel = Instantiate(modelPanelPrefab, modelPanelPrefab.transform.position, Quaternion.identity, localScrollArea.transform);
                ModelPanel panel = modelPanel.GetComponent<ModelPanel>();
                panel.populatePanel(models.Models[i], localIcon);
                localModelPanels.Add(panel);
            }
            else //Cloud model
            {
                GameObject modelPanel = Instantiate(modelPanelPrefab, modelPanelPrefab.transform.position, Quaternion.identity, cloudScrollArea.transform);
                ModelPanel panel = modelPanel.GetComponent<ModelPanel>();
                panel.populatePanel(models.Models[i], cloudIcon);
                cloudModelPanels.Add(panel);
            }
        }
    }

    //Display models corresponding to active tab (cloud/local)
    void displayActiveModels()
    {
        if (activeTab == 0) //Left tab (cloud models)
        {
            localModelBrowser.SetActive(false);
            cloudModelBrowser.SetActive(true);
        }
        else if (activeTab == 1) //Right tab (local models)
        {
            localModelBrowser.SetActive(true);
            cloudModelBrowser.SetActive(false);
        }
    }

    //Checks if all images in ModelPanels have been loaded before calling displayActiveModels
    void checkLoadingProgress()
    {
        bool loaded = true;
        for (int i = 0; i < localModelPanels.Count; i++)
        {
            if (!localModelPanels[i].doneLoading) //Check if model image in ModelPanel is done loading
            {
                loaded = false;
                break;
            }
        }

        if (loaded)
        {
            for (int i = 0; i < cloudModelPanels.Count; i++)
            {
                if (!cloudModelPanels[i].doneLoading) //Check if model image in ModelPanel is done loading
                {
                    loaded = false;
                    break;
                }
            }
        }

        if (loaded) //ModePanels are loaded
        {
            modelPanelsLoaded = true;
            displayActiveModels();
        }
    }

    //Change texture on left tab when clicked
    public void leftTabClicked()
    {
        if (activeTab != 0) //Switch tabs if not already active
        {
            leftTab.texture = lightLeft;
            rightTab.texture = darkRight;
            activeTab = 0;
            displayActiveModels();
        }
    }

    //Change texture on right tab when clicked
    public void rightTabClicked()
    {
        if (activeTab != 1) //Switch tabs if not already active
        {
            leftTab.texture = darkLeft;
            rightTab.texture = lightRight;
            activeTab = 1;
            displayActiveModels();
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (!modelPanelsLoaded) //ModelPanels are still loading
        {
            checkLoadingProgress();
        }
	}
}
