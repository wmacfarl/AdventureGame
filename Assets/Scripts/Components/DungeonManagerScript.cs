using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DungeonManagerScript : MonoBehaviour
{
    public List<GameObject> DungeonFloorGameObjects;
    public GameObject PlayerGameObject;

    private void Awake()
    {
        DungeonFloorGameObjects = new List<GameObject>();
    }

}
