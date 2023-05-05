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

/// <summary>
/// Joint-based indices for where wristbands go
/// </summary>
public enum WristBand { LeftWristBand = 7, RightWristBand = 4, LeftAnkleBand = 13, RightAnkleBand = 10};

/// <summary>
/// General class responsible for displaying joint points and wrist/ankle bands on the webcam feed
/// </summary>
public class ObjectPool : CSingletonMono<ObjectPool>
{
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public GameObject leftWristBand;
    public GameObject rightWristBand;
    public GameObject leftAnkleBand;
    public GameObject rightAnkleBand;
    public Transform poolFolder;
    public int amountToPool;

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            tmp.transform.SetParent(poolFolder);
            pooledObjects.Add(tmp);
        }
    }

    public GameObject GetNextPooledObject()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }

    public void SetSpecifiedObj(int index, Vector3 pos)
    {
        if (index > pooledObjects.Count || index < 0)
        {
            return;
        }
        pooledObjects[index].SetActive(true);
        pooledObjects[index].transform.position = pos;
    }

    public void SetBandPos(WristBand band, Vector3 pos, Quaternion quaternion)
    {
        switch (band)
        { 
            case WristBand.LeftWristBand:
                SetLeftBandPos(pos, quaternion);
                break;
            case WristBand.RightWristBand:
                SetRightBandPos(pos, quaternion);
                break;
            case WristBand.LeftAnkleBand:
                SetLeftABandPos(pos, quaternion);
                break;
            case WristBand.RightAnkleBand:
                SetRightABandPos(pos, quaternion);
                break;
            default: break;
        }
    }

    public void SetLeftBandPos(Vector3 pos, Quaternion quaternion)
    {
        leftWristBand.SetActive(true);
        leftWristBand.transform.position = pos;
        leftWristBand.transform.rotation = quaternion;
    }

    public void SetRightBandPos(Vector3 pos, Quaternion quaternion)
    {
        rightWristBand.SetActive(true);
        rightWristBand.transform.position = pos;
        rightWristBand.transform.rotation = quaternion;
    }
    public void SetLeftABandPos(Vector3 pos, Quaternion quaternion)
    {
        leftAnkleBand.SetActive(true);
        leftAnkleBand.transform.position = pos;
        leftAnkleBand.transform.rotation = quaternion;
    }

    public void SetRightABandPos(Vector3 pos, Quaternion quaternion)
    {
        rightAnkleBand.SetActive(true);
        rightAnkleBand.transform.position = pos;
        rightAnkleBand.transform.rotation = quaternion;
    }

    public void DisableAll()
    {
        leftWristBand.SetActive(false);
        rightWristBand.SetActive(false);
        leftAnkleBand.SetActive(false);
        rightAnkleBand.SetActive(false);
        foreach (GameObject obj in pooledObjects)
        {
            obj.SetActive(false);
        }
    }
}
