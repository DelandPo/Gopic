using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ImageCapture : MonoBehaviour
{
    //Image Capturing
    public List<Texture2D> capturedImages; //Stores captured images
    public List<string> capturedImagePaths; //Stores the paths of captured images
    int currentImg = 0; //Index of viewed image
    bool capture = true; //Indicates if images can be captured
    public Button cameraBtn; //Capture image button
    public Button galleryBtn; //View images button
    public GameObject canvas; //Holds UI components, disabled when a screenshot is taken
    public RawImage imgViewer; //Displays captured images
    public Text imgIndex; //Displays the index of the image being viewing out of the total number of images
    AspectRatioFitter aspectFit;
    public GameObject cam; //AR Camera
    public GameObject startCapturingBtn; //Button that starts image capturing
    public Slider captureProgress; //Progress bar that indicates how far along the capturing process is
    public GameObject doneCapturingIcon; //Icon that appears when all images have been captured
    public GameObject createModelPanel; //Panel that indicates that capturing was successful and allows the user to create the model
    bool creatingModel = false;

    //Temporary
    public RawImage imagePreview;
    public Text debugText;

    //Gyroscope Check
    private bool gyroEnabled; //Indicates if the device gyroscope is enabled
    private Gyroscope gyro; //Device gyroscope
    Vector3 orient; //Orientation of device as a Vector3
    float angleHorizontal = 0; //Angle of device on horizontal axis
    float angleVertical = 0; //Angle of device on vertical axis
    Vector3 acceleration; //Acceleration of device
    public Vector3 movementThreshold; //How fast the device can be moving when an image is captured
    bool deviceMoving = false; //Indicates if the device is moving

    //Capture Info
    Dictionary<float, Dictionary<float, bool>> captureInfo; //Holds information on the images captured
    [Tooltip("Number of images to capture. MUST BE A MULTIPLE OF verticalCapureRegions! Will be increased to a multiple of verticalCaptureRegions otherwise.")]
    public int imagesToCapture = 1; //Number of images to capture
    float horizontalCaptureRegions; //Stores the number of horizontalCaptureRegions
    float horizontalCaptureRegionSize; //Stores the range in degrees of a region that an image can be captured in on the horizontal axis
    [Tooltip("Number of images to capture on the vertical axis in each captureRegion. (Example: a value of 3 would have images be taken at 0 degrees, 45 degrees, and 90 degrees) MUST DIVIDE 90 EVENLY! Will be increased until 90 % verticalCaptureRegions = 0")]
    public float verticalCaptureRegions = 3; //Stores the number of verticalCaptureRegions
    float verticalCaptureRegionSize; //Stores the size of the range in degrees of a region that an image can be captured in on the vertical axis
    float curHorCapReg; //Current horizontal capture region
    float curVertCapReg; //Current vertical capture region
    bool needCapture = false; //Indicates if an image is needed in the device's current capture regions
    bool doneCapturing = false; //Indicates if all the required images have been captured
    bool startCapturing = false; //Indicates if image capturing should start

    // Use this for initialization
    void Start()
    {
        cameraBtn.gameObject.SetActive(false);
        aspectFit = imgViewer.GetComponent<AspectRatioFitter>();
        imgViewer.gameObject.SetActive(false);
        debugText.text = "";
        gyroEnabled = enableGyro();
        intializeCaptureInfo(); //Initialize the dictionary with values based on imagesToCapture
        captureProgress.minValue = 0;
        captureProgress.maxValue = imagesToCapture;
        captureProgress.gameObject.SetActive(false);
        startCapturingBtn.SetActive(true);
        doneCapturingIcon.SetActive(false);
        createModelPanel.SetActive(false);
    }

    //Capture button clicked (Manual image capture)
    public void captureBtnClick()
    {
        StartCoroutine(captureImage());
    }

    //Capture Image (Screenshot) ***Need around 20 images
    IEnumerator captureImage()
    {
        if (capture) //Image capturing available
        {
            //recordCaptureInfo(); //Record device orientation at time of image capture

            //Capture image
            canvas.SetActive(false); //Hiding UI
            yield return new WaitForEndOfFrame(); //Waiting for everything to render

            //Save image to Texture2D
            Texture2D img = ScreenCapture.CaptureScreenshotAsTexture(); //Save screenshot to a Texture2D
            img.Apply(); //Apply changes to make texture visible
            capturedImages.Add(img); //Save to List<Texture2D>

            //Save image to Application.persistentDataPath
            string imgName = "IMG" + capturedImagePaths.Count + ".jpg";
            string filePath = Application.persistentDataPath + "/" + imgName;
            ScreenCapture.CaptureScreenshot(imgName);
            capturedImagePaths.Add(filePath);

            //imagePreview.texture = img; //Set imagePreview to current image
            canvas.SetActive(true); //Restoring UI
        }
        else
        {
            //Display camera feed
            cam.SetActive(true); //Activate camera
            TrackerManager.Instance.GetTracker<ObjectTracker>().Stop(); //Disabling Vuforia object tracker (I think it is reenabled when the camera is set active)
            imgViewer.gameObject.SetActive(false); //Disable image gallery
            cameraBtn.gameObject.SetActive(false);
            //cameraBtn.GetComponentInChildren<Text>().text = "Capture Image"; //Change button text
            galleryBtn.GetComponentInChildren<Text>().text = "View Images"; //Change button text
            capture = true; //Allow image capturing
        }
    }

    //View captured images
    public void viewImages()
    {
        if (capturedImages.Count > 0)
        {
            if (capture) //Camera feed not visible
            {
                currentImg = capturedImages.Count - 1; //Most recent image
            }
            else
            {
                //Advancing to next image
                if (currentImg < capturedImages.Count - 1)
                {
                    currentImg++;
                }
                else
                {
                    currentImg = 0;
                }
            }

            //Display captured images
            capture = false;
            cameraBtn.gameObject.SetActive(true);
            cameraBtn.GetComponentInChildren<Text>().text = "Back to Camera";
            galleryBtn.GetComponentInChildren<Text>().text = "Next Image";
            aspectFit.aspectRatio = capturedImages[currentImg].width / capturedImages[currentImg].height;
            imgViewer.texture = capturedImages[currentImg]; //Display captured image
            cam.SetActive(false);
            imgIndex.text = (currentImg + 1) + " / " + imagesToCapture;
            imgViewer.gameObject.SetActive(true);
        }
    }

    //Start automatic image capturing
    public void startImageCapturing()
    {
        startCapturing = true;
        startCapturingBtn.SetActive(false);
        captureProgress.gameObject.SetActive(true);
    }

    //Enables the gyroscope if the device supports a gyroscope
    bool enableGyro()
    {
        if (SystemInfo.supportsGyroscope) //Gyroscope available
        {
            gyro = Input.gyro; //Get device's gyroscope
            gyro.enabled = true; //Enable device's gyroscope
            return true;
        }
        else //Gyroscope not available
        {
            return false;
        }
    }

    //Gets the orientation of the device
    void getOrientation()
    {
        if (gyroEnabled)
        {
            orient = gyro.attitude.eulerAngles; //Get device orientation
            angleHorizontal = orient.z; //Rotation in degrees of device on the horizontal axis
            angleVertical = orient.y; //Rotation in degrees of device on the vertical axis
            checkOrientation(); //Calculates if an image needs to be captured at this orientation
        }
    }

    //Checks if an image needs to be captured at the device's current orientation
    void checkOrientation()
    {
        //Check if device is within a valid rotational range on the vertical axis
        if ((0 <= angleVertical && angleVertical <= 90) || (270 <= angleVertical && angleVertical <= 360))
        {
            getCaptureRegions(); //Get current capture regions

            //Check if capture regions have been captured yet
            needCapture = !captureInfo[curHorCapReg][curVertCapReg];
        }
        else
        {
            needCapture = false;
        }
    }

    //Calculates what capture regions the device is currently in on the horizontal and vertical axes
    void getCaptureRegions()
    {
        //Get horizontal capture region
        for (int i = 0; i < horizontalCaptureRegions; i++)
        {
            float nextCaptureRegion = horizontalCaptureRegionSize * (i + 1);
            Debug.Log("nextCaptureRegion: " + nextCaptureRegion);
            if (angleHorizontal < nextCaptureRegion) //Horizontal Angle is within current capture region
            {
                Debug.Log("angleHorizontal < nextCaptureRegion");
                curHorCapReg = i * horizontalCaptureRegionSize;
                break;
            }
        }
        Debug.Log("curHorCapReg: " + curHorCapReg);

        //Get vertical capture region
        if (angleVertical > 90) //If vertical angle is between 270 & 360
        {
            angleVertical -= 270; //Making calculations easier (making vertical angle between 0 & 90)
        }
        for (int i = 0; i < verticalCaptureRegions; i++)
        {
            float nextCaptureRegion = verticalCaptureRegionSize * (i + 1);
            Debug.Log("nextCaptureRegion: " + nextCaptureRegion);
            if (angleVertical < nextCaptureRegion) //Horizontal Angle is within current capture region
            {
                Debug.Log("angleVertical < nextCaptureRegion");
                curVertCapReg = i * verticalCaptureRegionSize;
                break;
            }
        }
        Debug.Log("curVertCapReg: " + curVertCapReg);
    }

    //Setting up dictionary to match the number of images to capture
    void intializeCaptureInfo()
    {
        calculateCaptureRegions();
        Debug.Log("imagesToCapture: " + imagesToCapture);
        Debug.Log("horizontalCaptureRegions: " + horizontalCaptureRegions + " Region Size: " + horizontalCaptureRegionSize);
        Debug.Log("verticalCaptureRegions: " + verticalCaptureRegions + " Region Size: " + verticalCaptureRegionSize);

        captureInfo = new Dictionary<float, Dictionary<float, bool>>();
        for (int i = 0; i < horizontalCaptureRegions; i++)
        {
            Dictionary<float, bool> verticalAngles = new Dictionary<float, bool>();
            for (int j = 0; j < verticalCaptureRegions; j++)
            {
                verticalAngles.Add(j * verticalCaptureRegionSize, false);
            }
            
            captureInfo.Add(i * horizontalCaptureRegionSize, verticalAngles);
        }
    }

    //Calculate the number and size of capture regions
    void calculateCaptureRegions()
    {
        while (90 % verticalCaptureRegions != 0) //Ensures that all verticalCaptureRegions are the same size (I won't get a verticalCaptureRegion half as big)
        {
            verticalCaptureRegions++;
        }
        verticalCaptureRegionSize = 90 / verticalCaptureRegions;

        horizontalCaptureRegions = imagesToCapture / verticalCaptureRegions;
        while (imagesToCapture % verticalCaptureRegions != 0) //Ensures that all horizontalCaptureRegions are the same size (I won't get a horizontalCaptureRegion half as big)
        {
            imagesToCapture++;
            horizontalCaptureRegions = imagesToCapture / verticalCaptureRegions;
        }
        horizontalCaptureRegionSize = 360 / horizontalCaptureRegions; //Calculate the size of each captureRegion
    }

    //Records captured image orientation
    void recordCaptureInfo()
    {
        captureInfo[curHorCapReg][curVertCapReg] = true; //Record an  image capture in the current capture regions
    }

    //Gets the acceleration of the device
    void checkAcceleration()
    {
        if (gyroEnabled)
        {
            acceleration = gyro.userAcceleration; //Get device acceleration
            bool okMovementX = acceleration.x <= movementThreshold.x;
            bool okMovementY = acceleration.y <= movementThreshold.y;
            bool okMovementZ = acceleration.z <= movementThreshold.z;
            if (okMovementX && okMovementY && okMovementZ) //Device is at or below movement threshold on all axes
            {
                deviceMoving = false;
            }
            else
            {
                deviceMoving = true;
            }
        }
    }

    //Automatically captures images at the right orientations if the device is relatively still
    void autoCapture()
    {
        doneCapturing = capturedImages.Count == imagesToCapture;
        captureProgress.value = capturedImages.Count; //Update progress bar
        if (!deviceMoving && needCapture && capture) //If device isn't moving and the current device orientation needs to be captured and images aren't being viewed, capture image
        {
            StartCoroutine(captureImage());
        }
    }

    //Called when the user clicks create model after capturing the images
    public void createModel()
    {
        doneCapturingIcon.SetActive(false);
        createModelPanel.SetActive(false);
        creatingModel = true;
        UploadImages.Instance.UploadImagesToFirebase(capturedImagePaths);
    }

    //Deletes any images taken and saved to the device when an application quits
    private void OnApplicationPause(bool pause)
    {
        if (pause) //Application paused (or quit)
        {
            deleteFiles();
        }
        else //Unpaused/resumed
        {
            for (int i = 0; i < capturedImages.Count; i++) //Recreating images from stored textures and storing at the paths in capturedImagePaths[]
            {
                byte[] img = ImageConversion.EncodeToJPG(capturedImages[i], 100);
                File.WriteAllBytes(capturedImagePaths[i], img);
            }
        }
    }

    //Deletes all the image files saved to the device
    public void deleteFiles()
    {
        for (int i = 0; i < capturedImagePaths.Count; i++) //Deleting images from device storage
        {
            if (File.Exists(capturedImagePaths[i])) //Make sure file exists
            {
                File.Delete(capturedImagePaths[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startCapturing && !doneCapturing)
        {
            getOrientation();
            checkAcceleration();
            autoCapture();

            //debugText.text = "Orientation: " + orient + "\n Acceleration: " + acceleration + "\n Horizontal Angle: " + angleHorizontal + "\n Vertical Angle: " + angleVertical;
            //debugText.text = "Horizontal Angle: " + angleHorizontal + "\nVertical Angle: " + angleVertical + "\nHorizontal Capture Region: " + curHorCapReg + "\n Vertical Capture Region: " + curVertCapReg + "\nNeed to Capture: " + needCapture + "\nAcceleration: " + acceleration + "\nDevice Moving: " + deviceMoving + "\n" + Application.persistentDataPath;
        }
        else if (doneCapturing)
        {
            captureProgress.gameObject.SetActive(false); //Disable progress bar
            if (!creatingModel) //User hasn't clicked create model yet
            {
                doneCapturingIcon.SetActive(true);
                createModelPanel.SetActive(true);
            }
        }
    }
}
