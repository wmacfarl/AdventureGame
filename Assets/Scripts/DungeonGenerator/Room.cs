using System.Collections.Generic;
using UnityEngine;

public class Room
{

    /*
    *  This class is used to define a Room in a Dungeon.  Dungeons are divided into DungeonRegions and every room is in a DungeonRegion.
    *  Rooms are connected to Corridors and every room can have any number of Corridors connected to it.
    *  
    *  The primary way of creating Rooms, Corridors (and Dungeons) is through the DungeonGenerator.  The Room class is simple and open-ended enough that
    *  they could be created in other ways, though.
    */

    public DungeonRegion ContainingRegion;  //Every Room is contained by a DungeonRegion that it takes up a fraction of.  
                                            //A DungeonRegion contains at most one Room.
    public List<Room> AdjacentRooms;        //AdjacentRooms are rooms that are contained by DungeonRegions that touch our DungeonRegion
    public List<Corridor> Corridors;        //The Corridors that connect to this room.
    public Rect roomFootprint;              //The Room's size and position



    //This Constructor is currently only called from DungeonGenerator.MakeDungeon()
    public Room(DungeonRegion region)
    {
        this.ContainingRegion = region; //The DungeonRegion
        this.roomFootprint = region.Footprint;
        this.AdjacentRooms = new List<Room>();
        this.Corridors = new List<Corridor>();
    }

    public bool AmIConnectedByACorridorTo(Room room)
    {
        foreach (Corridor corridor in Corridors)
        {
            if (corridor.ConnectedRooms[0] == room || corridor.ConnectedRooms[1] == room)
            {
                return true;
            }
        }
        return false;
    }

    //This method is primarily used when generating the Dungeon to decide what rooms we can connect with Corridors but it's in this class because it
    //seems like it might have more general utility.

    //It is up to whatever method is creating and generating rooms to keep the AdjacentRooms list up to date so that this method works as expected
    public bool AmIAdjacentTo(Room roomToTest)
    {
        return (this.AdjacentRooms.Contains(roomToTest) || roomToTest.AdjacentRooms.Contains(this));
    }
}