using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ModelBrowser : MonoBehaviour {
    string localJsonFileName = "/localModels.json";
    string localJsonFilePath;
    string cloudJsonFileName = "/cloudModels.json";
    string cloudJsonFilePath;

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
        localJsonFilePath = Application.persistentDataPath + localJsonFileName;
        cloudJsonFilePath = Application.persistentDataPath + cloudJsonFileName;
        //localJsonFilePath = Application.streamingAssetsPath + localJsonFileName;
        //cloudJsonFilePath = Application.streamingAssetsPath + cloudJsonFileName;
        localModelPanels = new List<ModelPanel>();
        cloudModelPanels = new List<ModelPanel>();
        readLocalJSON(); //Load local model data
        readCloudJSON(); //Load cloud model data
        checkLoadingProgress();
    }

    //Gets local model data from local json file
    void readLocalJSON()
    {
        if (File.Exists(localJsonFilePath))
        {
            string dataAsJson = File.ReadAllText(localJsonFilePath);
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
        if (File.Exists(cloudJsonFilePath))
        {
            string dataAsJson = File.ReadAllText(cloudJsonFilePath);
            cloudModels = JsonUtility.FromJson<AllModels>(dataAsJson);
            populateModelBrowser(cloudModels, false);
        }
        else
        {
            Debug.LogError("Cannot load model data!");
        }
    }

    //Rewrites local json file to include new downloaded cloud model
    public void updateLocalJSON(ModelData modelData, ModelPanel modelPanel)
    {
        localModels.Models.Add(modelData);
        string jsonString = JsonUtility.ToJson(localModels); //Serialize json objects to json string
        File.WriteAllText(localJsonFilePath, jsonString); //Save json file
        modelPanel.icon.texture = localIcon; //Show that model has been downloaded
        for (int i = 0; i < localScrollArea.childCount; i++) //Destroy old model panels
        {
            GameObject panel = localScrollArea.GetChild(i).gameObject;
            panel.SetActive(false); //Disabling it first is necessary, otherwise the layout group doesn't allow object to be deleted
            Destroy(panel);
        }
        rightTabClicked(); //Switch to local browser
        modelPanelsLoaded = false;
        readLocalJSON(); //Update local model browser
    }

    void populateModelBrowser(AllModels models, bool local)
    {
        for (int i = 0; i < models.Models.Count; i++)
        {
            //Popluate the model panel
            if (local) //Local model
            {
                GameObject modelPanel = Instantiate(modelPanelPrefab, modelPanelPrefab.transform.position, Quaternion.identity, localScrollArea.transform);
                ModelPanel panel = modelPanel.GetComponent<ModelPanel>();
                panel.populatePanel(this, models.Models[i], localIcon, true);
                localModelPanels.Add(panel);
            }
            else //Cloud model
            {
                GameObject modelPanel = Instantiate(modelPanelPrefab, modelPanelPrefab.transform.position, Quaternion.identity, cloudScrollArea.transform);
                ModelPanel panel = modelPanel.GetComponent<ModelPanel>();
                panel.populatePanel(this, models.Models[i], cloudIcon, false);
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
            if (modelPanelsLoaded) //Don't switch tabs if model panels are still loading
            {
                displayActiveModels();
            }
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
            if (modelPanelsLoaded) //Don't switch tabs if model panels are still loading
            {
                displayActiveModels();
            }
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
