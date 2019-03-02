using System.Collections.Generic;
using UnityEngine;

public class DungeonRegion
{
    public DungeonRegion MySiblingRegion;
    public Rect Footprint;
    public int DepthInTree;
    public Room DungeonRoom;
    public Dungeon ParentDungeon;
    public DungeonRegion[] SubRegions;
    public bool IsRoomRegion;

    public DungeonRegion(Dungeon parent)
    {
        this.IsRoomRegion = false;
        this.ParentDungeon = parent;
        this.MySiblingRegion = null;
    }

    public DungeonRegion(Rect regionFootproint, DungeonRegion parentRegion) : this(parentRegion.ParentDungeon)
    {
        this.DepthInTree = parentRegion.DepthInTree + 1;
        Footprint = regionFootproint;
    }

    public List<Room> GetAllRoomsInRegion()
    {
        List<DungeonRegion> MyLeaves = this.GetLeafRegions(0);
        List<Room> MyRooms = new List<Room>();
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

    public void SetRoom(Room myRoom)
    {
        this.DungeonRoom = myRoom;
    }
}