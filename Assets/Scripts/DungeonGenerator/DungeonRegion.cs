using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 *  This class defines a node in a 2D Binary Space Partition tree which describes the layout of a Dungeon.
 *  
 *  Each Dungeon has a RootRegion that doesn't have any siblings.  This region is divided into two DungeonRegions that are siblings of eachother and
 *  are SubRegions of the RootRegion.  SubRegions are recursively divided into smaller and smaller pairs.
 *  
 *  The last level of DungeonRegions are called the "leaf" regions.  They are not divided and don't have any SubRegions.  Each leaf region contains
 *  a Room.
 *  
 *  Dungeons are divided into DungeonRegions structured as a Binary Space Partitioning Tree.  Every leaf-node in the tree contains a Room.
 *  
 *  Presently the BSP tree structure is only being used for generating the Dungeon and ensuring full-connectivity between rooms.  It could potentially
 *  be used for gameplay logic as well because the speed of traversing a BSP tree is quite fast.
 */

public class DungeonRegion
{
    public Dungeon ParentDungeon;           //The Dungeon that uses the DungeonRegion
    public DungeonRegion MySiblingRegion;   //All non-root regions are part of a matched pair from their parent region being divided.
    public Rect Footprint;                  //The location and dimensions of the region.
    public DungeonRegion[] SubRegions;      //If the DungeonRegion is not a leaf it will have 2 SubRegions
    public Room DungeonRoom;                //If the DungeonRegion is a leaf it will contain a DungeonRoom

    public int DepthInTree;                 //Number of subdivisions from the root node
    public bool WillBeLeafRegion;           //A flag that keeps track whether this region has been marked to become a leaf


    //Called from DungeonGenerator.CreateRootRegion() and DungeonGenerator.SplitRegion()
    public DungeonRegion(Dungeon parent)
    {
        this.WillBeLeafRegion = false;
        this.ParentDungeon = parent;
        this.MySiblingRegion = null;
    }

    public DungeonRegion(Rect regionFootproint, DungeonRegion parentRegion) : this(parentRegion.ParentDungeon)
    {
        this.DepthInTree = parentRegion.DepthInTree + 1;
        Footprint = regionFootproint;
    }

    //This locates all of the leaf nodes under a particular DungeonRegion and returns the Rooms associated with these nodes
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

    //This recursively searches all of the DungeonRegion's SubRegions to find Leaf regions.  

    //The depthCount variable is just a counter for how many recursions we've done to prevent infinite loops during development and 
    //debugging because crashing and having to restart Unity is a pain.

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