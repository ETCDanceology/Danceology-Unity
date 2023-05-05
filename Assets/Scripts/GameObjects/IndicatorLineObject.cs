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

/// <summary>
/// Relative directions to use when initializing an indicator point
/// </summary>
public enum RelativeDirection { Front, Back, Left, Right, Up, Down }

/// <summary>
/// Class containing data related to a single point in the indicator line
/// </summary>
[System.Serializable]
public class IndicatorPoint
{
    public PositionIndex positionIndex;                 // Joint position index to use as a relative position for this point
    public RelativeDirection relativeDirection;         // Direction from the joint position to place this point. The left/right is relative to the camera's view; all others are relative to the model
    public float relativeDistance;                      // Distance from the joint position in the direction of the relative position to place this indicator point
    
    /// <summary>
    /// Constructor for a copy of the given indicator point
    /// </summary>
    public IndicatorPoint(IndicatorPoint ip)
    {
        positionIndex = ip.positionIndex;
        relativeDirection = ip.relativeDirection;
        relativeDistance = ip.relativeDistance;
    }
}

/// <summary>
/// Class containing data related to a single indicator line
/// </summary>
[System.Serializable]
public class IndicatorLine
{
    public IndicatorPoint[] indicatorPoints;        // All indicator points to use along this line
    public float trailSpeed;                        // Speed to move this trail

    /// <summary>
    /// Constructor for a copy of the given indicator line
    /// </summary>
    public IndicatorLine(IndicatorLine il)
    {
        indicatorPoints = new IndicatorPoint[il.indicatorPoints.Length];
        for(int i = 0; i < indicatorPoints.Length; i++)
        {
            indicatorPoints[i] = new IndicatorPoint(il.indicatorPoints[i]);
        }
        trailSpeed = il.trailSpeed;
    }
}

/// <summary>
/// Monobehaviour class for a given IndicatorLineObject
/// </summary>
public class IndicatorLineObject : MonoBehaviour
{
    private Vector3[] trailPositions;           // List of points to hit with this line
    private bool isMoving;                      // Flag on whether this line is moving or not
    private int currentPositionIndex;           // Index of the current trail position we're going towards
    private float indicatorSpeed;               // Speed of the trail

    private TrailRenderer trail;                // Reference to the trail renderer component on this object

    /// <summary>
    /// Initializer for this object providing the object's target trail positions along with the speed of the indicator
    public void StartMoving(Vector3[] trailPositions, float indicatorSpeed)
    {
        this.trailPositions = trailPositions;
        this.indicatorSpeed = indicatorSpeed;

        trail = GetComponent<TrailRenderer>();

        if (trailPositions.Length == 0)
        {
            FinishPath();
            return;
        }

        trail.emitting = false;
        transform.position = trailPositions[0];

        currentPositionIndex = 0;
        isMoving = true;
        trail.emitting = true;
    }

    /// <summary>
    /// Called at the end of the indicator's path
    /// </summary>
    public void FinishPath()
    {
        isMoving = false;
        StartCoroutine(DestroyArrow());
    }

    /// <summary>
    /// Delayed destruction of the indicator that fades out the arrow
    /// </summary>
    private IEnumerator DestroyArrow()
    {
        float maxTime = GetComponent<TrailRenderer>().time;
        float fadeTime = 0;

        while (fadeTime < maxTime)
        {
            MeshRenderer arrowRenderer = GetComponentInChildren<MeshRenderer>();
            arrowRenderer.material.SetFloat("_Tweak_transparency", -(fadeTime / maxTime));
            Color trailMatColor = trail.material.color;
            trail.material.color = new Color(trailMatColor.r, trailMatColor.g, trailMatColor.b, 1.0f - (fadeTime / maxTime));

            yield return new WaitForEndOfFrame();
            fadeTime += 2 * Time.deltaTime;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        if (!isMoving) return;
        if (currentPositionIndex == trailPositions.Length - 1)
        {
            FinishPath();
            return;
        }

        // Updates indicator's rotation and position based on its current position and the next target position
        int nextPositionIndex = currentPositionIndex + 1;
        Vector3 currentPos = transform.position;
        Vector3 nextPos = trailPositions[nextPositionIndex];

        Vector3 dir = (nextPos - currentPos).normalized;
        float distToDest = Vector3.Distance(nextPos, transform.position);
        float distToMove = Mathf.Min(indicatorSpeed * Time.deltaTime, distToDest);

        // Smooths rotation to point to the target
        Quaternion rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((nextPos - currentPos), Vector3.up), 0.1f);
        transform.rotation = rotation;

        // Smooths move direction to point to the target
        Vector3 moveDir = Vector3.Lerp(transform.forward, dir, 0.75f);
        transform.position += moveDir * distToMove;

        if (distToMove == distToDest)
        {
            // Reached current target point; increment to next
            currentPositionIndex++;
        }
    }
}
