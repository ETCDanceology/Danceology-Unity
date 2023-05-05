/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global GameManager class responsible for managing game flow
/// </summary>
public class GameManager : CSingletonMono<GameManager>
{
    public GameState state = GameState.GameStart;                                           // Current game state
    public PlayerInputDevice playerInputDevice = PlayerInputDevice.CameraWithWholeBody;     // Current player input 
    public LevelType levelType;                                                             // Current level type

    public PoseDetector poseDetector;                               // Reference to the PoseDetector instance within the current scene
    private AnimModelMovement modelController;                      // Reference to the AnimModelMovement instance within the current scene
    private LevelData currentLevelData;                             // Refernece to current level data

    [Tooltip("Number of seconds to use for camera calibration time")]
    public float maxCalibTime = 3.0f;
    private float calibTimeElapsed = 0.0f;                          // Internal timer for calibration

    [Tooltip("Approximate BPM value used for rhythm-based animations")]
    public float bpm = 60;
    public float beat_second { get; private set; }                  // Computed value, number of seconds per beat
    public bool canDetect { get; private set; }                     // Computed value, returns whether camera detection is enabled
    public bool waitingForKeyPose { get; private set; }             // Flag on whether we are waiting for a key pose frame

    private int levelScore;                                         // Scoring for current level
    private int levelScoreCount;                                    // Number of times user has been scored for current level

    private void Start()
    {
        beat_second = 60 / bpm;
        modelController = FindObjectOfType<AnimModelMovement>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.AddListener(EventTypes.FinishedLevelStart, _StartLevel);
        EventBus.AddListener<AnimModelMovement>(EventTypes.ModelStoppedPlaying, FinishLevel);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(EventTypes.FinishedLevelStart, _StartLevel);
        EventBus.RemoveListener<AnimModelMovement>(EventTypes.ModelStoppedPlaying, FinishLevel);
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        // Perform different actions based on current game state
        switch (state)
        {
            case GameState.Loading:
                CheckLoadingUpdate();
                break;
            case GameState.LevelPrepare:
                LevelPreparationUpdate();
                break;
            case GameState.InLevel:
                InLevelUpdate();
                break;
            case GameState.LevelStart:
            case GameState.LevelEnd:
                // Do nothing in update loop
                break;
        }
    }

    /// <summary>
    /// Start the game
    /// </summary>
    public void StartGame()
    {
        SetState(GameState.GameStart);
    }

    /// <summary>
    /// Updates the current game state
    /// </summary>
    public void SetState(GameState g_state)
    {
        state = g_state;
        switch (state)
        {
            case GameState.Loading:
                LoadLevelInitial();
                break;
            case GameState.LevelPrepare:
                PrepareForLevelInitial();
                break;
            case GameState.LevelStart:
                LevelStartInitial();
                break;
            case GameState.InLevel:
                InLevelInitial();
                break;
            case GameState.LevelEnd:
                LevelEndInitial();
                break;
        }
    }

    /// <summary>
    /// Disables camera and stops pose detection
    /// </summary>
    public void DisableCamera()
    {
        playerInputDevice = PlayerInputDevice.NoCamera;
        canDetect = false;
        poseDetector.EndBehavior();
    }

    /// <summary>
    /// Enables player webcam pose detection
    /// </summary>
    public void EnableDetection()
    {
        canDetect = true;
    }

    /// <summary>
    /// Disables player webcam pose detection
    /// </summary>
    public void DisableDetection()
    {
        canDetect = false;
    }

    #region Loading Phase
    /// <summary>
    /// Initial function once level has started
    /// </summary>
    private void LoadLevelInitial()
    {
        if (levelType == LevelType.Intro)
        {
            // No additional loading needs to be done; directly swap to intro level
            SceneManager.LoadScene("IntroScene");
            return;
        }

        SceneManager.LoadScene("MainScene");
        currentLevelData = DataLoad.GetLevelData();
        Invoke(nameof(StartLoadingProcess), 0.5f);

        //reset calib timer
        calibTimeElapsed = 0;
    }

    /// <summary>
    /// Starts process for loading level
    /// </summary>
    private void StartLoadingProcess()
    {
        UIUtils.instance.HideUI(GameProgress.instance.gameObject);
        UIUtils.instance.HideUI(InLevelUI.instance.guideUIParent);
        FindObjectOfType<ToNextLevel>().HideIn();

        poseDetector = FindObjectOfType<PoseDetector>();
        poseDetector.LoadInitialize();
        InLevelUI.instance.PrepareForLevelInit();
    }

    /// <summary>
    /// Checks whether the loading has finished
    /// </summary>
    private void CheckLoadingUpdate()
    {
        if (poseDetector && poseDetector.CheckLoading())
        {
            EventBus.Broadcast(EventTypes.FinishedDataLoading);
        }
    }

    /// <summary>
    /// Called when loading finishes
    /// </summary>
    public void FinishedLoading(bool have_model)
    {
        if (!have_model)
        {
            // No level preparation needed, directly start the game
            SetState(GameState.LevelStart);
            return;
        }

        SetState(GameState.LevelPrepare);
    }
    #endregion

    #region Prepare for Level
    /// <summary>
    /// Initial level preparation steps
    /// </summary>
    private void PrepareForLevelInitial()
    {
        if (playerInputDevice == PlayerInputDevice.NoCamera)
        {
            // If no camera is being used, skip directly to level start
            InLevelUI.instance.ProceedWithNoCamera();
            SetState(GameState.LevelStart);
            return;
        }

        OutputDataReader.instance.EnableVisibility();
    }

    /// <summary>
    /// Update function for level calibration, check that player is within the screen
    /// </summary>
    private void LevelPreparationUpdate()
    {
        // Detect player using webcam
        poseDetector.Detect();

        int numJointsDetected = poseDetector.GetNumJointsOnScreen();
        if (PoseDetector.IsEntireBodyOnScreen(numJointsDetected))
        {
            calibTimeElapsed += Time.deltaTime;
            if (calibTimeElapsed >= maxCalibTime)
            {
                // Calibration successful! Starting level
                ObjectPool.instance.DisableAll();
                SetState(GameState.LevelStart);
            }
        }
        else
        {
            // Player out of screen; require further calibration
            calibTimeElapsed = Mathf.Max(0, calibTimeElapsed - Time.deltaTime);
        }

        InLevelUI.instance.SetCalibUI(numJointsDetected, calibTimeElapsed, maxCalibTime);
    }
    #endregion

    #region Level Start
    /// <summary>
    /// Initial call to start current level
    /// </summary>
    public void LevelStartInitial()
    {
        // Re-initialize scoring information
        levelScore = 0;
        levelScoreCount = 0;

        // BGM + UI updates
        BGMManager.instance.PlayBGM();
        UIUtils.instance.ShowUI(GameProgress.instance.gameObject);
        UIUtils.instance.ShowUI(InLevelUI.instance.guideUIParent, false);

        if (playerInputDevice != PlayerInputDevice.NoCamera)
        {
            canDetect = true;
            OutputDataReader.instance.EnableVisibility();
        }

        if (levelType == LevelType.Guided)
        {
            // Don't need next pose UI in guided level
            UIUtils.instance.HideUI(FindObjectOfType<NextPoseUI>().gameObject);
            InLevelUI.instance.TransitionToInGameUI(false); // Don't play webcam shrinking animation
        }
        else
        {
            InLevelUI.instance.TransitionToInGameUI(true);
        }
    }

    /// <summary>
    /// Once everything is set up, actually start the current level
    /// </summary>
    private void _StartLevel()
    {
        SetState(GameState.InLevel);
    }
    #endregion

    #region In Level
    /// <summary>
    /// Initial call to start the level
    /// </summary>
    public void InLevelInitial()
    {
        float playbackFPS;
        switch (levelType)
        {
            case LevelType.Guided:
                playbackFPS = 16;
                break;
            case LevelType.UnguidedSlow:
                playbackFPS = 24;
                break;
            case LevelType.UnguidedFast:
            default:
                playbackFPS = 32;
                break;
        }

        GameProgress.instance.ResetProgress();

        // Start 3D model animation and movement comparisons
        FindObjectOfType<AnimModelMovement>().StartPlaying(currentLevelData, levelType, playbackFPS);
        MovementCompare.instance.SetGameStart(playbackFPS);
    }

    /// <summary>
    /// Update call while in level
    /// </summary>
    public void InLevelUpdate()
    {
        if (playerInputDevice != PlayerInputDevice.NoCamera && canDetect)
        {
            poseDetector.Detect();
        }

        if (waitingForKeyPose)
        {
            MovementCompare.instance.EnableCompare();
        }
    }

    /// <summary>
    /// Add a score related to current frame
    /// </summary>
    public void AddNewScore(float score)
    {
        levelScore += (int)score;
        levelScoreCount++;
    }
    #endregion

    #region Level End
    /// <summary>
    /// Finishes current level, called once a level has ended
    /// </summary>
    public void FinishLevel(AnimModelMovement model)
    {
        SetState(GameState.LevelEnd);
    }

    /// <summary>
    /// Handles changes necessary at the end of the level
    /// </summary>
    public void LevelEndInitial()
    {
        poseDetector.EndBehavior();
        BGMManager.instance.StopBGM(true);
        OutputDataReader.instance.DisableVisibility();

        // If at least one scoring instance occurred && on appropriate level and input device, calculate and display scoring
        if (levelScoreCount != 0 && playerInputDevice != PlayerInputDevice.NoCamera && levelType != LevelType.Guided)
        {
            ResultUI.SharedInstance.SetScore(levelScore / levelScoreCount);
            ResultUI.SharedInstance.FadeIn();
        }
        else
        {
            FindObjectOfType<ToNextLevel>().ShowUp();
        }
    }
    #endregion

    /// <summary>
    /// Exits current scene and returns to the main menu
    /// </summary>
    public void ExitToMainMenu()
    {
        StartCoroutine(_ExitToMainMenu());
    }

    /// <summary>
    /// Exits to main menu asynchronously to allow for correct UI display
    /// </summary>
    IEnumerator _ExitToMainMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        MenuUI.instance.StartGame();
    }
}
