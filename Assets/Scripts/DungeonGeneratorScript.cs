using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneratorScript : MonoBehaviour
{
    [SerializeField]
    float MinimumAreaForDungeonRoom;

    [SerializeField]
    float MaximumAreaForDungeonRoom;

    [SerializeField]
    float MinimumLengthForDungeonRoom;

    [SerializeField]
    Vector2 SizeOfDungeonToGenerate;

    [SerializeField]
    float PercentChanceToStopSplittingRoom;

    [SerializeField]
    int MinimumDepthToStopSplitting;

    [SerializeField]
    float DungeonCorridorWidth;

    [SerializeField]
    float ScaleDownFactorMin;

    [SerializeField]
    float ScaleDownFactorMax;

    public void Start()
    {
        Dungeon newDungeon = new Dungeon(SizeOfDungeonToGenerate, MinimumAreaForDungeonRoom, MaximumAreaForDungeonRoom, MinimumLengthForDungeonRoom,
                                        PercentChanceToStopSplittingRoom, MinimumDepthToStopSplitting, DungeonCorridorWidth);
        bool splitARegionThisIteration = true;
        int loopCounter = 0;
        while (splitARegionThisIteration && loopCounter < 100)
        {
            loopCounter++;
            splitARegionThisIteration = false;
            List<DungeonRegion> currentLeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
            foreach (DungeonRegion dungeonRegion in currentLeafRegions)
            {
                if (dungeonRegion.Split())
                {
                    splitARegionThisIteration = true;
                }
            }
        }

        List<DungeonRegion> LeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
        foreach (DungeonRegion dungeonRegion in LeafRegions)
        {
            DungeonRoom newRoom = new DungeonRoom(dungeonRegion);
            newDungeon.Rooms.Add(newRoom);
            dungeonRegion.SetRoom(newRoom);
        }

        foreach (DungeonRoom room in newDungeon.Rooms)
        {
            foreach (DungeonRoom room2 in newDungeon.Rooms)
            {
                if (room != room2)
                {
                    if (RectHelper.DoRectsTouchWithinEpsilon(room.roomFootprint, room2.roomFootprint, .01f))
                    {
                        if (room.AdjacentRooms.Contains(room2) == false)
                        {
                            room.AdjacentRooms.Add(room2);
                        }
                        if (room2.AdjacentRooms.Contains(room) == false)
                        {
                            room2.AdjacentRooms.Add(room);
                        }
                    }
                }
            }
        }

        foreach (DungeonRoom room in newDungeon.Rooms)
        {
            Vector2 scaleDownFactor = new Vector2(Random.Range(ScaleDownFactorMin, ScaleDownFactorMax), Random.Range(ScaleDownFactorMin, ScaleDownFactorMax));
            Vector2 newSize = new Vector2(room.roomFootprint.size.x * scaleDownFactor.x, room.roomFootprint.size.y * scaleDownFactor.y);
            Vector2 sizeDelta = room.roomFootprint.size - newSize;
            float xSplit = Random.value;
            float ySplit = Random.value;
            room.roomFootprint.xMax -= sizeDelta.x * xSplit;
            room.roomFootprint.xMin += sizeDelta.x * (1-xSplit);
            room.roomFootprint.yMax -= sizeDelta.y * ySplit;
            room.roomFootprint.yMin += sizeDelta.y * (1 - ySplit);
            RectHelper.RoundToIntegerDimensions(room.roomFootprint);            
        }

        //Connect a room in each region to an adjacent room in its sibling region.
        foreach (DungeonRegion subRegion in newDungeon.AllSubRegions)
        {
            DungeonRegion siblingRegion = subRegion.MySiblingRegion;
            List<DungeonRoom> siblingRooms = siblingRegion.GetAllRoomsInRegion();
            bool madeConnection = false;
            foreach (DungeonRoom room in subRegion.GetAllRoomsInRegion())
            {
                foreach (DungeonRoom siblingRoom in siblingRegion.GetAllRoomsInRegion())
                {
                    if (room.AmIAdjacentTo(siblingRoom) && madeConnection == false && room.AmIConnectedByACorridorTo(siblingRoom) == false)
                    {
                        if (newDungeon.CreateCorridorBetweenRooms(room, siblingRoom))
                        {
                            madeConnection = true;
                        }
                    }
                }
            }
        }

        newDungeon.DebugDrawDungeon(20000);
        Debug.Log("Check = " + CheckDungeon(newDungeon));
    }

    GameObject CreateDungeonGameObject(Dungeon dungeon)
    {
        GameObject DungeonGameObject = new GameObject();
        DungeonGameObject.AddComponent<DungeonManagerScript>();
        return DungeonGameObject;
    }
}

public class Dungeon
{
    public DungeonRegion RootRegion;
    public List<DungeonCorridor> Corridors;
    public List<DungeonRegion> AllSubRegions;
    public List<DungeonRoom> Rooms;
    public Vector2 DungeonSize;
    public float MinimumRoomArea;
    public float MinimumRoomLength;
    public float MaximumRoomArea;
    public float ChanceToStopSplitting;
    public int MinimumTreeDepthToStopSplitting;
    public float CorridorWidth;


    public Dungeon(Vector2 size, float minArea, float maxArea, float minLength, float chanceToStopSplit, int minDepth, float corridorWidth)
    {
        this.DungeonSize = size;
        this.AllSubRegions = new List<DungeonRegion>();
        this.MinimumRoomArea = minArea;
        this.MaximumRoomArea = maxArea;
        this.MinimumRoomLength = minLength;
        this.ChanceToStopSplitting = chanceToStopSplit;
        this.MinimumTreeDepthToStopSplitting = minDepth;
        this.Corridors = new List<DungeonCorridor>();
        this.Rooms = new List<DungeonRoom>();
        this.CorridorWidth = corridorWidth;
        this.RootRegion = DungeonRegion.CreateRootRegion(this);
    }

    public bool CreateCorridorBetweenRooms(DungeonRoom room1, DungeonRoom room2)
    {
        Vector2 hallwayStartingPoint = room1.roomFootprint.center;
        Vector2 hallwayDirection = new Vector2();
        Vector2 hallwayEndingPoint = new Vector2();
        DungeonRegion region1 = room1.ContainingRegion;
        DungeonRegion region2 = room2.ContainingRegion;
        Color color = Color.grey;

        if (RectHelper.DoRectsTouchInX(room1.ContainingRegion.RegionFootprint, room2.ContainingRegion.RegionFootprint, .01f))
        {
            float minY = Mathf.Max(room1.roomFootprint.yMin, room2.roomFootprint.yMin) + 1;
            float maxY = Mathf.Min(room1.roomFootprint.yMax, room2.roomFootprint.yMax) - 1;
            if (maxY - minY < 2)
            {
                return false;
            }
            float yValue = Mathf.Round(Random.Range(minY, maxY));

            if (room1.roomFootprint.position.x < room2.roomFootprint.position.x)
            {
                hallwayDirection = Vector2.right;
                hallwayStartingPoint = new Vector2(room1.roomFootprint.xMax, yValue);
                hallwayEndingPoint = new Vector2(room2.roomFootprint.xMin, yValue);
            }
            else
            {
                hallwayDirection = Vector2.left;
                hallwayStartingPoint = new Vector2(room1.roomFootprint.xMin, yValue);
                hallwayEndingPoint = new Vector2(room2.roomFootprint.xMax, yValue);
            }
        }
        else if (RectHelper.DoRectsTouchInY(room1.ContainingRegion.RegionFootprint, room2.ContainingRegion.RegionFootprint, .01f))
        {
            float minX = Mathf.Max(room1.roomFootprint.xMin, room2.roomFootprint.xMin) + 1;
            float maxX = Mathf.Min(room1.roomFootprint.xMax, room2.roomFootprint.xMax) - 1;
            if (maxX - minX < 2)
            {
                return false;
            }
            float xValue = Mathf.Round(Random.Range(minX, maxX));
            if (room1.roomFootprint.position.y < room2.roomFootprint.position.y)
            {
                hallwayDirection = Vector2.up;
                hallwayStartingPoint = new Vector2(xValue, room1.roomFootprint.yMax);
                hallwayEndingPoint = new Vector2(xValue, room2.roomFootprint.yMin);
            }
            else
            {
                hallwayDirection = Vector2.down;
                hallwayStartingPoint = new Vector2(xValue, room1.roomFootprint.yMin);
                hallwayEndingPoint = new Vector2(xValue, room2.roomFootprint.yMax);
            }
        }
        else
        {
            Debug.Log("Rooms are not adjacent and so cannot be connected with a corridor.");
            Debug.DrawLine(hallwayStartingPoint, hallwayEndingPoint, Color.yellow, 20000);
        }

        Vector2 hallwayDimensions = hallwayStartingPoint - hallwayEndingPoint;
        hallwayDimensions += Vector2.Perpendicular(hallwayDirection);
        Rect corridorRect = new Rect(hallwayStartingPoint, hallwayDimensions * -1);
        float xMin = corridorRect.xMin;
        float xMax = corridorRect.xMax;
        float yMin = corridorRect.yMin;
        float yMax = corridorRect.yMax;

        if (xMin > xMax)
        {
            corridorRect.xMin = xMax;
            corridorRect.xMax = xMin;
        }
        if (yMin > yMax)
        {
            corridorRect.yMin = yMax;
            corridorRect.yMax = yMin;
        }
        RectHelper.RoundToIntegerDimensions(corridorRect);
        DungeonCorridor newCorridor = new DungeonCorridor(room1, room2, corridorRect);
        this.Corridors.Add(newCorridor);
        return true;
    }

    public void ScaleUpDungeon(float scaleFactor)
    {
        throw new System.Exception("Not written yet");
    }

    public void DebugDrawDungeon(float duration)
    {
        foreach (DungeonRoom room in Rooms)
        {
            RectHelper.DebugDrawRect(room.roomFootprint, Color.green, duration);
        }
        foreach (DungeonCorridor corridor in Corridors)
        {
            RectHelper.DebugDrawRect(corridor.CorridorFootprint, Color.yellow, duration);
        }
    }

    public bool CheckDungeon(Dungeon dungeon)
    {
        foreach (DungeonRoom room1 in dungeon.Rooms)
        {
            foreach (DungeonRoom room2 in dungeon.Rooms)
            {
                if (room1 != room2)
                {
                    if (RectHelper.DoRectsTouchWithinEpsilon(room1.roomFootprint, room2.roomFootprint, .01f))
                    {
                        Debug.Log("Rooms touch");
                        return false;
                    }
                }
            }
            foreach (DungeonCorridor corridor in room1.Corridors)
            {
                if (corridor.ConnectedRooms[0] != room1 && corridor.ConnectedRooms[1] != room1)
                {
                    Debug.Log("Room contains corridor in room.corridors but corridor does not contain room as connected room.");
                    return false;
                }
            }
            if (room1.Corridors.Count == 0)
            {
                Debug.Log("Room has no connections");
                return false;
            }
        }

        foreach (DungeonCorridor corridor in dungeon.Corridors)
        {
            foreach (DungeonRoom room in corridor.ConnectedRooms)
            {
                if (room.Corridors.Contains(corridor) == false)
                {
                    Debug.Log("Corridor contains room as connected room but room does not contain corridor.");
                    return false;
                }

                if (RectHelper.DoRectsTouchWithinEpsilon(room.roomFootprint, corridor.CorridorFootprint, .1f) == false)
                {
                    RectHelper.DebugDrawRect(corridor.CorridorFootprint, Color.red, 20000);
                    RectHelper.DebugDrawRect(room.roomFootprint, Color.magenta, 20000);

                    Debug.Log("corridor.minX = " + corridor.CorridorFootprint.xMin + "corridor.maxX = " + corridor.CorridorFootprint.xMax);
                    Debug.Log("corridor.minY = " + corridor.CorridorFootprint.yMin + "corridor.maxY = " + corridor.CorridorFootprint.yMax);

                    Debug.Log("room.minX = " + room.roomFootprint.xMin + "room.maxX = " + room.roomFootprint.xMax);
                    Debug.Log("room.minY = " + room.roomFootprint.yMin + "room.maxY = " + room.roomFootprint.yMax);


                    Debug.Log("Room does not connect to corridor");
                    return false;
                }
            }
            if (corridor.ConnectedRooms[0] == null || corridor.ConnectedRooms[1] == null)
            {
                Debug.Log("Corridor has null connection");
                return false;
            }
        }
        return true;
    }
}







