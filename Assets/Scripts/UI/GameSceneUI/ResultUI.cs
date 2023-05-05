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
using TMPro;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public static ResultUI SharedInstance;
    public TextMeshProUGUI hint;
    public Image round;
    public GameObject stars;
    public GameObject[] emptyStars;
    public GameObject[] fillStars;
    public Transform scoreFolder;

    private int[] scoreDistributionCount; // 0-excellent -> 5-bad

    private Vector3 targetPos = Vector3.zero;
    private Vector3 startPos = new Vector3(-1500, 0, 0);
    private const float SHOW_SCORE_TIME = 3;

    private void Awake()
    {
        SharedInstance = this;
        transform.localPosition = startPos;
        stars.SetActive(true);
        hint.gameObject.SetActive(false);
        round.gameObject.SetActive(false);
        scoreDistributionCount = new int[6];
    }

    /// <summary>
    /// Adds counter to score distribution based on given index
    /// </summary>
    public void AddToScoreDistribution(int index)
    {
        scoreDistributionCount[index] += 1;
    }

    /// <summary>
    /// Resets all score distribution
    /// </summary>
    private void ResetScoreDistribution()
    {
        scoreDistributionCount = new int[6];
    }

    /// <summary>
    /// Slide in the results screen UI
    /// </summary>
    public void FadeIn()
    {
        StartCoroutine(Move());
    }

    /// <summary>
    /// Given an index, sets the scoring (number) along with stars UI
    /// </summary>
    public void SetScore(int score)
    {
        for (int i = 0; i< fillStars.Length; i++)
        {
            fillStars[i].SetActive(false);
            emptyStars[i].SetActive(true);
        }

        if (GameManager.instance.playerInputDevice == PlayerInputDevice.NoCamera)
        {
            scoreFolder.gameObject.SetActive(false);
        }
        else
        {
            scoreFolder.gameObject.SetActive(true);
            for (int i = 0; i < scoreDistributionCount.Length; i++)
            {
                scoreFolder.Find("Score" + i).GetComponent<TextMeshProUGUI>().text = "x " + scoreDistributionCount[i];
            }
        }
        
        scoreText.text = score.ToString();
        if (score > 60)
        {
            fillStars[0].SetActive(true);
            emptyStars[0].SetActive(false);
        }
        if (score > 70)
        {
            fillStars[1].SetActive(true);
            emptyStars[1].SetActive(false);
        }
        if (score > 85)
        {
            fillStars[2].SetActive(true);
            emptyStars[2].SetActive(false);
        }
    }

    /// <summary>
    /// Sliding in animation main logic
    /// </summary>
    private IEnumerator Move()
    {
        float e = 0;
        Vector3 orig = transform.localPosition;
        while (e < 1)
        {
            transform.localPosition = Vector3.Lerp(orig, targetPos, e);
            e += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.localPosition = targetPos;

        e = SHOW_SCORE_TIME;
        hint.gameObject.SetActive(true);
        round.gameObject.SetActive(true);

        while (e > 0)
        {
            hint.text = Mathf.CeilToInt(e).ToString();
            round.fillAmount -= Time.deltaTime / SHOW_SCORE_TIME;
            e -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.localPosition = startPos;
        hint.gameObject.SetActive(false);
        round.gameObject.SetActive(false);
        FindObjectOfType<ToNextLevel>().ShowUp();
    }

    /// <summary>
    /// Called when finished displaying results
    /// </summary>
    public void End()
    {
        StopCoroutine(Move());
        ResetScoreDistribution();
        transform.localPosition = startPos;
        hint.gameObject.SetActive(false);
        round.gameObject.SetActive(false);
    }
}
