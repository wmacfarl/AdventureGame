using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonFloorScript : MonoBehaviour
{

    public GameObject MyGrid;
    public Tilemap MyTilemap;
    public List<GameObject> RoomGameObjects;
    public List<GameObject> CorridorGameObjects;
    public List<GameObject> DoorGameObjects;
    public DungeonFloor dungeonFloor;
    public int FloorNumber;

    // Start is called before the first frame update
    void Awake()
    {
        DoorGameObjects = new List<GameObject>();
        RoomGameObjects = new List<GameObject>();
        CorridorGameObjects = new List<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
