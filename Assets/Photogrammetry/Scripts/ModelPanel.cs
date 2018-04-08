using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelPanel : MonoBehaviour
{
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
        rectTransform = GetComponent<RectTransform>();
        modelImgRectTransform = modelImg.GetComponent<RectTransform>();
    }

    public void populatePanel(ModelData model, Texture2D modelIcon)
    {
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
            modelImg.texture = www.texture; //Get texture
            
            //modelImg.GetComponent<AspectRatioFitter>().aspectRatio = www.texture.width / www.texture.height;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
