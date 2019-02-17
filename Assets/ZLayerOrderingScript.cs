using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
