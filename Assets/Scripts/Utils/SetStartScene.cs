/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Util class for an editor command that helps with setting the start scene when Play Mode is started
/// </summary>
public class SetStartScene : MonoBehaviour
{
    [MenuItem("Util/Set Starting Scene")]
    static void SetStartingScene()
    {
        var scenePath = EditorBuildSettings.scenes[0].path;
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        EditorSceneManager.playModeStartScene = sceneAsset;
        Debug.Log(scenePath + " was set as default play mode scene");
    }

    [MenuItem("Util/Unset Starting Scene")]
    static void UnsetStartingScene()
    {
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("Removed default play mode scene");
    }
}
#endif