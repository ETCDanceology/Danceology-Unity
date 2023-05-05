/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System;
using UnityEngine;

/// <summary>
/// Enum storing all potential joints within the 3D model for animation & reference purposes
/// </summary>
public enum PositionIndex : int
{
    nose = 0,
    rEyeInner,
    rEye,
    rEyeOuter,
    lEyeInner,
    lEye,
    lEyeOuter,
    rEar,
    lEar,
    mouthRight,
    mouthLeft,

    rArm,
    lArm,
    rForeArm,
    lForeArm,
    rHand,
    lHand,
    rHandPinky,
    lHandPinky,
    rHandIndex,
    lHandIndex,
    rHandThumb,
    lHandThumb,

    rUpperLeg,
    lUpperLeg,
    rLeg,
    lLeg,
    rFoot,
    lFoot,
    rHeel,
    lHeel,
    rToeBase,
    lToeBase,

    //Calculated coordinates
    abdomenUpper,
    hip,
    head,
    neck,
    spine,

    Count,
    None,
}

/// <summary>
/// Extension of enum to allow for each int <-> enum conversion
/// </summary>
public static partial class EnumExtend
{
    public static int Int(this PositionIndex i)
    {
        return (int)i;
    }
}

/// <summary>
/// Data class for joint points and positions
/// </summary>
[Serializable]
public class JointPoint
{
    public Vector3 Pos3D = new Vector3();
    public Vector3 Now3D = new Vector3();
    public Vector3[] PrevPos3D = new Vector3[6];
    public float score3D;

    // Bones
    public Transform Transform = null;
    public Quaternion InitRotation;
    public Quaternion Inverse;
    public Quaternion InverseRotation;

    public JointPoint Child = null;
    public JointPoint Parent = null;

    // For Kalman filter
    public Vector3 P = new Vector3();
    public Vector3 X = new Vector3();
    public Vector3 K = new Vector3();
}

/// <summary>
/// Miscellaneous utility functions used with regards to 3D model movement and animation
/// </summary>
public class MovementUtil : MonoBehaviour
{
    /// <summary>
    /// Initializes all joint point fields based on the humanoid avatar in the given Animator
    /// </summary>
    public static void InitializeJointPoints(JointPoint[] jointPoints, Animator anim)
    {
        // Right Arm
        jointPoints[PositionIndex.rArm.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[PositionIndex.rForeArm.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.rHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.rHandPinky.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        jointPoints[PositionIndex.rHandThumb.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.rHandIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);

        // Left Arm
        jointPoints[PositionIndex.lArm.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[PositionIndex.lForeArm.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[PositionIndex.lHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.lHandPinky.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        jointPoints[PositionIndex.lHandThumb.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.lHandIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);

        // Face
        jointPoints[PositionIndex.lEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.lEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[PositionIndex.rEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.rEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);

        // Right Leg
        jointPoints[PositionIndex.rUpperLeg.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[PositionIndex.rLeg.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.rFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.rHeel.Int()].Transform = null;
        jointPoints[PositionIndex.rToeBase.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightToes);

        // Left Leg
        jointPoints[PositionIndex.lUpperLeg.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.lLeg.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.lFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.lHeel.Int()].Transform = null;
        jointPoints[PositionIndex.lToeBase.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftToes);

        // etc
        jointPoints[PositionIndex.abdomenUpper.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.hip.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[PositionIndex.head.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.neck.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.spine.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);

        // Child Settings
        // Right Arm
        jointPoints[PositionIndex.rArm.Int()].Child = jointPoints[PositionIndex.rForeArm.Int()];
        jointPoints[PositionIndex.rForeArm.Int()].Child = jointPoints[PositionIndex.rHand.Int()];
        jointPoints[PositionIndex.rForeArm.Int()].Parent = jointPoints[PositionIndex.rArm.Int()];

        // Left Arm
        jointPoints[PositionIndex.lArm.Int()].Child = jointPoints[PositionIndex.lForeArm.Int()];
        jointPoints[PositionIndex.lForeArm.Int()].Child = jointPoints[PositionIndex.lHand.Int()];
        jointPoints[PositionIndex.lForeArm.Int()].Parent = jointPoints[PositionIndex.lArm.Int()];

        // Right Leg
        jointPoints[PositionIndex.rUpperLeg.Int()].Child = jointPoints[PositionIndex.rLeg.Int()];
        jointPoints[PositionIndex.rLeg.Int()].Child = jointPoints[PositionIndex.rFoot.Int()];
        jointPoints[PositionIndex.rFoot.Int()].Child = jointPoints[PositionIndex.rToeBase.Int()];
        jointPoints[PositionIndex.rFoot.Int()].Parent = jointPoints[PositionIndex.rLeg.Int()];

        // Left Leg
        jointPoints[PositionIndex.lUpperLeg.Int()].Child = jointPoints[PositionIndex.lLeg.Int()];
        jointPoints[PositionIndex.lLeg.Int()].Child = jointPoints[PositionIndex.lFoot.Int()];
        jointPoints[PositionIndex.lFoot.Int()].Child = jointPoints[PositionIndex.lToeBase.Int()];
        jointPoints[PositionIndex.lFoot.Int()].Parent = jointPoints[PositionIndex.lLeg.Int()];

        // etc
        jointPoints[PositionIndex.spine.Int()].Child = jointPoints[PositionIndex.neck.Int()];
        jointPoints[PositionIndex.neck.Int()].Child = jointPoints[PositionIndex.head.Int()];
    }

    /// <summary>
    /// Triangulate the normal vector given three points
    /// </summary>
    public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    /// <summary>
    /// Grabs the inverse quaternion rotation given two points and a forward vector
    /// </summary>
    public static Quaternion GetInverse(JointPoint p1, JointPoint p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position, forward));
    }

    #region Kalman Update (reducing noise)
    /// <summary>
    /// Clears Kalman Update-related parameters
    /// </summary>
    public static void ClearKalman(JointPoint measurement)
    {
        measurement.K = Vector3.zero;
        measurement.P = Vector3.zero;
        measurement.X = Vector3.zero;
    }

    /// <summary>
    /// Updates Kalman Update-related parameters
    /// </summary>
    private static void measurementUpdate(JointPoint measurement, float KalmanParamQ, float KalmanParamR)
    {
        measurement.K.x = (measurement.P.x + KalmanParamQ) / (measurement.P.x + KalmanParamQ + KalmanParamR);
        measurement.K.y = (measurement.P.y + KalmanParamQ) / (measurement.P.y + KalmanParamQ + KalmanParamR);
        measurement.K.z = (measurement.P.z + KalmanParamQ) / (measurement.P.z + KalmanParamQ + KalmanParamR);
        measurement.P.x = KalmanParamR * (measurement.P.x + KalmanParamQ) / (KalmanParamR + measurement.P.x + KalmanParamQ);
        measurement.P.y = KalmanParamR * (measurement.P.y + KalmanParamQ) / (KalmanParamR + measurement.P.y + KalmanParamQ);
        measurement.P.z = KalmanParamR * (measurement.P.z + KalmanParamQ) / (KalmanParamR + measurement.P.z + KalmanParamQ);
    }

    /// <summary>
    /// Updates positions based on relevate Kalman parameters
    /// </summary>
    public static void KalmanUpdate(JointPoint measurement, float KalmanParamQ, float KalmanParamR)
    {
        measurementUpdate(measurement, KalmanParamQ, KalmanParamR);
        measurement.Pos3D.x = measurement.X.x + (measurement.Now3D.x - measurement.X.x) * measurement.K.x;
        measurement.Pos3D.y = measurement.X.y + (measurement.Now3D.y - measurement.X.y) * measurement.K.y;
        measurement.Pos3D.z = measurement.X.z + (measurement.Now3D.z - measurement.X.z) * measurement.K.z;
        measurement.X = measurement.Pos3D;
    }
    #endregion
}
