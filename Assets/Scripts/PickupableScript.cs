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

    public void GetPutDown()
    {
        transform.parent = null;
        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
    }
}
