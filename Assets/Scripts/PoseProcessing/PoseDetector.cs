/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class PoseDetector : MonoBehaviour
{
    #region Engine Struct
    /// <summary>
    /// Keeps track of the current inference backend, model execution interface, 
    /// and model type
    /// </summary>
    private struct Engine
    {
        public WorkerFactory.Type workerType;
        public IWorker worker;

        public Engine(WorkerFactory.Type workerType, Model model)
        {
            this.workerType = workerType;
            worker = WorkerFactory.CreateWorker(workerType, model);
        }
    }
    #endregion

    #region Global Variables
    [Tooltip(".onnx model used to process webcam input")]
    public NNModel modelAsset;  // Reference to the onnx model

    public OutputDataReader outputDataProcessor;
    private Model cal_model;    // Calculated model that will be used in barracuda 

    [Tooltip("Screen for viewing preprocessed images")]
    public Transform videoScreen;

    [Tooltip("Backend type to use when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    [Tooltip("Dimensions of the image being fed to the model")]
    public Vector2Int imageDims = new Vector2Int(256, 256);

    [Tooltip("Camera capture framerate")]
    public int camera_framerate = 30;

    [Tooltip("Texture used to capture webcam input")]
    public WebCamTexture webCamTexture;

    // The interface used to execute the neural network
    private Engine engine;
    private Vector2Int videoDims;               // Webcam original input dims
    private RenderTexture videoTexture;         // Render texture to show webcam output
    private RenderTexture rTex;                 // Render texture used to create input to model
    private Tensor input;                       // Input tensor fed into model
    private bool successfullyLoaded;            // Flag indicating successful loading
    #endregion

    #region Main Behavior

    /// <summary>
    /// In loading phase, initialize camera and model and UI elements
    /// </summary>
    public void LoadInitialize()
    {
        rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGBHalf);
        successfullyLoaded = false;

        InitialWebCam();
        InitialVideoUI();
        CreateWorker();
    }

    /// <summary>
    /// Check whether model loading has completed
    /// </summary>
    public bool CheckLoading()
    {
        if (GameManager.instance.state != GameState.Loading) return false;
        if (GameManager.instance.playerInputDevice == PlayerInputDevice.NoCamera) return true;
        if (successfullyLoaded) return true;

        if (webCamTexture == null)
        {
            InitialWebCam();
            return false;
        } // Try to initialize the web camera again later

        Graphics.Blit(webCamTexture, rTex);

        // Prepare the input image to be fed to the selected model
        ProcessImageInput(rTex);

        // Execute neural network with the provided input
        engine.worker.Execute(input); //one forward
        input.Dispose(); // Release GPU resources allocated for the Tensor

        Tensor firstOutput = engine.worker.PeekOutput("output1");
        Tensor secondOutput = engine.worker.PeekOutput("output2");

        if (secondOutput != null)
        {
            successfullyLoaded = true;
        }

        firstOutput.Dispose();
        secondOutput.Dispose();
        return successfullyLoaded;
    }

    /// <summary>
    /// In the calibration scene, returns the number of person joints detected
    /// </summary>
    public int GetNumJointsOnScreen()
    {
        Graphics.Blit(webCamTexture, videoTexture); // Render webcam input on UI screen

        // Copy the src RenderTexture to the new rTex RenderTexture
        Graphics.Blit(videoTexture, rTex);

        // Prepare the input image to be fed to the selected model
        ProcessImageInput(rTex);

        // Execute neural network with the provided input
        engine.worker.Execute(input); // one forward pass

        // Release GPU resources allocated for the Tensor
        input.Dispose();

        var output = OpenPoseOutputProcessor.instance.ProcessOpenPoseBody(engine.worker);
        if (output.Item2.Count < 1) return 0;

        int body_detect = 0;
        foreach (int i in output.Item2[0].body_part)
        {
            if (i != -1)
            {
                body_detect++;
            }
        }
        return body_detect;
    }

    /// <summary>
    /// Given a number of joints, returns whether the entire person can be considered on the screen
    /// These values are purposefully looser to accomodate detection inaccuracies
    /// </summary>
    public static bool IsEntireBodyOnScreen(int num_joints_detected)
    {
        switch (GameManager.instance.playerInputDevice)
        {
            case PlayerInputDevice.CameraWithWholeBody:
                return num_joints_detected >= 15;
            case PlayerInputDevice.CameraWithHalfBody:
                return num_joints_detected >= 7;
            default:
                return false;
        }
    }

    /// <summary>
    /// Main function that detects and provides output to processing
    /// </summary>
    public void Detect()
    {
        Graphics.Blit(webCamTexture, videoTexture); // Render webcam input on UI screen

        // Copy the src RenderTexture to the new rTex RenderTexture
        Graphics.Blit(videoTexture, rTex);

        // Prepare the input image to be fed to the selected model
        ProcessImageInput(rTex);

        // Execute neural network with the provided input
        engine.worker.Execute(input); // one forward pass

        // Release GPU resources allocated for the Tensor
        input.Dispose();

        // Process the model output
        ProcessModelOutput(engine.worker);
    }

    private void OnDisable()
    {
        EndBehavior();
    }

    /// <summary>
    /// When the pose detector is no longer being used, need to stop the web camera texture and dispose the model
    /// </summary>
    public void EndBehavior()
    {
        if (webCamTexture == null) return;

        webCamTexture.Stop();
        engine.worker.Dispose();
    }
    #endregion

    #region Util Functions

    /// <summary>
    /// Create the ML model engine worker
    /// </summary>
    private void CreateWorker()
    {
        if (GameManager.instance.playerInputDevice == PlayerInputDevice.NoCamera) return;

        cal_model = ModelLoader.Load(modelAsset); 
        workerType = WorkerFactory.ValidateType(workerType);    // Validate if backend is supported, otherwise use fallback type
        engine = new Engine(workerType, cal_model);             // Create a worker that will execute the model with the selected backend
    }

    /// <summary>
    /// Initialize the UI used for displaying the web camera capture
    /// </summary>
    private void InitialVideoUI()
    {
        if (GameManager.instance.playerInputDevice != PlayerInputDevice.NoCamera)
        {   
            UIUtils.instance.ShowUI(videoScreen.gameObject);
            videoScreen.GetComponent<RawImage>().texture = videoTexture;                // Set this render texture to rawimage
            videoScreen.GetComponent<RawImage>().material.mainTexture = videoTexture;   // Apply the new videoTexture to the VideoScreen Gameobject
        }
        else
        {
            UIUtils.instance.HideUI(videoScreen.gameObject);
        }
    }

    /// <summary>
    /// Initialize and start web camera input
    /// </summary>
    private void InitialWebCam()
    {
        if (GameManager.instance.playerInputDevice == PlayerInputDevice.NoCamera) return;
        if (webCamTexture != null) return;

        Application.targetFrameRate = camera_framerate;
        webCamTexture = new WebCamTexture(imageDims.x, imageDims.y, camera_framerate);
        webCamTexture.Play();

        if (webCamTexture != null)
        {
            videoDims.x = webCamTexture.height;
            videoDims.y = webCamTexture.width;
        }

        videoTexture = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGBHalf);
    }

    /// <summary>
    /// Process image into input for the model
    /// </summary>
    private void ProcessImageInput(RenderTexture image)
    {
        input = new Tensor(image, channels: 3);
    }

    /// <summary>
    /// Process model's output
    /// </summary>
    private void ProcessModelOutput(IWorker worker)
    {
        var output = OpenPoseOutputProcessor.instance.ProcessOpenPoseBody(worker);
        outputDataProcessor.ProcessInputImage(output.Item1, output.Item2);
    }
    #endregion
}
