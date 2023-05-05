/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Util class for an editor window that helps with pipeline from Maya 3D animation -> Unity GameObject animation
/// </summary>
public class ProcessAnimationWindow : EditorWindow
{
    private AnimationClip inputAnim;
    private AnimationClip inputFlippedAnim;

    private string textToReplace;
    private string replacementText;
    private string jointHeader;

    private string[] flipJointNames = new string[] { "Hips", "Spine", "Spine1", "Spine2", "Neck", "Head" };
    private string[] swapJointNames = new string[] { 
        "LeftUpLeg", "RightUpLeg", "LeftLeg", "RightLeg", "LeftFoot", "RightFoot", "LeftToeBase", "RightToeBase", "LeftToe_End", "RightToe_End", "LeftShoulder", "RightShoulder",
        "LeftArm", "RightArm", "LeftForeArm", "RightForeArm", "LeftHand", "RightHand", "LeftHandIndex1", "RightHandIndex1", "LeftHandIndex2", "RightHandIndex2",
        "LeftHandIndex3", "RightHandIndex3", "LeftHandIndex4", "RightHandIndex4"
    };

    [MenuItem("Window/Process Animation")]
    static void Init()
    {
        GetWindow(typeof(ProcessAnimationWindow));
    }

    public void OnGUI()
    {
        inputAnim = EditorGUILayout.ObjectField("Clip", inputAnim, typeof(AnimationClip), false) as AnimationClip;
        inputFlippedAnim = EditorGUILayout.ObjectField("Flipped Clip", inputFlippedAnim, typeof(AnimationClip), false) as AnimationClip;

        if (inputAnim != null)
        {
            EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(inputAnim);

            textToReplace = EditorGUILayout.TextField("Text To Replace", textToReplace);
            replacementText = EditorGUILayout.TextField("Replacement Text", replacementText);
            if (GUILayout.Button("Rename Animation"))
            {
                RenameAnimation();
            }

            jointHeader = EditorGUILayout.TextField("Joint Header", jointHeader);
            if (GUILayout.Button("Generate Flipped Animation"))
            {
                GenerateFlippedAnimation();
            }

            if (inputFlippedAnim != null && GUILayout.Button("Combine Animations"))
            {
                AppendAnimations();
            }
        }
    }

    void RenameAnimation()
    {
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(inputAnim);
        Dictionary<string, AnimationCurve> curvesByName = new Dictionary<string, AnimationCurve>();

        foreach (EditorCurveBinding binding in curves)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(inputAnim, binding);
            float[] curveValues = curve.keys.Select(key => key.value).ToArray();
            if (Mathf.Max(curveValues) - Mathf.Min(curveValues) == 0) 
            {
                continue; // Skip stagnant curves
            }

            string[] pathArr = binding.path.Split("/");
            curvesByName.Add(pathArr[pathArr.Length - 1] + "/" + binding.propertyName, curve);
        }

        // Save to clip
        AnimationClip newClip = new AnimationClip();
        foreach (EditorCurveBinding binding in curves)
        {
            string[] pathArr = binding.path.Split("/");
            string lookupKey = pathArr[pathArr.Length - 1] + "/" + binding.propertyName;

            AnimationCurve curve = curvesByName.GetValueOrDefault(lookupKey, null);
            if (curve == null) continue;

            string newPath = binding.path;
            if (textToReplace.Length > 0)
            { 
                newPath = newPath.Replace(textToReplace, replacementText); // "BackExercise_AnimatedHalf_Test:", "");
            }
            newClip.SetCurve(newPath, typeof(Transform), binding.propertyName, curve);
        }

        AssetDatabase.CreateAsset(newClip, "Assets/Animation/back_exercise_renamed.anim");
    }


    void GenerateFlippedAnimation()
    {
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(inputAnim);
        Dictionary<string, AnimationCurve> curvesByName = new Dictionary<string, AnimationCurve>();

        foreach (EditorCurveBinding binding in curves)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(inputAnim, binding);
            string[] pathArr = binding.path.Split("/");
            curvesByName.Add(pathArr[pathArr.Length - 1] + "/" + binding.propertyName, curve);
            Debug.Log(pathArr[pathArr.Length - 1] + "/" + binding.propertyName);
        }

        // Flip single joints
        string[] eulerRotations = new string[] { "localEulerAnglesRaw.x", "localEulerAnglesRaw.y", "localEulerAnglesRaw.z", "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w", "m_LocalPosition.x" };
        int[] eulerMulFactors = new int[] { 1, -1, -1, 1, -1, -1, 1, -1 };

        List<Keyframe> newKeys = new List<Keyframe>();
        foreach (string jointName in flipJointNames)
        {
            for (int j = 0; j < eulerRotations.Length; j++)
            {
                string rot = eulerRotations[j];
                int mulFactor = eulerMulFactors[j];

                string lookupKey = jointHeader + jointName + "/" + rot;
                AnimationCurve curve = curvesByName.GetValueOrDefault(lookupKey, null);
                if (curve == null) continue;

                newKeys.Clear();
                foreach (Keyframe key in curve.keys)
                {
                    newKeys.Add(new Keyframe(key.time, mulFactor * key.value));
                }

                curvesByName[lookupKey] = new AnimationCurve(newKeys.ToArray());
            }
        }

        // Flip double joints
        for (int i = 0; i < swapJointNames.Length; i += 2)
        {
            string joint1Name = swapJointNames[i];
            string joint2Name = swapJointNames[i+1];

            for (int j = 0; j < eulerRotations.Length; j++)
            {
                string rot = eulerRotations[j];
                int mulFactor = eulerMulFactors[j];

                string lookupKey1 = jointHeader + joint1Name + "/" + rot;
                AnimationCurve curve1 = curvesByName.GetValueOrDefault(lookupKey1, null);

                string lookupKey2 = jointHeader + joint2Name + "/" + rot;
                AnimationCurve curve2 = curvesByName.GetValueOrDefault(lookupKey2, null);

                if (curve1 == null || curve2 == null) continue;

                List<Keyframe> newKeys2 = new List<Keyframe>();
                foreach (Keyframe key in curve1.keys)
                {
                    newKeys2.Add(new Keyframe(key.time, mulFactor * key.value));
                }

                List<Keyframe> newKeys1 = new List<Keyframe>();
                foreach (Keyframe key in curve2.keys)
                {
                    newKeys1.Add(new Keyframe(key.time, mulFactor * key.value));
                }

                curvesByName[lookupKey1] = new AnimationCurve(newKeys1.ToArray());
                curvesByName[lookupKey2] = new AnimationCurve(newKeys2.ToArray());
            }
        }

        // Save to clip
        AnimationClip newClip = new AnimationClip();
        foreach (EditorCurveBinding binding in curves)
        {
            string[] pathArr = binding.path.Split("/");
            string lookupKey = pathArr[pathArr.Length - 1] + "/" + binding.propertyName;
            AnimationCurve curve = curvesByName[lookupKey];
            newClip.SetCurve(binding.path, typeof(Transform), binding.propertyName, curve);
        }

        AssetDatabase.CreateAsset(newClip, "Assets/Animation/back_exercise_flip.anim");
    }

    void AppendAnimations()
    {
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(inputAnim);
        Dictionary<string, AnimationCurve> curvesByName = new Dictionary<string, AnimationCurve>();

        foreach (EditorCurveBinding binding in curves)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(inputAnim, binding);
            string[] pathArr = binding.path.Split("/");
            curvesByName.Add(pathArr[pathArr.Length - 1] + "/" + binding.propertyName, curve);
        }

        // Flip single joints
        float offsetTime = 0.1f;
        EditorCurveBinding[] flippedCurves = AnimationUtility.GetCurveBindings(inputFlippedAnim);
        foreach (EditorCurveBinding binding in flippedCurves)
        {
            AnimationCurve flippedCurve = AnimationUtility.GetEditorCurve(inputFlippedAnim, binding);
            string[] pathArr = binding.path.Split("/");
            string lookupKey = pathArr[pathArr.Length - 1] + "/" + binding.propertyName;

            AnimationCurve originalCurve = curvesByName[lookupKey];
            float startFlippedOffset = originalCurve[originalCurve.length - 1].time + offsetTime;
            for (int i = 0; i < flippedCurve.length; i++)
            {
                originalCurve.AddKey(startFlippedOffset + flippedCurve[i].time, flippedCurve[i].value);
            }

            curvesByName[lookupKey] = originalCurve;
        }

        // Save to clip
        AnimationClip newClip = new AnimationClip();
        foreach (EditorCurveBinding binding in curves)
        {
            string[] pathArr = binding.path.Split("/");
            string lookupKey = pathArr[pathArr.Length - 1] + "/" + binding.propertyName;
            AnimationCurve curve = curvesByName[lookupKey];
            newClip.SetCurve(binding.path, typeof(Transform), binding.propertyName, curve);
        }

        AssetDatabase.CreateAsset(newClip, "Assets/Animation/back_exercise_combined.anim");
    }
}
#endif
