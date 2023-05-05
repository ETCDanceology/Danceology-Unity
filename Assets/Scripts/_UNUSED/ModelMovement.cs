/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 * 
 * This code was initially used to programmatically generate animations from 3D joint position
 * data. It is no longer being used.
 **/

using UnityEngine;

public class ModelMovement : MonoBehaviour
{
    // Joint position and bone
    private JointPoint[] jointPoints;
    public JointPoint[] JointPoints { get { return jointPoints; } }

    private Vector3 initPosition; // Initial center position
    private float initFloorPositionY; // Initial floor position

    // Move in z direction
    private float centerTall = 224 * 0.75f;
    private float tall = 224 * 0.75f;
    private float prevTall = 224 * 0.75f;
    public float ZScale = 0.8f;

    public GameObject nose;

    [Header("Smoothing Parameters")]
    public float KalmanParamQ = 0.001f;
    public float KalmanParamR = 0.0015f;
    public float LowPassParam = 0.1f;
    public TutorialKeyFrameData levelTutorialData;
    private float playbackFPS = 30.0f;

    private Animator anim;
    public LevelData levelData;
    private LevelType levelType;
    private bool loopPlayback;

    private float timer;
    private bool playLevel;

    [Header("Visible for debugging purposes, press N to jump to next tutorial index")]
    public int tutorial_ind = 0;
    public int currentPoseIndex = 0;

    void Awake()
    {
        levelTutorialData.StackKeyframeTutorialData();
        InitializeJointPoints();
    }

    public void StartPlaying(LevelData levelData, LevelType levelType, float playbackFPS, bool loopPlayback = false)
    {
        this.levelData = levelData;
        GameProgress.instance.SetTotalAction(levelData.poseData.Length);

        this.levelType = levelType;
        this.playbackFPS = playbackFPS;
        this.loopPlayback = loopPlayback;

        currentPoseIndex = 0;
        timer = 0;
        SetPose();
        playLevel = true;
        tutorial_ind = 0;
        EventBus.Broadcast(EventTypes.ModelStartedPlaying, this);
    }

    public void StopPlaying()
    {
        playLevel = false;

        EventBus.Broadcast(EventTypes.ModelStoppedPlaying, this);
    }

    public int GetTotalFrames()
    {
        return levelData.poseData.Length;
    }

    public int GetCurrentFrame()
    {
        return currentPoseIndex;
    }

    public float GetPlayFPS()
    {
        return playbackFPS;
    }


    public void SetPlayFPS(int fps)
    {
        playbackFPS = fps;
    }
    public Vector3? GetJointPosition(PositionIndex positionIndex)
    {
        return jointPoints[positionIndex.Int()]?.Transform?.position;
    }
    
    private void Update()
    {
        if (levelType == LevelType.Guided)
        {
            if (isTutorialKeyPoseFrame(currentPoseIndex))
            {
                HandleTutorialKeyPose();
            }
        }
        if (!playLevel) return;

        timer += Time.deltaTime;
        if (timer >= (1 / playbackFPS)) 
        {
            currentPoseIndex += 1;//Mathf.FloorToInt(timer * playbackFPS);

            GameProgress.instance.UpdateProgress(currentPoseIndex);
            timer = 0;
            if (currentPoseIndex >= levelData.poseData.Length)
            {
                if (loopPlayback)
                {
                    currentPoseIndex = 0;
                }
                else
                {
                    StopPlaying();
                    return;
                }
            }

            SetPose(true);
            MovementCompare.instance.CallNextFrameData();
            if (levelData.isKeyPoseFrame(currentPoseIndex))
            {
                HandleKeyPose();
            }
        }
    }

    #region tutorial

    /// <summary>
    /// check is tutorial this frame is a tutorial key frame
    /// </summary>
    /// <param name="currentPose"></param>
    /// <returns></returns>
    private bool isTutorialKeyPoseFrame(int currentPose)
    {
        if (levelTutorialData.keyframeTutorialData != null)
        {
            if (tutorial_ind >= levelTutorialData.keyframeTutorialData.Count)
            {
                return false;
            }
            else if (currentPose >= levelTutorialData.keyframeTutorialData[tutorial_ind].frameCount)
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// broadcast a event to trigger tutorial key
    /// </summary>
    private void HandleTutorialKeyPose()
    {
        EventBus.Broadcast(EventTypes.ReachedTutorialKey, levelTutorialData.keyframeTutorialData[tutorial_ind]);
    }

    /// <summary>
    /// to next tutorial ind point
    /// </summary>
    /// <param name="debugJumpToFrame">debug param, set to true if you want to jump to the next frame</param>
    public void TutorialToNext(bool debugJumpToFrame = false)
    {
        tutorial_ind++;
        if (debugJumpToFrame)
        {
            currentPoseIndex = GetCurrentTutorial().frameCount;
            HandleTutorialKeyPose();
        }
    }

    /// <summary>
    /// tutorial stop playing
    /// </summary>
    public void TutorialStopPlaying()
    {
        playLevel = false;
    }

    public TutorialData GetCurrentTutorial()
    {
        return levelTutorialData.keyframeTutorialData[tutorial_ind];
    }

    public void ContinuePlaying()
    {
        playLevel = true;
    }
    /// <summary>
    /// continue playing with a reset ind
    /// </summary>
    /// <param name="reset_ind"></param>
    public void ContinuePlaying(int reset_ind)
    {
       
        if (reset_ind - 1 >= 0)
        {
            currentPoseIndex = reset_ind - 1;
            SetPose();
            currentPoseIndex = reset_ind;
            SetPose(true);
        }
        else
        {
            currentPoseIndex = reset_ind;
            SetPose();

        }
        
        ContinuePlaying();
    }

    public bool isStop()
    {
        return !playLevel;
    }
    #endregion

    private void HandleKeyPose()
    {
        EventBus.Broadcast(EventTypes.ReachedKeyFrame, currentPoseIndex);
    }

    private float prevDy = 0;
    private void InitializeJointPoints()
    {
        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for (var i = 0; i < PositionIndex.Count.Int(); i++) jointPoints[i] = new JointPoint();

        anim = GetComponent<Animator>();
        MovementUtil.InitializeJointPoints(jointPoints, anim);
        jointPoints[PositionIndex.nose.Int()].Transform = nose.transform;

        // Set Inverse
        var forward = MovementUtil.TriangleNormal(jointPoints[PositionIndex.hip.Int()].Transform.position, jointPoints[PositionIndex.lUpperLeg.Int()].Transform.position, jointPoints[PositionIndex.rUpperLeg.Int()].Transform.position);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }
        }

        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = MovementUtil.GetInverse(jointPoint, jointPoint.Child, forward);
                jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
            }
        }

        var hip = jointPoints[PositionIndex.hip.Int()];
        initPosition = hip.Transform.position;
        hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hip.InverseRotation = hip.Inverse * hip.InitRotation;

        initFloorPositionY = Mathf.Min(jointPoints[PositionIndex.lToeBase.Int()].Transform.position.y, jointPoints[PositionIndex.rToeBase.Int()].Transform.position.y);
        prevDy = initPosition.y - initFloorPositionY;

        // For Head Rotation
        var head = jointPoints[PositionIndex.head.Int()];
        head.InitRotation = head.Transform.rotation;
        var gaze = jointPoints[PositionIndex.nose.Int()].Transform.position - head.Transform.position;
        head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
        head.InverseRotation = head.Inverse * head.InitRotation;

        var lHand = jointPoints[PositionIndex.lHand.Int()];
        var lf = MovementUtil.TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lHandPinky.Int()].Pos3D, jointPoints[PositionIndex.lHandIndex.Int()].Pos3D);
        lHand.InitRotation = lHand.Transform.rotation;
        lHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.lHandIndex.Int()].Transform.position - jointPoints[PositionIndex.lHandPinky.Int()].Transform.position, lf));
        lHand.InverseRotation = lHand.Inverse * lHand.InitRotation;

        var rHand = jointPoints[PositionIndex.rHand.Int()];
        var rf = MovementUtil.TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rHandIndex.Int()].Pos3D, jointPoints[PositionIndex.rHandPinky.Int()].Pos3D);
        rHand.InitRotation = jointPoints[PositionIndex.rHand.Int()].Transform.rotation;
        rHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.rHandIndex.Int()].Transform.position - jointPoints[PositionIndex.rHandPinky.Int()].Transform.position, rf));
        rHand.InverseRotation = rHand.Inverse * rHand.InitRotation;

        jointPoints[PositionIndex.hip.Int()].score3D = 1f;
        jointPoints[PositionIndex.neck.Int()].score3D = 1f;
        jointPoints[PositionIndex.nose.Int()].score3D = 1f;
        jointPoints[PositionIndex.head.Int()].score3D = 1f;
        jointPoints[PositionIndex.spine.Int()].score3D = 1f;
    }

    private void SetPose(bool useKalmanUpdate = false, bool useLowPassFilter = false)
    {
        if (levelData == null) return;
        if (jointPoints == null) return;

        PoseData poseData = levelData.poseData[currentPoseIndex];
        Keypoint3D[] keypoints3D = poseData.keypoints3D;

        for (int i = 0; i < keypoints3D.Length; i++)
        {
            Keypoint3D keyPoint = keypoints3D[i];

            jointPoints[i].Now3D.x = keyPoint.x;
            jointPoints[i].Now3D.y = -keyPoint.y;
            jointPoints[i].Now3D.z = -keyPoint.z;
            jointPoints[i].score3D = keyPoint.score;
        }

        // Calculate hip location
        var lc = (jointPoints[PositionIndex.rUpperLeg.Int()].Now3D + jointPoints[PositionIndex.lUpperLeg.Int()].Now3D) / 2f;
        jointPoints[PositionIndex.hip.Int()].Now3D = (jointPoints[PositionIndex.abdomenUpper.Int()].Now3D + lc) / 2f;

        // Calculate neck location
        jointPoints[PositionIndex.neck.Int()].Now3D = (jointPoints[PositionIndex.rArm.Int()].Now3D + jointPoints[PositionIndex.lArm.Int()].Now3D) / 2f;

        // Calculate head location
        var cEar = (jointPoints[PositionIndex.rEar.Int()].Now3D + jointPoints[PositionIndex.lEar.Int()].Now3D) / 2f;
        var hv = cEar - jointPoints[PositionIndex.neck.Int()].Now3D;
        var nhv = Vector3.Normalize(hv);
        var nv = jointPoints[PositionIndex.nose.Int()].Now3D - jointPoints[PositionIndex.neck.Int()].Now3D;
        jointPoints[PositionIndex.head.Int()].Now3D = jointPoints[PositionIndex.neck.Int()].Now3D + nhv * Vector3.Dot(nhv, nv);

        // Calculate spine location
        jointPoints[PositionIndex.spine.Int()].Now3D = jointPoints[PositionIndex.abdomenUpper.Int()].Now3D;

        foreach (var jp in jointPoints)
        {
            if (useKalmanUpdate)
            {
                MovementUtil.KalmanUpdate(jp, KalmanParamQ, KalmanParamR);
            }
            else
            {
                MovementUtil.ClearKalman(jp);
                jp.Pos3D = jp.Now3D;
            }

            if (useLowPassFilter)
            {
                jp.PrevPos3D[0] = jp.Pos3D;
                for (var i = 1; i < jp.PrevPos3D.Length; i++)
                {
                    jp.PrevPos3D[i] = jp.PrevPos3D[i] * LowPassParam + jp.PrevPos3D[i - 1] * (1f - LowPassParam);
                }
                jp.Pos3D = jp.PrevPos3D[jp.PrevPos3D.Length - 1];
            }
        }

        PoseUpdate();
    }
    public void PoseUpdate()
    {
        // calculate movement range of z-coordinate from height
        var t1 = Vector3.Distance(jointPoints[PositionIndex.head.Int()].Pos3D, jointPoints[PositionIndex.neck.Int()].Pos3D);
        var t2 = Vector3.Distance(jointPoints[PositionIndex.neck.Int()].Pos3D, jointPoints[PositionIndex.spine.Int()].Pos3D);
        var pm = (jointPoints[PositionIndex.rUpperLeg.Int()].Pos3D + jointPoints[PositionIndex.lUpperLeg.Int()].Pos3D) / 2f;
        var t3 = Vector3.Distance(jointPoints[PositionIndex.spine.Int()].Pos3D, pm);
        var t4r = Vector3.Distance(jointPoints[PositionIndex.rUpperLeg.Int()].Pos3D, jointPoints[PositionIndex.rLeg.Int()].Pos3D);
        var t4l = Vector3.Distance(jointPoints[PositionIndex.lUpperLeg.Int()].Pos3D, jointPoints[PositionIndex.lLeg.Int()].Pos3D);
        var t4 = (t4r + t4l) / 2f;
        var t5r = Vector3.Distance(jointPoints[PositionIndex.rLeg.Int()].Pos3D, jointPoints[PositionIndex.rFoot.Int()].Pos3D);
        var t5l = Vector3.Distance(jointPoints[PositionIndex.lLeg.Int()].Pos3D, jointPoints[PositionIndex.lFoot.Int()].Pos3D);
        var t5 = (t5r + t5l) / 2f;
        var t = t1 + t2 + t3 + t4 + t5;

        // Low pass filter in z direction
        tall = t * 0.7f + prevTall * 0.3f;
        prevTall = tall;

        if (tall == 0)
        {
            tall = centerTall;
        }
        var dz = (centerTall - tall) / centerTall * ZScale;

        // movement and rotation of center
        var forward = MovementUtil.TriangleNormal(jointPoints[PositionIndex.lArm.Int()].Pos3D, jointPoints[PositionIndex.lUpperLeg.Int()].Pos3D, jointPoints[PositionIndex.rUpperLeg.Int()].Pos3D);
        jointPoints[PositionIndex.hip.Int()].Transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initFloorPositionY, initPosition.z + dz);
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation;

        // rotate each of bones
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
            }
        }

        // Head Rotation
        var gaze = jointPoints[PositionIndex.nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        var f = MovementUtil.TriangleNormal(jointPoints[PositionIndex.nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        var head = jointPoints[PositionIndex.head.Int()];
        head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

        // Wrist rotation
        var lHand = jointPoints[PositionIndex.lHand.Int()];
        var lf = MovementUtil.TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lHandPinky.Int()].Pos3D, jointPoints[PositionIndex.lHandIndex.Int()].Pos3D);
        lHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lHandIndex.Int()].Pos3D - jointPoints[PositionIndex.lHandPinky.Int()].Pos3D, lf) * lHand.InverseRotation;

        var rHand = jointPoints[PositionIndex.rHand.Int()];
        var rf = MovementUtil.TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rHandIndex.Int()].Pos3D, jointPoints[PositionIndex.rHandPinky.Int()].Pos3D);
        rHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rHandIndex.Int()].Pos3D - jointPoints[PositionIndex.rHandPinky.Int()].Pos3D, rf) * rHand.InverseRotation;

        // Update hip position
        var hipPosY = jointPoints[PositionIndex.hip.Int()].Transform.position.y;
        var footPosY = Mathf.Min(jointPoints[PositionIndex.lToeBase.Int()].Transform.position.y, jointPoints[PositionIndex.rToeBase.Int()].Transform.position.y);
        var dy = hipPosY - footPosY;

        dy = dy * 0.7f + prevDy * 0.3f;
        prevDy = dy;

        var hipPosition = jointPoints[PositionIndex.hip.Int()].Transform.position;
        jointPoints[PositionIndex.hip.Int()].Transform.position = new Vector3(hipPosition.x, hipPosition.y + dy, hipPosition.z);
    }
}
