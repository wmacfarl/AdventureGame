using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    public DungeonRegion RootRegion;
    public List<Corridor> Corridors;
    public List<DungeonRegion> AllSubRegions;
    public List<Room> Rooms;
    public Vector2 DungeonSize;
    


    public Dungeon(Vector2 size)
    {
        this.DungeonSize = size;
        this.AllSubRegions = new List<DungeonRegion>();
        this.Corridors = new List<Corridor>();
        this.Rooms = new List<Room>();
    }

    public void DebugDraw(float duration)
    {
        foreach (Room room in Rooms)
        {
            RectHelper.DebugDrawRect(room.roomFootprint, Color.green, duration);
        }
        foreach (Corridor corridor in Corridors)
        {
            RectHelper.DebugDrawRect(corridor.Footprint, Color.yellow, duration);
        }
    }
}

