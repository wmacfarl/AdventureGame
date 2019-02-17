using UnityEngine;
using System.Collections;

/* This script causes the object that it is attached to to follow the "target" GameObject.  
 * If you don't manually set the "target" in the inspector, it looks for an object named "Player"
 * 
 * targetOffset" can be set to have the camera follow a point offset from the target gameObject.  This is useful in the event that you
 * UI elements that take up some of your screen so you don't want the player right in the center.
 * 
 * Set dampTime in the inspector to set how long it takes for the camera to reach it's target.
 */

public class CameraFollowScript : MonoBehaviour
{
    public Vector3 targetOffset;
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
            Vector3 delta = target.position+targetOffset -GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
        else
        {
            target = GameObject.Find("Player").transform;
        }

    }
}