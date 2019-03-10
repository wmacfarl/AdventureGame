using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/*
 * This class is a simple GameObject interface to the DungeonGenerator.  This class exists to get parameter input from the inspector and pass it to a 
 * DungeonGenerator.
 * 
 * One of the more persistent confusions I have with Unity architecture is when to make a GameObject, when to make code a MonoBehaviour, etc, so I'm
 * not totally sold on this design.
 * 
 * I wanted to separate the actual Dungeon generation logic from Unity/MonoBehaviour specific things but wanted to be able to access paramenters
 * through the inspector so that's what this class is for.
 */

public class DungeonGenerationScript : MonoBehaviour
{
    [SerializeField]
    Vector2 SizeOfDungeonToGenerate;

    [SerializeField]
    float MinimumAreaToSplitRegion;    //If a DungeonRegions's area is less then this value it will stop splitting into SubRegions during the
                                        //generation process
    [SerializeField]
    float MinimumLengthToSplitRegion;  //If a DungeonRegions's x or y dimension is less then this value it will stop splitting into SubRegions 
                                        //during the generation process
    [SerializeField]
    float ChanceToStopSplittingRoom;    //Once the DungeonRegions have split a certain number of times, they have a percent chance to stop splitting
                                        //even if they are large enough that they could split.  This adds variety in room size
    [SerializeField]
    int MinimumDepthToStopSplitting;    //The number of Splits that need to occur before we check whether we should stop splitting

    [SerializeField]
    float MaximumAreaForDungeonRoom;    //DungeonRegion must be smaller than this value before we start checking whether to randomly stop splitting

    [SerializeField]
    float DungeonCorridorWidth;        

    [SerializeField]
    float MinimumPercentOfRegionForRoom;//After DungeonRegions are defined through splits, leaf regions contain rooms.  Rooms take up a subset of                                       
    [SerializeField]                    //the space in their region.  These variables set the minimum and maximum amount of the region to have the
    float MaximumPercentOfRegionForRoom;//room occupy.

    [SerializeField]                    //An amount to scale the Dungeon after it is generated.  This might in some cases be better than generating a 
    int DungeonScaleFactor;            //larger dungeon.

    
    [SerializeField]                    //These tiles are set in the inspector and determine what art we use to tile the room.  The wall tiles should
    public Tile RoomFloorTile;          //have colliders set in the Tile asset.  The floor tiles should not.
    [SerializeField]
    public Tile CorridorFloorTile;
    [SerializeField]
    public Tile EastWallTile;
    [SerializeField]
    public Tile WestWallTile;
    [SerializeField]
    public Tile NorthWallInnerTile;
    [SerializeField]
    public Tile NorthWallOuterTile;
    [SerializeField]
    public Tile SouthWallTile;
    [SerializeField]
    public Tile NorthEastWallTile;
    [SerializeField]
    public Tile SouthEastWallTile;
    [SerializeField]
    public Tile NorthWestWallTile;
    [SerializeField]
    public Tile SouthWestWallTile;
    [SerializeField]
    public Tile InteriorWallTile;

    [SerializeField]                    //These are prefabs for doors, assigned in the inspector.  These are GameObjects and not tiles.
    public GameObject NorthSouthDoorPrefab;
    [SerializeField]
    public GameObject EastWestDoorPrefab;

    [SerializeField]                    //These are prefabs for rooms and corridors which contain whatever scripts we need to handle these
    public GameObject RoomPrefab;       //kinds of regions in-game.
    [SerializeField]
    public GameObject CorridorPrefab;

    [SerializeField]
    int NumberOfFloors;

    List<DungeonFloor> DungeonFloors;

    public void GenerateFloors(DungeonGenerator generator, DungeonManagerScript dungeonManager)
    {
        List<GameObject> floors = new List<GameObject>();
        for (int i = 0; i < NumberOfFloors; i++)
        {
            DungeonFloor newFloor = generator.MakeDungeon();
            GameObject newFloorGameObject = new GameObject();
            newFloorGameObject.name = "DungeonFloor_" + i;
            DungeonFloorScript newFloorScript = newFloorGameObject.AddComponent<DungeonFloorScript>();

            newFloorScript.FloorNumber = i;
            newFloorScript.dungeonFloor = newFloor;

            GenerateTilemap(newFloorGameObject);
            FillTilemap(newFloorGameObject);
            MakeRooms(newFloorGameObject);
            MakeCorridors(newFloorGameObject);
            CheckCorridorRooms();
            if (i != 0)
            {
                newFloorGameObject.SetActive(false);
            }

            dungeonManager.DungeonFloorGameObjects.Add(newFloorGameObject);            
        }
    }


    public void PopulateFloors(DungeonGenerator generator, DungeonManagerScript dungeonManager )
    {

    }

    public void Start()
    {

        DungeonGenerator generator = new DungeonGenerator(SizeOfDungeonToGenerate, MinimumAreaToSplitRegion, MinimumLengthToSplitRegion,
            ChanceToStopSplittingRoom, MinimumDepthToStopSplitting, MaximumAreaForDungeonRoom, DungeonCorridorWidth, MinimumPercentOfRegionForRoom,
            MaximumPercentOfRegionForRoom, DungeonScaleFactor);

        GameObject dungeonManagerGameObject = new GameObject();
        DungeonManagerScript dungeonManager = dungeonManagerGameObject.AddComponent<DungeonManagerScript>();
        dungeonManagerGameObject.name = "Dungeon Manager";
    
        GenerateFloors(generator, dungeonManager);
        PopulateFloors(generator, dungeonManager);

        GameObject.Destroy(this);
    }

    public void FillTilemap(GameObject dungeonFloorGO)
    {
        Tilemap tilemap = dungeonFloorGO.GetComponent<DungeonFloorScript>().MyTilemap;
        DungeonFloor newFloor = dungeonFloorGO.GetComponent<DungeonFloorScript>().dungeonFloor;
        BoxFill(tilemap, InteriorWallTile, tilemap.WorldToCell(newFloor.RootRegion.Footprint.min * 1.2f), tilemap.WorldToCell(newFloor.RootRegion.Footprint.max * 1.2f));
    }

    public void TileRoom(Room room, Tilemap tilemap)
    {
        Vector3Int min = tilemap.WorldToCell(room.Footprint.min+Vector2.right + Vector2.up);
        Vector3Int max = tilemap.WorldToCell(room.Footprint.max-Vector2.right*2-Vector2.up);
        //Fill the interior with the floor
        BoxFill(tilemap, RoomFloorTile, min, max);
        Vector3Int NorthWestCorner = tilemap.WorldToCell(new Vector2(room.Footprint.min.x, room.Footprint.max.y-1));
        Vector3Int NorthEastCorner = tilemap.WorldToCell(room.Footprint.max-Vector2.right-Vector2.up);
        Vector3Int SouthWestCorner = tilemap.WorldToCell(room.Footprint.min);
        Vector3Int SouthEastCorner = tilemap.WorldToCell(new Vector2(room.Footprint.max.x-1, room.Footprint.min.y));

        BoxFill(tilemap, NorthWallOuterTile, NorthWestCorner+Vector3Int.up+Vector3Int.right, NorthEastCorner+Vector3Int.up-Vector3Int.right);

        BoxFill(tilemap, NorthWallInnerTile, NorthWestCorner, NorthEastCorner);
        BoxFill(tilemap, SouthWallTile, SouthWestCorner, SouthEastCorner);
        BoxFill(tilemap, EastWallTile, SouthEastCorner, NorthEastCorner);
        BoxFill(tilemap, WestWallTile, SouthWestCorner, NorthWestCorner);

        tilemap.SetTile(NorthEastCorner, NorthEastWallTile);
        tilemap.SetTile(NorthWestCorner, NorthWestWallTile);
        tilemap.SetTile(SouthEastCorner, SouthEastWallTile);
        tilemap.SetTile(SouthWestCorner, SouthWestWallTile);

    }

    

    public void MakeRooms(GameObject newFloorGO)
    {
        DungeonFloorScript dungeonFloorScript = newFloorGO.GetComponent<DungeonFloorScript>();
        Tilemap tilemap= dungeonFloorScript.MyTilemap;
        DungeonFloor dungeonFloor = dungeonFloorScript.dungeonFloor;
        foreach (Room room in dungeonFloor.Rooms)
        {
            TileRoom(room, tilemap);
            GameObject roomObject = GameObject.Instantiate(RoomPrefab);

            dungeonFloorScript.RoomGameObjects.Add(roomObject);
            roomObject.transform.position = room.Footprint.position;
            roomObject.GetComponent<RoomScript>().room = room;
            roomObject.transform.parent = newFloorGO.transform;

        }
    }

    public void MakeCorridors(GameObject newFloorGO)
    {
        DungeonFloorScript dungeonFloorScript = newFloorGO.GetComponent<DungeonFloorScript>();
        Tilemap tilemap = dungeonFloorScript.MyTilemap;
        DungeonFloor dungeonFloor = dungeonFloorScript.dungeonFloor;

        foreach (Corridor corridor in dungeonFloor.Corridors)
        {
            Vector3Int min = tilemap.WorldToCell(corridor.Footprint.min);
            Vector3Int max = tilemap.WorldToCell(corridor.Footprint.max-Vector2.right-Vector2.up);
            BoxFill(tilemap, CorridorFloorTile, min, max);
            GameObject corridorGameObject = GameObject.Instantiate(CorridorPrefab);
            dungeonFloorScript.CorridorGameObjects.Add(corridorGameObject);
            corridorGameObject.GetComponent<CorridorScript>().corridor = corridor;
            corridorGameObject.transform.position = corridor.Footprint.position;
            corridorGameObject.transform.parent = newFloorGO.transform;
            GameObject doorToInstantiate;
            GameObject newDoor1;
            GameObject newDoor2;
            if (corridor.Direction.x == 0)
            {
                doorToInstantiate = NorthSouthDoorPrefab;
                Vector2 southDoorPosition = new Vector2(corridor.Footprint.xMin, corridor.Footprint.yMin - 1);
                Vector2 northDoorPosition = new Vector2(corridor.Footprint.xMin, corridor.Footprint.yMax);
                newDoor1 = GameObject.Instantiate(doorToInstantiate, southDoorPosition, Quaternion.identity);
                newDoor2 = GameObject.Instantiate(doorToInstantiate, northDoorPosition, Quaternion.identity);
            }
            else
            {
                doorToInstantiate = EastWestDoorPrefab;
                Vector2 westDoorPosition = new Vector2(corridor.Footprint.xMin - 1, corridor.Footprint.yMin); ;
                Vector2 eastDoorPosition = new Vector2(corridor.Footprint.xMax, corridor.Footprint.yMin);
                newDoor1 = GameObject.Instantiate(doorToInstantiate, westDoorPosition, Quaternion.identity);
                newDoor2 = GameObject.Instantiate(doorToInstantiate, eastDoorPosition, Quaternion.identity);
            }

            dungeonFloorScript.DoorGameObjects.Add(newDoor1);
            dungeonFloorScript.DoorGameObjects.Add(newDoor2);

            newDoor1.transform.parent = corridorGameObject.transform;
            newDoor2.transform.parent = corridorGameObject.transform;

            newDoor1.GetComponent<DoorScript>().Corridor = corridorGameObject;
            newDoor2.GetComponent<DoorScript>().Corridor = corridorGameObject;

            newDoor1.GetComponent<DoorScript>().Room = corridorGameObject.GetComponent<CorridorScript>().Room1;
            newDoor2.GetComponent<DoorScript>().Room = corridorGameObject.GetComponent<CorridorScript>().Room2;

            foreach (RoomScript roomScript in Object.FindObjectsOfType<RoomScript>())
            {
                if (roomScript.room == corridor.ConnectedRooms[0] || roomScript.room == corridor.ConnectedRooms[1])
                {
                    roomScript.AttachedCorridors.Add(corridorGameObject);
                    if (roomScript.room == corridor.ConnectedRooms[0])
                    {
                        corridorGameObject.GetComponent<CorridorScript>().Room1 = roomScript.transform.gameObject;
                    }
                    if (roomScript.room == corridor.ConnectedRooms[1])
                    {
                        corridorGameObject.GetComponent<CorridorScript>().Room2 = roomScript.transform.gameObject;
                    }
                }
            }
        }
    }

    public void GenerateTilemap(GameObject floorGameObject)
    {
        GameObject tilemapGO = new GameObject();
        GameObject gridGO = new GameObject();

        tilemapGO.AddComponent<Tilemap>();
        tilemapGO.AddComponent<TilemapRenderer>();
        Grid grid = gridGO.AddComponent<Grid>();

        grid.transform.parent = floorGameObject.transform;
        gridGO.name = "Grid";
        tilemapGO.name = "Tilemap";
        tilemapGO.transform.parent = gridGO.transform;
        DungeonFloorScript newFloorScript = floorGameObject.GetComponent<DungeonFloorScript>();
        newFloorScript.MyGrid = gridGO;
        newFloorScript.MyTilemap = newFloorScript.MyGrid.GetComponentInChildren<Tilemap>();
    }


    public void BoxFill(Tilemap map, TileBase tile, Vector3Int start, Vector3Int end)
    { 
        //Determine directions on X and Y axis
        var xDir = start.x < end.x ? 1 : -1;
        var yDir = start.y < end.y ? 1 : -1;
        //How many tiles on each axis?
        int xCols = Mathf.Abs(start.x - end.x)+1;
        int yCols = Mathf.Abs(start.y - end.y)+1;
        //Start painting
        for (var x = 0; x < xCols; x++)
        {
            for (var y = 0; y < yCols; y++)
            {
                var tilePos = start + new Vector3Int(x * xDir, y * yDir, 0);
                map.SetTile(tilePos, tile);
            }
        }
    }

    //Small override, to allow for world position to be passed directly
    public void BoxFill(Tilemap map, TileBase tile, Vector3 start, Vector3 end)
    {
        BoxFill(map, tile, map.WorldToCell(start), map.WorldToCell(end));
    }

    public bool CheckCorridorRooms()
    {
        CorridorScript[] corridorScripts = Object.FindObjectsOfType<CorridorScript>();
        {
            foreach (CorridorScript corridorScript in corridorScripts)
            {
                Corridor corridor = corridorScript.corridor;
                if (corridor.ConnectedRooms[0] != corridorScript.Room1.GetComponent<RoomScript>().room)
                {
                    Debug.Log("rooms don't match");
                    return false;
                }
            }
        }
        return true;
    }
}







