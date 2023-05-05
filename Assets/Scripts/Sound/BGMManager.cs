/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;

/// <summary>
/// Global manager for all BGM in the scene
/// </summary>
public class BGMManager : CSingletonMono<BGMManager>
{
    [Header("BGM Clips")]
    public AudioClip tutorialBGM;
    public AudioClip normalBGM;

    [Header("BGM Settings")]
    [Tooltip("Target volume for tutorial BGM")]
    public float targetVolumeTutorial;

    [Tooltip("Target volume for normal BGM")]
    public float targetVolumeNormal;

    [Tooltip("Fade time for fading BGM in/out")]
    public float fadeMaxTime = 1.5f;

    private AudioSource source;
    private bool isFading;
    private float startVolume;
    private float targetVolume;
    private float fadeTimer;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        if (isFading)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, fadeTimer / fadeMaxTime);
            fadeTimer += Time.deltaTime;

            if (fadeTimer >= fadeMaxTime)
            {
                isFading = false;
                source.volume = targetVolume;
            }
        }
    }

    /// <summary>
    /// Plays BGM based on level type
    /// </summary>
    public void PlayBGM()
    {
        if (GameManager.instance.levelType == LevelType.Guided)
        {
            source.clip = tutorialBGM;
            targetVolume = targetVolumeTutorial;
        } 
        else
        {
            source.clip = normalBGM;
            targetVolume = targetVolumeNormal;
        }

        startVolume = source.volume;
        isFading = true;
        fadeTimer = 0;
        source.Play();
    }

    /// <summary>
    /// Pause BGM
    /// </summary>
    public void Pause()
    {
        source.Pause();
    }

    /// <summary>
    /// Resume BGM
    /// </summary>
    public void Resume()
    {
        source.UnPause();
    }

    /// <summary>
    /// Stops BGM
    /// </summary>
    public void StopBGM(bool fadeOut)
    {
        if (fadeOut)
        {
            startVolume = source.volume;
            targetVolume = 0;
            isFading = true;
            fadeTimer = 0;
        }
        else
        {
            source.Stop();
        }
    }
}
