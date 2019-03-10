using System.Collections.Generic;
using UnityEngine;

  /*
   *  This class is used to define a Dungeon.  Dungeons contain Rooms connected by Corridors.
   *  
   *  Dungeons are divided into DungeonRegions structured as a Binary Space Partitioning Tree.  Every leaf-node in the tree contains a Room.
   *  
   *  Rooms are connected to Corridors and every room can have any number of Corridors connected to it.
   *  
   *  The primary way of creating Rooms, Corridors (and Dungeons) is through the DungeonGenerator.  The Room class is simple and open-ended enough that
   *  they could be created in other ways, though.
   */
public class DungeonFloor
{
    public DungeonRegion RootRegion;    //The root DungeonRegion.  This is the only DungeonRegion without a sibling.
    public List<Corridor> Corridors;
    public List<DungeonRegion> AllSubRegions;   //All of the DungeonRegions in the Dungeon arranged as a List rather than a tree for easy iteration.
    public List<Room> Rooms;                    
    public Vector2 DungeonSize;                 
    
    //This constructor is called from DungeonGenerator.MakeDungeon()
    public DungeonFloor(Vector2 size)
    {
        this.DungeonSize = size;
        this.AllSubRegions = new List<DungeonRegion>();
        this.Corridors = new List<Corridor>();
        this.Rooms = new List<Room>();
    }

    //A helper method for drawing the Dungeon layout in the Scene view.
    public void DebugDraw(float duration)
    {
        foreach (Room room in Rooms)
        {
            RectHelper.DebugDrawRect(room.Footprint, Color.green, duration);
        }
        foreach (Corridor corridor in Corridors)
        {
            RectHelper.DebugDrawRect(corridor.Footprint, Color.yellow, duration);
        }
    }
}

