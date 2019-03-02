using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This sprite handles the z-ordering of objects and characters based on their y-position.  Objects higher on the y-axis should be occluded
 * by objects lower on the y-axis to create an illusion of perspective.
 * 
 * Put this script on any GameObject that the player can stand adjacent to and below or that has a collider smaller than its sprite.
 * 
 * This assumes that the origin/pivot for the sprite is the same for all objects.  This should be bottom-center for other scripts in the project.
 */

[RequireComponent(typeof(SpriteRenderer))]
public class ZLayerOrderingScript : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spriteRenderer.isVisible)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -10);
        }
    }
}
