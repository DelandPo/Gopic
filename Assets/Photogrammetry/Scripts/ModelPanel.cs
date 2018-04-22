using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelPanel : MonoBehaviour
{
    ModelBrowser modelBrowser;
    ModelImporter modelImporter;
    ModelData modelData;
    bool local;
    public RawImage icon;
    public Text modelName;
    public Text userId;
    public RawImage modelImg;
    RectTransform modelImgRectTransform;
    RectTransform rectTransform;
    internal bool doneLoading = false;

    // Use this for initialization
    void Start()
    {
        modelImporter = FindObjectOfType<ModelImporter>();
        rectTransform = GetComponent<RectTransform>();
        modelImgRectTransform = modelImg.GetComponent<RectTransform>();
    }

    public void populatePanel(ModelBrowser browser, ModelData model, Texture2D modelIcon, bool isLocal)
    {
        modelBrowser = browser;
        modelData = model;
        local = isLocal;

        //Populate fields
        StartCoroutine(loadImage(model.PicturesUri));
        icon.texture = modelIcon;
        modelName.text = model.Name;
        userId.text = model.UserId;
    }

    IEnumerator loadImage(string url)
    {
        // Start a download of the given URL
        using (WWW www = new WWW(url))
        {
            // Wait for download to complete
            yield return www;

            doneLoading = true;
            modelImg.color = Color.white;
            modelImg.texture = www.texture; //Get texture

            //Debug.Log(modelData.Name + "width: " + www.texture.width + " height: " + www.texture.height);
            //modelImg.GetComponent<AspectRatioFitter>().aspectRatio = www.texture.width / www.texture.height;
            //Debug.Log("Adjusted: " + modelData.Name + " aspect ratio");
        }
    }

    public void modelPanelClicked()
    {
        if (local) //Local model
        {
            importModel();
        }
        else //Cloud model
        {
            //downloadModel();
        }
    }

    //Downloads a cloud model and stores on device
    void downloadModel()
    {
        Debug.Log("download " + modelData.Name);
        local = true;
        modelData.ModelUri = "TestObj/result.obj"; //Change ModelUri to local file path
        modelBrowser.updateLocalJSON(modelData, this);
    }
    
    //Imports local model into scene
    void importModel()
    {
        Debug.Log("import " + modelData.Name);
        StartCoroutine(modelImporter.importModel(modelData.ModelUri));
        //modelImporter.demoImportModel(modelData.Name);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
