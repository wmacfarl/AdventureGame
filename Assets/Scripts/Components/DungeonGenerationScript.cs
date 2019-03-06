using UnityEngine;
using UnityEngine.Tilemaps;

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

    public Tile RoomTile;
    public Tile CorridorTile;
    public Tile WallTile;

    Tilemap tilemap;
    Dungeon MyDungeon;

    public void Start()
    {
        DungeonGenerator generator = new DungeonGenerator(SizeOfDungeonToGenerate, MinimumAreaToSplitRegion, MinimumLengthToSplitRegion,
            ChanceToStopSplittingRoom, MinimumDepthToStopSplitting, MaximumAreaForDungeonRoom, DungeonCorridorWidth, MinimumPercentOfRegionForRoom,
            MaximumPercentOfRegionForRoom, DungeonScaleFactor);
        MyDungeon = generator.MakeDungeon();
        GenerateTilemap();
        TileRooms();
        TileCorridors();
        tilemap.RefreshAllTiles();
    }

    public void TileRooms()
    {
        foreach (Room room in MyDungeon.Rooms)
        {
            Vector3Int min = tilemap.WorldToCell(room.Footprint.min);
            Vector3Int max = tilemap.WorldToCell(room.Footprint.max);
            BoxFill(tilemap, RoomTile, min, max);
        }
    }

    public void TileCorridors()
    {
        foreach (Corridor corridor in MyDungeon.Corridors)
        {
            Vector3Int min = tilemap.WorldToCell(corridor.Footprint.min);
            Vector3Int max = tilemap.WorldToCell(corridor.Footprint.max);
            BoxFill(tilemap, CorridorTile, min, max);
        }
    }

    public GameObject GenerateTilemap()
    {
        GameObject tilemapGO = new GameObject();
        GameObject gridGO = new GameObject();

        tilemap = tilemapGO.AddComponent<Tilemap>();
        tilemapGO.AddComponent<TilemapRenderer>();
        Grid grid = gridGO.AddComponent<Grid>();
        gridGO.name = "Grid";
        tilemapGO.name = "Tilemap";
        tilemapGO.transform.parent = gridGO.transform;
        return gridGO;
    }


    public void BoxFill(Tilemap map, TileBase tile, Vector3Int start, Vector3Int end)
    {
        //Determine directions on X and Y axis
        var xDir = start.x < end.x ? 1 : -1;
        var yDir = start.y < end.y ? 1 : -1;
        //How many tiles on each axis?
        int xCols = Mathf.Abs(start.x - end.x);
        int yCols = Mathf.Abs(start.y - end.y);
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
}







