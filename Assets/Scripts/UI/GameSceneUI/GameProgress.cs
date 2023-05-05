/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine.UI;

public class GameProgress : CSingletonMono<GameProgress>
{
    public int totalActionCount;
    public int MaxPosInd;
    public Image bar;

    public void SetTotalAction(int totalAction)
    {
        totalActionCount = totalAction;
    }

    public void UpdateProgress(int PoseInd)
    {
        if (PoseInd > MaxPosInd)
        {
            MaxPosInd = PoseInd;
            bar.fillAmount = PoseInd / (float)totalActionCount;
        }
    }

    public void ResetProgress()
    {
        MaxPosInd = 0;
        bar.fillAmount = 0;
    }
}
