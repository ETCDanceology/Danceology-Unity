/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Helper enum storing all the different types of SFX
/// </summary>
public enum SFX
{ 
    BadFeedback = 0,
    OKFeedback,
    GoodFeedback,
    GreatFeedback,
    ExcellentFeedback,
}

/// <summary>
/// Associated audio data for a given SFX
/// </summary>
[System.Serializable]
public class SFXData
{
    public SFX sfxName;
    public AudioClip sfxClip;

    [Range(0.1f, 1.0f)]
    public float defaultVolume = 1.0f;

    [Range(-3.0f, 3.0f)]
    public float defaultPitch = 1.0f;
}

/// <summary>
/// Global manager for all SFX in the game
/// </summary>
public class SFXManager : CSingletonMono<SFXManager>
{
    public SFXData[] sfxDatas;
    private Dictionary<SFX, AudioSource> sfxMapping;
    private Dictionary<string, AudioSource> clipMapping;        // When directly playing a clip, use this to map from clip to its own AudioSource
    private Dictionary<string, bool> ClipStopMapping;           // Mapping of whether a clip has been stopped
    private Dictionary<string, float> ClipStopTimeStep;         // Mapping for what timestep the clip has been stopped at

    private List<CaptionEntry> captions;                        // Current caption data list for the audio file
    private string curKey;                                      // Current on-going voice file name
    private int curLineIndex;                                   // Current index position in caption list
    public TextMeshProUGUI text;                                // Reference to asset that's used to display captions

    void Start()
    {
        sfxMapping = new Dictionary<SFX, AudioSource>();
        clipMapping = new Dictionary<string, AudioSource>();
        ClipStopMapping = new Dictionary<string, bool>();
        ClipStopTimeStep = new Dictionary<string, float>();

        foreach (SFXData data in sfxDatas)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            newSource.clip = data.sfxClip;
            newSource.volume = data.defaultVolume;
            newSource.pitch = data.defaultPitch;

            sfxMapping.Add(data.sfxName, newSource);
        }
    }

    /// <summary>
    /// General update function
    /// </summary>
    private void Update()
    {
        if (captions != null && clipMapping[curKey].isPlaying && curLineIndex < captions.Count && clipMapping[curKey].time > captions[curLineIndex].Time)
        {
            text.gameObject.SetActive(true);
            text.text = captions[curLineIndex++].Text;

            if (GameManager.instance.playerInputDevice != PlayerInputDevice.NoCamera && CaptionManager.instance.GetCurIndex() >= 4)
            {
                text.transform.localPosition = new Vector3(-250, -435, 0);
                text.margin = new Vector4(100, 0, 100, 0);
            }
        }
        else if (captions == null || !clipMapping[curKey].isPlaying)
        {
            text.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Pause all currently playing audio (include voice and sfx)
    /// </summary>
    public void PauseAll()
    {
        foreach (var k in clipMapping.Keys)
        {
            if (clipMapping[k].isPlaying)
            {
                float stop_time = clipMapping[k].time;
                clipMapping[k].Stop();
                if (ClipStopMapping.ContainsKey(k))
                {
                    ClipStopMapping[k] = true;
                    ClipStopTimeStep[k] = stop_time;
                }
                else
                {
                    ClipStopMapping.Add(k, true);
                    ClipStopTimeStep.Add(k, stop_time);
                }
            }
        }
    }


    /// <summary>
    /// Resume all currently paused audio
    /// </summary>
    public void ResumeAll()
    {
        List<string> new_replay_clip = new List<string>();
        foreach (KeyValuePair<string, bool> item in ClipStopMapping)
        {
            if (item.Value)
            {
                clipMapping[item.Key].time = ClipStopTimeStep[item.Key];
                clipMapping[item.Key].Play();
                new_replay_clip.Add(item.Key);
            }
        }

        for (int i = 0; i < new_replay_clip.Count; i++)
        {
            ClipStopMapping[new_replay_clip[i]] = false;
        }

        text.gameObject.SetActive(true);
    }

    /// <summary>
    /// Stop all audio
    /// </summary>
    public void StopAll()
    {
        foreach (SFX sfxName in sfxMapping.Keys)
        {
            sfxMapping[sfxName].Stop();
        }

        foreach (string clipName in clipMapping.Keys)
        {
            clipMapping[clipName].Stop();
        }
    }

    /// <summary>
    /// Play a given SFX given the name of the SFX type
    /// </summary>
    public void PlaySFX(SFX sfxName)
    {
        if (sfxMapping.ContainsKey(sfxName))
        { 
            sfxMapping[sfxName].Play();
        }
    }

    /// <summary>
    /// Play a sfx directly from audio clip
    /// </summary>
    public void PlaySFX(AudioClip clip, float defaultVolume = 1.0f, float defaultPitch = 1.0f)
    {
        string key = clip.name;
        if (clipMapping.ContainsKey(key))
        {
            clipMapping[key].clip = clip;
            clipMapping[key].volume = defaultVolume;
            clipMapping[key].pitch = defaultPitch;
        }
        else
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            newSource.clip = clip;
            newSource.volume = defaultVolume;
            newSource.pitch = defaultPitch;

            clipMapping.Add(key, newSource);
        }
        clipMapping[key].Play();

        curKey = key;
        captions = CaptionManager.instance.LoadNewCaption();
        curLineIndex = 0;
    }

    /// <summary>
    /// Returns whether this sfx is currently playing given sfx type
    /// </summary>
    public bool SFXisPlaying(SFX sfxName)
    {
        if (sfxMapping.ContainsKey(sfxName))
        {
            return sfxMapping[sfxName].isPlaying;
        }
        return false;
    }

    /// <summary>
    /// Returns whether this sfx is currently playing given audio clip name
    /// </summary>
    public bool SFXisPlaying(string key)
    {
        if (clipMapping.ContainsKey(key))
        {
            if (!clipMapping[key].isPlaying || clipMapping[key].time == 0)
            {
                // Audio is not playing or has already finished
                return false;
            }
            return clipMapping[key].time < clipMapping[key].clip.length;
        }

        return false;
    }

    /// <summary>
    /// Returns whether this sfx is currently playing given a reference to the audio clip
    /// </summary>
    public bool SFXisPlaying(AudioClip clip)
    {
        string key = clip.name;
        return SFXisPlaying(key);
    }
}
