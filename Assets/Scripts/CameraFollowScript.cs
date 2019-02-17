﻿using UnityEngine;
using System.Collections;

/* This script causes the object that it is attached to to follow the "target" GameObject.  
 * If you don't manually set the "target" in the inspector, it looks for an object named "Player"
 * 
 * Set dampTime in the inspector to set how long it takes for the camera to reach it's target.
 */

public class CameraFollowScript : MonoBehaviour
{

    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    private void Start()
    {
        if (target == null)
        {
            target = GameObject.Find("Player").transform;
        }
    }

    void Update()
    {
        if (target)
        {
            Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
            Vector3 delta = target.position -GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
        else
        {
            target = GameObject.Find("Player").transform;
        }

    }
}