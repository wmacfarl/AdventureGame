using System.Collections.Generic;
using UnityEngine;

/*
 * This class takes a set of parameters defining the Dungeon generation process and generates a random dungeon based on those parameters.
 * 
 * The Dungeon is generated using a Binary Space Partitioning tree with an algorithm described here:
 * http://www.roguebasin.com/index.php?title=Basic_BSP_Dungeon_generation
 * 
 */

public class DungeonGenerator{
    Vector2 SizeOfDungeonToGenerate;

    float MinimumAreaToSplitRegion;    //If a DungeonRegions's area is less then this value it will stop splitting into SubRegions during the
                                        //generation process

    float MinimumLengthForDungeonRegion;  //If a DungeonRegions's x or y dimension is less then this value it will stop splitting into SubRegions 
                                        //during the generation process

    float ChanceToStopSplittingRoom;    //Once the DungeonRegions have split a certain number of times, they have a percent chance to stop splitti
                                        //even if they are large enough that they could split.  This adds variety in room size

    int MinimumDepthToStopSplitting;    //The number of Splits that need to occur before we check whether we should stop splitting
    float MaximumAreaForDungeonRoom;    //DungeonRegion must be smaller than this value before we start checking whether to randomly stop splitting  
    float DungeonCorridorWidth;          
    
    float MinimumPercentOfRegionForRoom;//After DungeonRegions are defined through splits, leaf regions contain rooms.  Rooms take up a subset of 
    float MaximumPercentOfRegionForRoom;//the space in their region.  These variables set the minimum and maximum amount of the region to have the minArea;

    int DungeonScaleFactor;           //An amount to scale the Dungeon after it is generated.  This might in some cases be better than generating a        
                                      //larger dungeon.

    //Constructor invoked by DungeonGenerationScript from parameters set in the inspector.
    public DungeonGenerator(Vector2 size, float minRoomArea, float minRoomLength, float chanceToStopSplitting, int minimumSplitsBeforeStop, 
                            float maxRegionAreaBeforeStopSplitting,float corridorWidth, float minRegionPercentOccupiedByRoom, 
                            float maxRegionPercentOccupiedByRoom, int dungeonScale)
    {
        this.MinimumAreaToSplitRegion = minRoomArea;
        this.MaximumAreaForDungeonRoom = maxRegionAreaBeforeStopSplitting;
        this.MinimumLengthForDungeonRegion = minRoomLength;
        this.SizeOfDungeonToGenerate = size;
        this.ChanceToStopSplittingRoom = chanceToStopSplitting;
        this.MinimumDepthToStopSplitting = minimumSplitsBeforeStop;
        this.DungeonCorridorWidth = corridorWidth;
        this.MinimumPercentOfRegionForRoom = minRegionPercentOccupiedByRoom;
        this.MaximumPercentOfRegionForRoom = maxRegionPercentOccupiedByRoom;

        if (MinimumAreaToSplitRegion < 1)
        {
            throw new System.Exception("MinimumAreaToSplitRegion cannot be less then 1");
        }
        if (MinimumLengthForDungeonRegion < 1)
        {
            throw new System.Exception("MinimumLengthForDungeonRegion cannot be less then 1");
        }
        if (DungeonCorridorWidth < 1)
        {
            throw new System.Exception("DungeonCorridorWidth cannot be less then 1");
        }
        if (MinimumPercentOfRegionForRoom > 1)
        {
            throw new System.Exception("MinimumPercentOfRegionForRoom cannot be greater then 1");
        }
        if (MaximumPercentOfRegionForRoom > 1)
        {
            throw new System.Exception("MaximumPercentOfRegionForRoom cannot be greater then 1");
        }
        if (MinimumPercentOfRegionForRoom > MaximumAreaForDungeonRoom)
        {
            throw new System.Exception("MaximumPercentOfRegionForRoom cannot be less then MinimumPercentOfRegionForRoom");
        }
        this.DungeonScaleFactor = dungeonScale;
    }

    //MakeDungeon() returns a Dungeon object randomly generated based on the DungeonGenerator's parameters.
    public DungeonFloor MakeDungeon()
    {
        //Make a root region that is the size of the whole Dungeon
        DungeonFloor newDungeon = new DungeonFloor(SizeOfDungeonToGenerate);
        CreateAndSetRootRegion(newDungeon);

        //Run through each LeafRegion in the Dungeon and attempt to split that Leaf.  This should generate two new LeafRegions and make the region
        //that we just split no longer a Leaf.  We repeat this until we don't successfully perform any splits on any of the Leaves (because the rooms
        //are small enough) and the DungeonRegions are defined.
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

        //Add a Room object for each LeafRegion in the Dungeon.
        List<DungeonRegion> LeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
        foreach (DungeonRegion dungeonRegion in LeafRegions)
        {
            Room newRoom = new Room(dungeonRegion);
            newDungeon.Rooms.Add(newRoom);
            dungeonRegion.SetRoom(newRoom);

        }

        //For each Room in the Dungeon, find all adjacent rooms and store them in Room.AdjacentRooms for future lookup.
        foreach (Room room in newDungeon.Rooms)
        {
            foreach (Room room2 in newDungeon.Rooms)
            {
                if (room != room2)
                {
                    if (RectHelper.DoRectsTouchWithinEpsilon(room.Footprint, room2.Footprint, .01f))
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

        //At this point, the rooms take up 100% of the DungeonRegion that contains them.  Now we scale them down to create space between the walls
        //based on the parameters set in the DungeonGenerator constructor.
        foreach (Room room in newDungeon.Rooms)
        {

            Vector2 scaleDownFactor = new Vector2(Random.Range(MinimumPercentOfRegionForRoom, MaximumPercentOfRegionForRoom), 
                Random.Range(MinimumPercentOfRegionForRoom, MaximumPercentOfRegionForRoom));
            Vector2 newSize = new Vector2(room.Footprint.size.x * scaleDownFactor.x, room.Footprint.size.y * scaleDownFactor.y);
            Vector2 sizeDelta = room.Footprint.size - newSize;
            float xSplit = Random.value;
            float ySplit = Random.value;
            room.Footprint.xMax -= sizeDelta.x * xSplit;
            room.Footprint.xMin += sizeDelta.x * (1 - xSplit);
            room.Footprint.yMax -= sizeDelta.y * ySplit;
            room.Footprint.yMin += sizeDelta.y * (1 - ySplit);
            room.Footprint = RectHelper.FloorToIntegerDimensions(room.Footprint);
        }

        //Connect a room in each region to an adjacent room in its sibling region.  This ensures that the Dungeon is fully-connected and there are
        //no isolated rooms.
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

        ScaleUpDungeon(newDungeon, DungeonScaleFactor);
        newDungeon.DebugDraw(20000);
        Debug.Log("Does Dungeon pass check? = " + CheckDungeon(newDungeon));
        return newDungeon;
    }

    //Creates a DungeonRegion with the same size as its parent Dungeon to be the root region.  This region is centered on (0,0) to make scaling
    //things up and down easier.
    public void CreateAndSetRootRegion(DungeonFloor parentDungeon)
    {
        DungeonRegion rootRegion = new DungeonRegion(parentDungeon);
        rootRegion.Footprint = new Rect(-parentDungeon.DungeonSize * .5f, parentDungeon.DungeonSize);
        rootRegion.DepthInTree = 0;
        parentDungeon.RootRegion = rootRegion;
    }

    //Takes a Dungeon object and generates a Unity GameObject to be instantiated in the scene to represent the Dungeon.
    GameObject CreateDungeonGameObject(DungeonFloor dungeon)
    {
        GameObject DungeonGameObject = new GameObject();
        DungeonGameObject.AddComponent<DungeonManagerScript>();
        return DungeonGameObject;
    }

    //Check the Dungeon for integrity.
    public bool CheckDungeon(DungeonFloor dungeon)
    {
        foreach (Room room1 in dungeon.Rooms)
        {
            foreach (Room room2 in dungeon.Rooms)
            {
                if (room1 != room2)
                {
                    if (RectHelper.DoRectsTouchWithinEpsilon(room1.Footprint, room2.Footprint, .01f))
                    {
                        RectHelper.DebugDrawRect(room1.Footprint, Color.red, 20000);
                        RectHelper.DebugDrawRect(room2.Footprint, Color.yellow, 20000);
                        Debug.Log("Rooms overlap");
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

                if (RectHelper.DoRectsTouchWithinEpsilon(room.Footprint, corridor.Footprint, .1f) == false)
                {
                    RectHelper.DebugDrawRect(corridor.Footprint, Color.red, 20000);
                    RectHelper.DebugDrawRect(room.Footprint, Color.magenta, 20000);

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

    //Calculates and generates a corridor that connects two rooms.  Returns true if we successfully make a Corridor, false if we don't for any reason

    //Rooms must be adjacent to each other and the walls must touch for a minimum distance of DungeonCorridorwidth*2 so that the Corridor 
    //can easily be fit along both walls.

    //Corridors are always axis-aligned rectangles.
    bool CreateCorridorBetweenRooms(DungeonFloor dungeon, Room room1, Room room2)
    {
        Vector2 hallwayStartingPoint = room1.Footprint.center;
        Vector2 hallwayDirection = new Vector2();
        Vector2 hallwayEndingPoint = new Vector2();
        DungeonRegion region1 = room1.ContainingRegion;
        DungeonRegion region2 = room2.ContainingRegion;
        
        //If the regions touch in the X axis, we make a horizontal corridor
        if (RectHelper.DoRectsTouchInX(room1.ContainingRegion.Footprint, room2.ContainingRegion.Footprint, .01f))
        {
            //This defines the range of y values that we can place the corridor and have it be along both walls
            float minY = Mathf.Max(room1.Footprint.yMin, room2.Footprint.yMin) + DungeonCorridorWidth;
            float maxY = Mathf.Min(room1.Footprint.yMax, room2.Footprint.yMax) - DungeonCorridorWidth;

            //If rooms don't overlap enough, we cannot make a corridor
            if (maxY - minY < 2)
            {
                return false;
            }

            //Pick a spot that is along both walls to place the corridor
            float yValue = Mathf.Round(Random.Range(minY, maxY));

            if (room1.Footprint.position.x > room2.Footprint.position.x)
            {
                Room oldRoom1 = room1;
                Room oldRoom2 = room2;
                room1 = oldRoom2;
                room2 = oldRoom1;
            }
                hallwayDirection = Vector2.right;
                hallwayStartingPoint = new Vector2(room1.Footprint.xMax, yValue);
                hallwayEndingPoint = new Vector2(room2.Footprint.xMin, yValue);
            
        }
        //If the regions touch in the X axis, we make a horizontal corridor
        else if (RectHelper.DoRectsTouchInY(room1.ContainingRegion.Footprint, room2.ContainingRegion.Footprint, .01f))
        {
            //This defines the range of x values that we can place the corridor and have it be along both walls
            float minX = Mathf.Max(room1.Footprint.xMin, room2.Footprint.xMin) + DungeonCorridorWidth;
            float maxX = Mathf.Min(room1.Footprint.xMax, room2.Footprint.xMax) - DungeonCorridorWidth;

            //If rooms don't overlap enough, we cannot make a corridor
            if (maxX - minX < 2)
            {
                return false;
            }

            //Pick a spot that is along both walls to place the corridor
            float xValue = Mathf.Round(Random.Range(minX, maxX));
            if (room1.Footprint.position.y > room2.Footprint.position.y)
            {
                Room oldRoom1 = room1;
                Room oldRoom2 = room2;
                room1 = oldRoom2;
                room2 = oldRoom1;
            }

                hallwayDirection = Vector2.up;
                hallwayStartingPoint = new Vector2(xValue, room1.Footprint.yMax);
                hallwayEndingPoint = new Vector2(xValue, room2.Footprint.yMin);
        }
        else
        {
            Debug.Log("Rooms are not adjacent and so cannot be connected with a corridor.");
        }

        //Create a Rect defining the footprint of the corridor
        Vector2 hallwayDimensions = hallwayStartingPoint - hallwayEndingPoint;
        hallwayDimensions += Vector2.Perpendicular(hallwayDirection)*DungeonCorridorWidth;
        Rect corridorRect = new Rect(hallwayStartingPoint, hallwayDimensions * -1);

        //If we made our rectangle "backwards" we can end up with negative lengths and maximum values that are smaller than minimum values
        //If so, we correct for this and ensure that all of our rectangles are "forwards"
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

        //We want all of our corridors and rooms to be integer positions and dimensions so that we can easily use a TileMap to draw them
        corridorRect = RectHelper.FloorToIntegerDimensions(corridorRect);
        Corridor newCorridor = new Corridor(room1, room2, corridorRect, hallwayDirection);
        dungeon.Corridors.Add(newCorridor);
        return true;
    }

    //Because CreateCorridorBetweenRooms() is somewhat conservative in its determination of whether it can successfully make a corridor, 
    //it may sometimes be better to make a smaller dungeon and the scale it up to fit you game rather than make a larger dungeon.
    public void ScaleUpDungeon(DungeonFloor dungeon, int scaleFactor)
    {
        if (scaleFactor < 1)
        {
            throw new System.Exception("Scale Factor cannot be less then 1");
        }
        dungeon.RootRegion.Footprint.size *= scaleFactor;
        dungeon.RootRegion.Footprint.position *= scaleFactor;
        foreach (DungeonRegion region in dungeon.AllSubRegions)
        {
            region.Footprint.position *= scaleFactor;
            region.Footprint.size *= scaleFactor;
        }
        foreach (Room room in dungeon.Rooms)
        {
            room.Footprint.position *= scaleFactor;
            room.Footprint.size *= scaleFactor;
        }
        foreach (Corridor corridor in dungeon.Corridors)
        {
            corridor.Footprint.position *= scaleFactor;
            corridor.Footprint.size *= scaleFactor;
        }
    }

    //This splits a DungeonRegion into two sections that perfectly cover the region and sets these two regions as the SubRegions of the region that was
    //split.  Returns "true" if a split successfully occurs, "false" if we do not to split for any reason.
 
    //By recursively splitting DungeonRegions into SubRegions we create Binary Space Partitioning (BSP) tree to map out the dungeon.

    //The split occurs along either the horizontal or vertical axis and ensures that all sides of the region are at least MinimumLengthToSplitRegion
    //long and that the area of the region is at least MinimumAreaToSplitRegion.

    bool SplitRegion(DungeonRegion region)
    {
        //Don't split a region that's already split or that has been marked to be a leaf region.
        if (region.SubRegions != null || region.WillBeLeafRegion)
        {
            return false;
        }

        //If we have split enough times, and the region is small enough, we have a random chance of stopping the splits early to create more variety
        //in final room size
        if (region.DepthInTree > MinimumDepthToStopSplitting && region.Footprint.size.x * region.Footprint.size.y < MaximumAreaForDungeonRoom)
        {
            if (Random.value < ChanceToStopSplittingRoom)
            {
                region.WillBeLeafRegion = true;
                return false;
            }
        }

        //Don't split if a region's area is below the minimum to split
        if (region.Footprint.size.x * region.Footprint.size.y < MinimumAreaToSplitRegion)
        {
            region.WillBeLeafRegion = true;
            return false;
        }
        
        //Check whether any dimension is below the threshold for splitting
        bool canSplitHorizontally = true;
        bool canSplitVertically = true;
        if (region.Footprint.size.x < MinimumLengthForDungeonRegion * 2)
        {
            canSplitHorizontally = false;
        }
        if (region.Footprint.size.y < MinimumLengthForDungeonRegion * 2)
        {
            canSplitVertically = false;
        }

        //If we cannot split horizontally or vertically we cannot split and should mark this region to be a leaf
        if (canSplitHorizontally == false && canSplitVertically == false)
        {
            region.WillBeLeafRegion = true;
            return false;
        }

        bool doSplitHorizontally = true;
        float splitDistance = 0;

        //If we can only split in one dimension, split in that dimension
        if (canSplitHorizontally == true && canSplitVertically == false)
        {
            doSplitHorizontally = true;
        }
        else if (canSplitVertically == true && canSplitHorizontally == false)
        {
            doSplitHorizontally = false;
        }
        //Otherwise flip a coin.  This could be rewritten to bias for certain length:width ratios, potentially.
        else
        {
            doSplitHorizontally = (Random.value < .5f);
        }

        //Pick a spot along the edge of the region to make the split.  This spot has to be at least MinimumLengthForDungeonRegion away from the edge
        if (doSplitHorizontally)
        {
            splitDistance = Random.Range(MinimumLengthForDungeonRegion, region.Footprint.size.x - MinimumLengthForDungeonRegion);
        }
        else
        {
            splitDistance = Random.Range(MinimumLengthForDungeonRegion, region.Footprint.size.y - MinimumLengthForDungeonRegion);
        }

        //Round to integer so that we can use TileMaps to render the dungeon
        splitDistance = Mathf.Round(splitDistance);

        //We shouldn't ever actually get here.
        if (splitDistance <= 0)
        {
            Debug.Log("We are here");
            region.WillBeLeafRegion = true;
            return false;
        }

        //Create new regions based on the split
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


        //If we messed up and either of the regions is too small, throw away the split and mark the region as a leaf.
        if (IsFootprintTooSmallForRegion(newRegion1Footprint) || IsFootprintTooSmallForRegion(newRegion2Footprint))
        {
            region.WillBeLeafRegion = true;
            return false;
        }

        //If we made it this far, we have two good DungeonRegions and we should set them as SubRegions and return true
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

    bool IsFootprintTooSmallForRegion(Rect footprint)
    {
        return (footprint.size.x < MinimumLengthForDungeonRegion || footprint.size.y < MinimumLengthForDungeonRegion
            || footprint.size.x * footprint.size.y < MinimumAreaToSplitRegion);
    }
}