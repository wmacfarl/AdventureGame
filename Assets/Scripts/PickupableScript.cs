using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableScript : MonoBehaviour
{
    public Vector2 handlePosition;
    new BoxCollider2D collider;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetPickedUp(GameObject pickerUpperGameObject, Vector2 carryOffset)
    {
        transform.parent = pickerUpperGameObject.transform;
        collider.enabled = false;
        transform.localPosition = handlePosition+carryOffset;
        GetComponent<SpriteRenderer>().sortingLayerName = "Foreground";
    }

    public void GetPutDown(Vector2 placeDistance)
    {
        transform.localPosition = placeDistance;
        transform.parent = null;
        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        collider.enabled = true;
    }

    public bool CanIBePutDown(Vector2 placeDistance)
    {
        Debug.Log("calling canIbeputdown");
        Vector2 carryPosition = transform.localPosition;
        Debug.Log("placedistance = " + placeDistance);
        transform.localPosition = placeDistance;
        collider.enabled = true;
        Collider2D[] overlappingColliders = Physics2D.OverlapAreaAll(collider.bounds.min, collider.bounds.max);
        Debug.Log("min = " + collider.bounds.min + ",  max = " + collider.bounds.max);
        Debug.DrawLine(collider.bounds.min, collider.bounds.max, Color.green, 1f);
        collider.enabled = false;

        foreach (Collider2D overlappingCollider in overlappingColliders)
        {
            Debug.Log("Overlapping Collider = " + overlappingCollider.transform.name);
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
