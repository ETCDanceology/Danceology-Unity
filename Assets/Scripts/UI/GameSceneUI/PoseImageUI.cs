/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PoseImageUI : MonoBehaviour
{
    public Image poseImage;
    public Image poseBackGround;
    public Sprite highlightBg;
    public float fadeTime = 0.75f;

    private GameObject target;
    private bool startFading;
    private float fadeTimer;

    /// <summary>
    /// Initializes a pose image UI based on a given key pose data
    /// </summary>
    public void Initialize(KeyPoseData poseData, GameObject target)
    {
        poseImage.sprite = poseData.poseSprite;
        if (poseData.isReversed)
        {
            transform.Rotate(Vector3.up, 180);
        }

        this.target = target;
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        if (startFading)
        {
            Color imageColor = poseImage.color;
            imageColor.a = Mathf.Lerp(0, 1, 1 - (fadeTimer / fadeTime));
            poseImage.color = imageColor;

            fadeTimer += Time.deltaTime;
            if (fadeTimer >= fadeTime)
            {
                Destroy(gameObject);
            }
        }
        else if (transform.position.x <= target.transform.position.x)
        {
            poseBackGround.sprite = highlightBg;
            transform.SetParent(transform.parent.parent);
            transform.position = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);
            startFading = true;
            fadeTimer = 0;
            transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), fadeTime);

            NextPoseUI npui = GetComponentInParent<NextPoseUI>();
            npui.StartShrink();
            MovementCompare.instance.EnableCompare();
        }
    }
}
