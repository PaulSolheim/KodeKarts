﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int lap = 0;
    public int checkPoint = -1;
    public float timeEntered = 0;
    int checkPointCount;
    int nextCheckPoint;
    public GameObject lastCP;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] cps = GameObject.FindGameObjectsWithTag("checkpoints");
        checkPointCount = cps.Length;
        foreach (GameObject c in cps)
        {
            if (c.name == "0")
            {
                lastCP = c;
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "checkpoints")
        {
            int thisCPNumber = int.Parse(col.gameObject.name);
            if (thisCPNumber == nextCheckPoint)
            {
                lastCP = col.gameObject;
                checkPoint = thisCPNumber;
                timeEntered = Time.time;
                if (checkPoint == 0) lap++;

                nextCheckPoint++;
                if (nextCheckPoint >= checkPointCount)
                    nextCheckPoint = 0;
            }
        }
    }
}
