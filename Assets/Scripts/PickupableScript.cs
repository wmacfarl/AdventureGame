using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This script should be on all objects that can be picked-up or put down.
 * 
 * In order to draw the picked-up object on top of the player who is carrying it, this script requires that your project have
 * two sorting layers named:
 *   "Foreground" and
 *   "Background"
 *   
 * Objects that can be interacted with should be set to the "Objects" physics layer.
 */

[RequireComponent(typeof(BoxCollider2D))]

public class PickupableScript : MonoBehaviour
{
    //Required Component
    new BoxCollider2D collider;
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    //Called by the object triggering the pick-up.  A picked-up object has its collider disabled and is just a child of the carrier.
    public void GetPickedUp(GameObject pickerUpperGameObject, Vector2 carryOffset)
    {
        transform.parent = pickerUpperGameObject.transform;
        collider.enabled = false;
        transform.localPosition = carryOffset;
    }

    //Called by the object triggering the pick-up.  A picked-up object has its collider disabled and is just a child of the carrier.
    public void GetPutDown(Vector2 placeDistance)
    {
        transform.localPosition = placeDistance;
        transform.parent = null;
        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        collider.enabled = true;
    }

    //Returns true if there is space for my collider placeDistance away from the origin of the object I am currently parented to.  
    public bool CanIBePutDown(Vector2 placeDistance)
    {
        Vector2 carryPosition = transform.localPosition;
        transform.localPosition = placeDistance;
        collider.enabled = true;
        Collider2D[] overlappingColliders = Physics2D.OverlapAreaAll(collider.bounds.min, collider.bounds.max);
        collider.enabled = false;

        foreach (Collider2D overlappingCollider in overlappingColliders)
        {
            if (overlappingCollider != collider)
            {
                transform.localPosition = carryPosition;
                return false;
            }
        }
        transform.localPosition = carryPosition;
        return true;
    }
}
