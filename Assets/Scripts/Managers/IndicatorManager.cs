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
/// Manager class for indicator arrows, resposible for converting indicator data into actual GameObjects to spawn within the scene
/// </summary>
public class IndicatorManager: CSingletonMono<IndicatorManager>
{
    [Tooltip("Prefab to use for arrows denoting application of force")]
    public GameObject indicatorLinePrefab_Force;

    [Tooltip("Prefab to use for arrows denoting dancer movement")]
    public GameObject indicatorLinePrefab_Movement;

    private AnimModelMovement model;    // Reference to AnimModelMovement instance in the current scene

    void Start()
    {
        model = FindObjectOfType<AnimModelMovement>();
    }

    /// <summary>
    /// Util function to grab relative direction with reference to the main camera
    /// </summary>
    private Vector3 GetCameraDirVector(RelativeDirection dir)
    {
        switch (dir)
        {
            case RelativeDirection.Right:
                return Camera.main.transform.right;
            case RelativeDirection.Left:
                return -Camera.main.transform.right;
            case RelativeDirection.Up:
                return Camera.main.transform.up;
            case RelativeDirection.Down:
                return -Camera.main.transform.up;
            case RelativeDirection.Back:
                return Camera.main.transform.forward;
            case RelativeDirection.Front:
                return -Camera.main.transform.forward;
            default:
                return -Camera.main.transform.forward;
        }
    }

    /// <summary>
    /// Grabs Vector3 coordinates of positions based on each of the IndicatorPoints within the line
    /// </summary>
    private Vector3[] GetPositions(IndicatorLine indicatorLine)
    {
        IndicatorPoint[] points = indicatorLine.indicatorPoints;

        // Converts indicator points to absolute world positions
        Vector3[] trailPositions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            IndicatorPoint point = points[i];

            Vector3? possibleJointPos = model.GetJointPosition(point.positionIndex);
            Vector3 jointPos;
            if (possibleJointPos == null)
            {
                // No position found for this joint; sets to zero vector
                jointPos = Vector3.zero;
            }
            else
            {
                jointPos = possibleJointPos.Value;
            }

            Vector3 dirVector = GetCameraDirVector(point.relativeDirection);
            jointPos += dirVector * point.relativeDistance;

            trailPositions[i] = jointPos;
        }

        return trailPositions;
    }

    /// <summary>
    /// Spawns and plays a given indicator line, instantiates prefab based on given arrow type
    /// </summary>
    public void PlayIndicator(IndicatorLine indicatorLine, ArrowType arrowType)
    {
        // Compute positions based on points in the indicator line
        Vector3[] trailPositions = GetPositions(indicatorLine);

        GameObject indicatorGameObj;
        if (arrowType == ArrowType.Force)
        {
            indicatorGameObj = Instantiate(indicatorLinePrefab_Force);
        }
        else
        {
            indicatorGameObj = Instantiate(indicatorLinePrefab_Movement);
        }

        IndicatorLineObject lineObj = indicatorGameObj.GetComponent<IndicatorLineObject>();
        lineObj.StartMoving(trailPositions, indicatorLine.trailSpeed);
    }
}
