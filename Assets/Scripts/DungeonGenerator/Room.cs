using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public DungeonRegion ContainingRegion;
    public List<Room> AdjacentRooms;
    public List<Corridor> Corridors;

    public Rect roomFootprint;

    public Room(DungeonRegion region)
    {
        this.ContainingRegion = region;
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

    public bool AmIAdjacentTo(Room roomToTest)
    {
        return (this.AdjacentRooms.Contains(roomToTest) || roomToTest.AdjacentRooms.Contains(this));
    }
}