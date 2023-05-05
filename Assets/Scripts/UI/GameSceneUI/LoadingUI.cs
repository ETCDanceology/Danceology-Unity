/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;
using UnityEngine.UI;
public class LoadingUI : MonoBehaviour
{
    public float min_loading_time = 10f;    // Minimum loading time before moving on
    public float max_loading_time = 20f;    // Maximum loading time before shutting off camera and moving on
    public Image loadingBar;

    float timer = 0f;
    bool start_load = false;
    bool is_finished_dataLoading = false;

    private void Start()
    {
        EventBus.AddListener(EventTypes.FinishedDataLoading, SetFinishedDataLoading);
        LoadPhaseInit();
    }

    /// <summary>
    /// General update loop
    /// </summary>
    public void Update()
    {
        if (!start_load) return;

        timer += Time.deltaTime;
        if (timer >= max_loading_time)
        {
            LoadingUIEnd();
        }
        else if ((timer >= min_loading_time) && (is_finished_dataLoading))
        {
            LoadingUIEnd();
        }
        else
        {
            LoadingArtUpdate();
        }
    }

    /// <summary>
    /// Update loading bar
    /// </summary>
    public void LoadingArtUpdate()
    {
        loadingBar.fillAmount = Mathf.Clamp(timer / min_loading_time, 0, 0.9f);
    }

    /// <summary>
    /// Initialize loading parameters on level start
    /// </summary>
    public void LoadPhaseInit()
    {
        timer = 0;
        start_load = true;
        is_finished_dataLoading = false;
        loadingBar.fillAmount = 0;
    }

    /// <summary>
    /// Call when all preparation data has finished loading
    /// </summary>
    public void SetFinishedDataLoading()
    {
        is_finished_dataLoading = true;
    }

    /// <summary>
    /// Called once loading has finished
    /// </summary>
    public void LoadingUIEnd()
    {
        loadingBar.fillAmount = Mathf.Lerp(loadingBar.fillAmount, 1, 0.1f);
        if (loadingBar.fillAmount > 0.99f)
        {
            UIUtils.instance.HideUI(gameObject);
            start_load = false;
            GameManager.instance.FinishedLoading(is_finished_dataLoading);
        }
    }
}
