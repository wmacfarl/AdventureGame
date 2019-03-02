using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator{
    float MinimumAreaForDungeonRoom;
    float MaximumAreaForDungeonRoom;
    float MinimumLengthForDungeonRoom;
    Vector2 SizeOfDungeonToGenerate;
    float ChanceToStopSplittingRoom;
    int MinimumDepthToStopSplitting;
    float DungeonCorridorWidth;
    float MinimumPercentOfRegionForRoom;
    float MaximumPercentOfRegionForRoom;

    public DungeonGenerator(float minArea, float maxArea, float minLength, Vector2 size, float chanceToSplit, int minSplitDepth, 
        float corridorWidth, float minRoomPercent, float maxRoomPercent)
    {
        this.MinimumAreaForDungeonRoom = minArea;
        this.MaximumAreaForDungeonRoom = maxArea;
        this.MinimumLengthForDungeonRoom = minLength;
        this.SizeOfDungeonToGenerate = size;
        this.ChanceToStopSplittingRoom = chanceToSplit;
        this.MinimumDepthToStopSplitting = minSplitDepth;
        this.DungeonCorridorWidth = corridorWidth;
        this.MinimumPercentOfRegionForRoom = minRoomPercent;
        this.MaximumPercentOfRegionForRoom = maxRoomPercent;
    }


    public Dungeon MakeDungeon()
    {
        Dungeon newDungeon = new Dungeon(SizeOfDungeonToGenerate);
        CreateRootRegion(newDungeon);

        bool didISplit = true;
        int loopCounter = 0;
        while (didISplit && loopCounter < 100)
        {
            loopCounter++;
            didISplit = false;
            List<DungeonRegion> currentLeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
            foreach (DungeonRegion dungeonRegion in currentLeafRegions)
            {
                if (SplitRegion(dungeonRegion))
                {
                    didISplit = true;
                }
            }
        }

        List<DungeonRegion> LeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
        foreach (DungeonRegion dungeonRegion in LeafRegions)
        {
            RectHelper.DebugDrawRect(dungeonRegion.Footprint, Color.red, 20000);
            Room newRoom = new Room(dungeonRegion);
            newDungeon.Rooms.Add(newRoom);
            dungeonRegion.SetRoom(newRoom);

        }

        foreach (Room room in newDungeon.Rooms)
        {
            foreach (Room room2 in newDungeon.Rooms)
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

        foreach (Room room in newDungeon.Rooms)
        {

            Vector2 scaleDownFactor = new Vector2(Random.Range(MinimumPercentOfRegionForRoom, MaximumPercentOfRegionForRoom), 
                Random.Range(MinimumPercentOfRegionForRoom, MaximumPercentOfRegionForRoom));
            Vector2 newSize = new Vector2(room.roomFootprint.size.x * scaleDownFactor.x, room.roomFootprint.size.y * scaleDownFactor.y);
            Vector2 sizeDelta = room.roomFootprint.size - newSize;
            float xSplit = Random.value;
            float ySplit = Random.value;
            room.roomFootprint.xMax -= sizeDelta.x * xSplit;
            room.roomFootprint.xMin += sizeDelta.x * (1 - xSplit);
            room.roomFootprint.yMax -= sizeDelta.y * ySplit;
            room.roomFootprint.yMin += sizeDelta.y * (1 - ySplit);
            
        }

        //Connect a room in each region to an adjacent room in its sibling region.
        foreach (DungeonRegion subRegion in newDungeon.AllSubRegions)
        {
            DungeonRegion siblingRegion = subRegion.MySiblingRegion;
            List<Room> siblingRooms = siblingRegion.GetAllRoomsInRegion();
            bool madeConnection = false;
            foreach (Room room in subRegion.GetAllRoomsInRegion())
            {
                foreach (Room siblingRoom in siblingRegion.GetAllRoomsInRegion())
                {
                    if (room.AmIAdjacentTo(siblingRoom) && madeConnection == false && room.AmIConnectedByACorridorTo(siblingRoom) == false)
                    {
                        if (CreateCorridorBetweenRooms(newDungeon, room, siblingRoom))
                        {
                            madeConnection = true;
                        }
                    }
                }
            }
        }

        newDungeon.DebugDraw(20000);
        Debug.Log("Check = " + CheckDungeon(newDungeon));
        return newDungeon;
    }

    GameObject CreateDungeonGameObject(Dungeon dungeon)
    {
        GameObject DungeonGameObject = new GameObject();
        DungeonGameObject.AddComponent<DungeonManagerScript>();
        return DungeonGameObject;
    }

    public bool CheckDungeon(Dungeon dungeon)
    {
        foreach (Room room1 in dungeon.Rooms)
        {
            foreach (Room room2 in dungeon.Rooms)
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
            foreach (Corridor corridor in room1.Corridors)
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

        foreach (Corridor corridor in dungeon.Corridors)
        {
            foreach (Room room in corridor.ConnectedRooms)
            {
                if (room.Corridors.Contains(corridor) == false)
                {
                    Debug.Log("Corridor contains room as connected room but room does not contain corridor.");
                    return false;
                }

                if (RectHelper.DoRectsTouchWithinEpsilon(room.roomFootprint, corridor.Footprint, .1f) == false)
                {
                    RectHelper.DebugDrawRect(corridor.Footprint, Color.red, 20000);
                    RectHelper.DebugDrawRect(room.roomFootprint, Color.magenta, 20000);

                    Debug.Log("corridor.minX = " + corridor.Footprint.xMin + "corridor.maxX = " + corridor.Footprint.xMax);
                    Debug.Log("corridor.minY = " + corridor.Footprint.yMin + "corridor.maxY = " + corridor.Footprint.yMax);

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

    bool CreateCorridorBetweenRooms(Dungeon dungeon, Room room1, Room room2)
    {
        Vector2 hallwayStartingPoint = room1.roomFootprint.center;
        Vector2 hallwayDirection = new Vector2();
        Vector2 hallwayEndingPoint = new Vector2();
        DungeonRegion region1 = room1.ContainingRegion;
        DungeonRegion region2 = room2.ContainingRegion;
        Color color = Color.grey;

        if (RectHelper.DoRectsTouchInX(room1.ContainingRegion.Footprint, room2.ContainingRegion.Footprint, .01f))
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
        else if (RectHelper.DoRectsTouchInY(room1.ContainingRegion.Footprint, room2.ContainingRegion.Footprint, .01f))
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
        Corridor newCorridor = new Corridor(room1, room2, corridorRect);
        dungeon.Corridors.Add(newCorridor);
        return true;

    }

    public void ScaleUpDungeon(Dungeon dungeon, float scaleFactor)
    {
        throw new System.Exception("Not written yet");
    }

    public void CreateRootRegion(Dungeon parentDungeon)
    {
        DungeonRegion rootRegion = new DungeonRegion(parentDungeon);
        rootRegion.Footprint = new Rect(-parentDungeon.DungeonSize * .5f, parentDungeon.DungeonSize);
        rootRegion.DepthInTree = 0;
        parentDungeon.RootRegion = rootRegion;
    }

    bool SplitRegion(DungeonRegion region)
    {
        if (region.SubRegions != null || region.IsRoomRegion)
        {
            return false;
        }
        if (region.DepthInTree > MinimumDepthToStopSplitting && region.Footprint.size.x * region.Footprint.size.y < MaximumAreaForDungeonRoom)
        {
            if (Random.value < ChanceToStopSplittingRoom)
            {
                region.IsRoomRegion = true;
                return false;
            }
        }

        if (region.Footprint.size.x * region.Footprint.size.y < MinimumAreaForDungeonRoom)
        {
            region.IsRoomRegion = true;
            return false;
        }

        bool canSplitHorizontally = true;
        bool canSplitVertically = true;
        if (region.Footprint.size.x < MinimumLengthForDungeonRoom * 2)
        {
            canSplitHorizontally = false;
        }
        if (region.Footprint.size.y < MinimumLengthForDungeonRoom * 2)
        {
            canSplitVertically = false;
        }

        if (canSplitHorizontally == false && canSplitVertically == false)
        {
            region.IsRoomRegion = true;
            return false;
        }

        bool doSplitHorizontally = true;
        float splitDistance = 0;

        if (canSplitHorizontally == true && canSplitVertically == false)
        {
            doSplitHorizontally = true;
        }
        else if (canSplitVertically == true && canSplitHorizontally == false)
        {
            doSplitHorizontally = false;
        }
        else
        {
            doSplitHorizontally = (Random.value < .5f);
        }

        if (doSplitHorizontally)
        {
            splitDistance = Random.Range(MinimumLengthForDungeonRoom, region.Footprint.size.x - MinimumLengthForDungeonRoom);
        }
        else
        {
            splitDistance = Random.Range(MinimumLengthForDungeonRoom, region.Footprint.size.y - MinimumLengthForDungeonRoom);
        }

        splitDistance = Mathf.Round(splitDistance);

        if (splitDistance <= 0)
        {
            region.IsRoomRegion = true;
            return false;
        }

        Vector2 newRegionSize1, newRegionSize2, newRegionOrigin1, newRegionOrigin2;

        if (doSplitHorizontally)
        {
            newRegionSize1 = new Vector2(splitDistance, region.Footprint.size.y);
            newRegionSize2 = new Vector2(region.Footprint.size.x - splitDistance, region.Footprint.size.y);
            newRegionOrigin1 = region.Footprint.position;
            newRegionOrigin2 = region.Footprint.position + Vector2.right * newRegionSize1.x;
        }
        else
        {
            newRegionSize1 = new Vector2(region.Footprint.size.x, splitDistance);
            newRegionSize2 = new Vector2(region.Footprint.size.x, region.Footprint.size.y - splitDistance);
            newRegionOrigin1 = region.Footprint.position;
            newRegionOrigin2 = region.Footprint.position + Vector2.up * newRegionSize1.y;
        }

        Rect newRegion1Footprint = new Rect(newRegionOrigin1, newRegionSize1);
        Rect newRegion2Footprint = new Rect(newRegionOrigin2, newRegionSize2);

        if (IsFootprintTooSmallForRoom(newRegion1Footprint) || IsFootprintTooSmallForRoom(newRegion2Footprint))
        {
            region.IsRoomRegion = true;
            return false;
        }

        region.SubRegions = new DungeonRegion[2];

        region.SubRegions[0] = new DungeonRegion(newRegion1Footprint, region);
        region.SubRegions[1] = new DungeonRegion(newRegion2Footprint, region);

        region.SubRegions[0].ParentDungeon = region.ParentDungeon;
        region.SubRegions[1].ParentDungeon = region.ParentDungeon;

        region.ParentDungeon.AllSubRegions.AddRange(region.SubRegions);
        region.SubRegions[0].MySiblingRegion = region.SubRegions[1];
        region.SubRegions[1].MySiblingRegion = region.SubRegions[0];

        return true;
    }

    bool IsFootprintTooSmallForRoom(Rect footprint)
    {
        return (footprint.size.x < MinimumLengthForDungeonRoom || footprint.size.y < MinimumLengthForDungeonRoom
            || footprint.size.x * footprint.size.y < MinimumAreaForDungeonRoom);
    }
}







