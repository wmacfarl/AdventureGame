using System.Collections.Generic;
using UnityEngine;

/*
 *  This class is used to define a connection between Rooms in a Dungeon.  Corridors are axis-aligned rectangles.  There is no provision for 
 *  Corridors that have turns in them.  
 *  
 *  If you wanted to make a corridor with a 90-degree turn you could potentially make two that connect to a 
 *  CorridorWidthxCorridorWidth room in the middle.
 *  
 *  The primary way of creating Corridors and Rooms (and Dungeons) is through the DungeonGenerator.  The class is simple and open-ended enough that
 *  they could be created in other ways, though.
 */

public class Corridor
{
    public Room[] ConnectedRooms;                   //The Rooms the Corridor is connected to
    public Rect Footprint;                          //A rectangle defining the location and dimensions of the Corridor.    

    //At the moment the only place this constructor is called is from the DungeonGenerator's CreateDungeon method.  

    //The constructor doesn't do any checking to see if the connection makes sense.  It is up to the code that creates the Corridor to ensure that
    //the connection makes sense given game and geometry logic.
    
    public Corridor(Room room1, Room room2, Rect footprint)
    {
        this.ConnectedRooms = new Room[2];
        this.ConnectedRooms[0] = room1;
        this.ConnectedRooms[1] = room2;
        this.Footprint = footprint;
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