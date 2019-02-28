using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRegion
{
    public DungeonRegion MySiblingRegion;
    float MinimumRoomArea;
    float MinimumRoomLength;
    float MaximumRoomArea;
    float ChanceToStopSplitting;
    int MinimumTreeDepthToStopSplitting;
    bool IsRoomRegion;
    public Rect RegionFootprint;
    int treeDepth;
    DungeonRoom DungeonRoom;
    Dungeon ParentDungeon;
    DungeonRegion[] SubRegions;

    private DungeonRegion(Dungeon parent)
    {
        this.ParentDungeon = parent;
        this.IsRoomRegion = false;
        this.MySiblingRegion = null;
    }

    public static DungeonRegion CreateRootRegion(Dungeon parentDungeon)
    {
        DungeonRegion rootRegion = new DungeonRegion(parentDungeon);
        rootRegion.RegionFootprint = new Rect(-parentDungeon.DungeonSize * .5f, parentDungeon.DungeonSize);
        rootRegion.MinimumRoomArea = parentDungeon.MinimumRoomArea;
        rootRegion.MinimumRoomLength = parentDungeon.MinimumRoomLength;
        rootRegion.MaximumRoomArea = parentDungeon.MaximumRoomArea;
        rootRegion.ChanceToStopSplitting = parentDungeon.ChanceToStopSplitting;
        rootRegion.MinimumTreeDepthToStopSplitting = parentDungeon.MinimumTreeDepthToStopSplitting;
        rootRegion.treeDepth = 0;
        return rootRegion;
    }

    public List<DungeonRoom> GetAllRoomsInRegion()
    {
        List<DungeonRegion> MyLeaves = this.GetLeafRegions(0);
        List<DungeonRoom> MyRooms = new List<DungeonRoom>();
        foreach (DungeonRegion leaf in MyLeaves)
        {
            if (MyRooms.Contains(leaf.DungeonRoom) == false)
            {
                MyRooms.Add(leaf.DungeonRoom);
            }
        }

        return MyRooms;
    }

    public List<DungeonRegion> GetLeafRegions(int depthCount)
    {
        List<DungeonRegion> leafRegions = new List<DungeonRegion>();
        if (this.SubRegions == null)
        {
            leafRegions.Add(this);
            return leafRegions;
        }
        else
        {
            leafRegions.AddRange(this.SubRegions[0].GetLeafRegions(depthCount));
            leafRegions.AddRange(this.SubRegions[1].GetLeafRegions(depthCount));
            return leafRegions;
        }
    }

    public void SetRoom(DungeonRoom myRoom)
    {
        this.DungeonRoom = myRoom;
    }

    DungeonRegion(Rect regionFootproint, DungeonRegion parentRegion)
    {
        this.MinimumRoomArea = parentRegion.MinimumRoomArea;
        this.MinimumRoomLength = parentRegion.MinimumRoomLength;
        this.MaximumRoomArea = parentRegion.MaximumRoomArea;
        this.ChanceToStopSplitting = parentRegion.ChanceToStopSplitting;
        this.MinimumTreeDepthToStopSplitting = parentRegion.MinimumTreeDepthToStopSplitting;
        this.treeDepth = parentRegion.treeDepth + 1;
        RegionFootprint = regionFootproint;
        IsRoomRegion = false;
    }

    public bool Split()
    {
        if (this.SubRegions != null || this.IsRoomRegion)
        {
            return false;
        }
        if (this.treeDepth > this.MinimumTreeDepthToStopSplitting && this.RegionFootprint.size.x * this.RegionFootprint.size.y < this.MaximumRoomArea)
        {
            if (Random.value < this.ChanceToStopSplitting)
            {
                this.IsRoomRegion = true;
                return false;
            }
        }

        if (this.RegionFootprint.size.x * RegionFootprint.size.y < this.MinimumRoomArea)
        {
            IsRoomRegion = true;
            return false;
        }

        bool canSplitHorizontally = true;
        bool canSplitVertically = true;

        if (this.RegionFootprint.size.x < this.MinimumRoomLength * 2)
        {
            canSplitHorizontally = false;
        }
        if (this.RegionFootprint.size.y < this.MinimumRoomLength * 2)
        {
            canSplitVertically = false;
        }

        bool doSplitHorizontally = true;
        float splitDistance = 0;

        if (canSplitHorizontally == false && canSplitVertically == false)
        {
            this.IsRoomRegion = true;
            return false;
        }

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
            splitDistance = Random.Range(this.MinimumRoomLength, this.RegionFootprint.size.x - this.MinimumRoomLength);
        }
        else
        {
            splitDistance = Random.Range(this.MinimumRoomLength, this.RegionFootprint.size.y - this.MinimumRoomLength);
        }

        splitDistance = Mathf.Round(splitDistance);

        if (splitDistance <= 0)
        {
            IsRoomRegion = true;
            return false;
        }

        Vector2 newRegionSize1, newRegionSize2, newRegionOrigin1, newRegionOrigin2;

        if (doSplitHorizontally)
        {
            newRegionSize1 = new Vector2(splitDistance, this.RegionFootprint.size.y);
            newRegionSize2 = new Vector2(this.RegionFootprint.size.x - splitDistance, RegionFootprint.size.y);
            newRegionOrigin1 = this.RegionFootprint.position;
            newRegionOrigin2 = this.RegionFootprint.position + Vector2.right * newRegionSize1.x;
        }
        else
        {
            newRegionSize1 = new Vector2(this.RegionFootprint.size.x, splitDistance);
            newRegionSize2 = new Vector2(this.RegionFootprint.size.x, RegionFootprint.size.y - splitDistance);
            newRegionOrigin1 = this.RegionFootprint.position;
            newRegionOrigin2 = this.RegionFootprint.position + Vector2.up * newRegionSize1.y;
        }

        Rect newRegion1Footprint = new Rect(newRegionOrigin1, newRegionSize1);
        Rect newRegion2Footprint = new Rect(newRegionOrigin2, newRegionSize2);

        if (IsFootprintTooSmall(newRegion1Footprint) || IsFootprintTooSmall(newRegion2Footprint))
        {
            IsRoomRegion = true;
            return false;
        }

        SubRegions = new DungeonRegion[2];

        SubRegions[0] = new DungeonRegion(newRegion1Footprint, this);
        SubRegions[1] = new DungeonRegion(newRegion2Footprint, this);

        SubRegions[0].ParentDungeon = this.ParentDungeon;
        SubRegions[1].ParentDungeon = this.ParentDungeon;

        this.ParentDungeon.AllSubRegions.AddRange(SubRegions);
        SubRegions[0].MySiblingRegion = SubRegions[1];
        SubRegions[1].MySiblingRegion = SubRegions[0];

        return true;
    }

    bool IsFootprintTooSmall(Rect footprint)
    {
        return (footprint.size.x < MinimumRoomLength || footprint.size.y < MinimumRoomLength || footprint.size.x * footprint.size.y < MinimumRoomArea);
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

public class DungeonCorridor
{
    public DungeonRoom[] ConnectedRooms;
    public List<DungeonCorridor> ConnectedCorridors;
    public Rect CorridorFootprint;

    public DungeonCorridor(DungeonRoom room1, DungeonRoom room2, Rect footprint)
    {
        this.ConnectedRooms = new DungeonRoom[2];
        this.ConnectedRooms[0] = room1;
        this.ConnectedRooms[1] = room2;
        this.CorridorFootprint = footprint;
        if (room1.Corridors.Contains(this) == false)
        {
            room1.Corridors.Add(this);
        }
        if (room2.Corridors.Contains(this) == false)
        {
            room2.Corridors.Add(this);
        }
    }
}

public class DungeonRoom
{
    public DungeonRegion ContainingRegion;
    public List<DungeonRoom> AdjacentRooms;
    public List<DungeonCorridor> Corridors;

    public Rect roomFootprint;

    public DungeonRoom(DungeonRegion region)
    {
        this.ContainingRegion = region;
        this.roomFootprint = region.RegionFootprint;
        this.AdjacentRooms = new List<DungeonRoom>();
        this.Corridors = new List<DungeonCorridor>();
    }

    public bool AmIConnectedByACorridorTo(DungeonRoom room)
    {
        foreach (DungeonCorridor corridor in Corridors)
        {
            if (corridor.ConnectedRooms[0] == room || corridor.ConnectedRooms[1] == room)
            {
                return true;
            }
        }
        return false;
    }

    public bool AmIAdjacentTo(DungeonRoom roomToTest)
    {
        return (this.AdjacentRooms.Contains(roomToTest) || roomToTest.AdjacentRooms.Contains(this));
    }
}