using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Globalization;
using System.IO;
using System;
using UnityEngine.Networking;

public class UploadImages : MonoBehaviour
{

    private static UploadImages _instance;
    public static UploadImages Instance { get { return _instance; } }

    private string baseUrl = "gs://photogrammetry-dfa81.appspot.com/";
    private string userName = "Ananda/";
    private int uploadCount = 0;
    private string cloudFunctionUrl = "https://us-central1-photogrammetry-dfa81.cloudfunctions.net/helloWorld";
    List<string> FilePath = new List<string>();
    List<string> images = new List<string>();
    UnityWebRequest postRequestToProcess;
    StorageReference user_ref;
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance);
        }
        else
        {
            _instance = this;
        }
    }


    void Start()
    {

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storage_ref = storage.GetReferenceFromUrl(baseUrl);
        user_ref = storage_ref.Child(userName);
        Debug.Log(user_ref.ToString());
    }


    private void SetUserName(string name)
    {
        userName = name;
    }

    public void UploadImagesToFirebase(List<string> Paths)
    {
        //Creating a different session everytime the user wants create a scene
      Debug.Log("Module Started");
        
        string sessionReference = CreateUserSession();
        Firebase.Storage.StorageReference session_ref =  user_ref.Child(sessionReference);
        MetadataChange type = new MetadataChange { ContentType = "image/jpeg" };
        int Counter = 0;
        foreach (string ImagePath in Paths)
        {
            string imageName = "Image_" + Counter.ToString();
            Counter++;
            StorageReference folder_ref = session_ref.Child(imageName);
            folder_ref.PutFileAsync(ImagePath, type)
                .ContinueWith((Task<StorageMetadata> task) =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.Log(task.Exception.ToString());
                    }
                    else
                    {
                        StorageMetadata metadata = task.Result;
                        string download_url = metadata.DownloadUrl.ToString();
                        images.Add(download_url);
                        Debug.Log(download_url);
                        uploadCount++;
                        CheckIfComplete();
                    }
                }
                );
        }
    }

    public string CreateUserSession()
    {

        string temp = "SESSION" + GetTimeStamp();
        Debug.Log(temp);
        return temp;
    }


    private string GetTimeStamp()
    {
        string time = DateTime.Now.ToString();
        char[] delimeter = { ':', ' ', '/' };
        string[] subs = time.Split(delimeter);
        string finalString = string.Empty;
        foreach (string s in subs)
            finalString += '_' + s;
        return finalString;
    }

    private void CheckIfComplete()
    {
        if (uploadCount == FilePath.Count)
        {
            WWWForm imagesUrl = new WWWForm();
            updateFields(imagesUrl);
            postRequestToProcess = UnityWebRequest.Post(cloudFunctionUrl,imagesUrl);
            StartCoroutine(SendLinks());
        }
    }

    private void updateFields(WWWForm imagesUrl)
    {
        for (int i = 0; i < images.Count; i++)
        {
            string field = i.ToString();
            string data = images[i].ToString();
            imagesUrl.AddField(field, data);
        }
    }


    IEnumerator SendLinks()
    {
        yield return postRequestToProcess.SendWebRequest();
        if (postRequestToProcess.isNetworkError || postRequestToProcess.isHttpError)
        {
            // Replace this with dialog boxes
            Debug.Log(postRequestToProcess.error);
        }
        else
        {
            // Replace this with dialog boxes
            Debug.Log("Request Sucessfull");
            Debug.Log(postRequestToProcess.downloadHandler.text.ToString());
        }
    }

    public void UnitTests()
    {
        string basePath = "C:\\Users\\anan_\\Downloads\\Images\\";
        for (int i = 1; i <= 7; i++)
            FilePath.Add(basePath + i.ToString() + ".jpg");
        UploadImagesToFirebase(FilePath);
    }


 
}
