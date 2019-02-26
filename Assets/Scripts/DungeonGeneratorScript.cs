﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DungeonGeneratorScript : MonoBehaviour
{
    [SerializeField]
    float MinimumAreaForDungeonRoom;

    [SerializeField]
    float MaximumAreaForDungeonRoom;

    [SerializeField]
    float MinimumLengthForDungeonRoom;

    [SerializeField]
    Vector2 SizeOfDungeonToGenerate;

    [SerializeField]
    float PercentChanceToStopSplittingRoom;

    [SerializeField]
    int MinimumDepthToStopSplitting;

    [SerializeField]
    float DungeonCorridorWidth;

    static void DebugDrawRect(Rect rect, Color color, float duration)
    {
        Vector2[] points = new Vector2[4];
        points[0] = rect.position;
        points[1] = rect.position + Vector2.right * rect.size.x;
        points[2] = rect.position + rect.size;
        points[3] = rect.position + Vector2.up * rect.size.y;

        Debug.DrawLine(points[0], points[1], color, duration);
        Debug.DrawLine(points[1], points[2], color, duration);
        Debug.DrawLine(points[2], points[3], color, duration);
        Debug.DrawLine(points[3], points[0], color, duration);
    }

    static public bool DoRectsTouchInX(Rect rect1, Rect rect2, float epsilon)
    {
        rect1.size += Vector2.right * epsilon;
        rect2.size += Vector2.right * epsilon;
        rect1.position -= Vector2.right * epsilon * .5f;
        rect2.position -= Vector2.right * epsilon * .5f;
        return (rect1.Overlaps(rect2) || rect2.Overlaps(rect1));
    }

    static public bool DoRectsTouchInY(Rect rect1, Rect rect2, float epsilon)
    {
        rect1.size += Vector2.up * epsilon;
        rect2.size += Vector2.up * epsilon;
        rect1.position -= Vector2.up * epsilon * .5f;
        rect2.position -= Vector2.up * epsilon * .5f;
        return (rect1.Overlaps(rect2) || rect2.Overlaps(rect1));
    }

    static public bool DoRectsTouchWithinEpsilon(Rect rect1, Rect rect2, float epsilon)
    {
        return DoRectsTouchInX(rect1, rect2, epsilon) || DoRectsTouchInY(rect1, rect2, epsilon);
    }

    public class Dungeon
    {
        public DungeonRegion RootRegion;
        List<DungeonCorridor> Corridors;
        public List<DungeonRegion> AllSubRegions;
        public List<DungeonRoom> Rooms;
        Vector2 DungeonSize;
        float MinimumRoomArea;
        float MinimumRoomLength;
        float MaximumRoomArea;
        float ChanceToStopSplitting;
        int MinimumTreeDepthToStopSplitting;
        float CorridorWidth;


        public Dungeon(Vector2 size, float minArea, float maxArea, float minLength, float chanceToStopSplit, int minDepth, float corridorWidth)
        {
            this.DungeonSize = size;
            this.AllSubRegions = new List<DungeonRegion>();
            this.MinimumRoomArea = minArea;
            this.MaximumRoomArea = maxArea;
            this.MinimumRoomLength = minLength;
            this.ChanceToStopSplitting = chanceToStopSplit;
            this.MinimumTreeDepthToStopSplitting = minDepth;
            this.Corridors = new List<DungeonCorridor>();
            this.Rooms = new List<DungeonRoom>();
            this.CorridorWidth = corridorWidth;
            this.RootRegion = DungeonRegion.CreateRootRegion(this.DungeonSize, this.MinimumRoomArea, this.MinimumRoomLength,
                                                             this.MaximumRoomArea, this.ChanceToStopSplitting, this.MinimumTreeDepthToStopSplitting,
                                                             this);
        }

        public bool CreateCorridorBetweenRooms(DungeonRoom room1, DungeonRoom room2)
        {
            Vector2 hallwayStartingPoint = room1.roomFootprint.center;
            Vector2 hallwayDirection = new Vector2();
            Vector2 hallwayEndingPoint = new Vector2();
            DungeonRegion region1 = room1.ContainingRegion;
            DungeonRegion region2 = room2.ContainingRegion;
            Color color = Color.grey;

            if (DoRectsTouchInX(room1.ContainingRegion.RegionFootprint, room2.ContainingRegion.RegionFootprint, .01f))
            {
                float minY = Mathf.Max(room1.roomFootprint.yMin, room2.roomFootprint.yMin+1.5f);
                float maxY = Mathf.Min(room1.roomFootprint.yMax, room2.roomFootprint.yMax-1.5f);
                if (maxY-minY < 2)
                {
                    Debug.Log("too close to connect");
                    return false;
                }
                float yValue = Mathf.Round(Random.Range(minY, maxY));
          
                if (room1.roomFootprint.position.x < room2.roomFootprint.position.x)
                {
                    hallwayDirection = Vector2.right;
                    hallwayStartingPoint = new Vector2(room1.roomFootprint.xMax, yValue);
                    hallwayEndingPoint = new Vector2(room2.roomFootprint.xMin, yValue);
                    color = Color.green;
                }
                else
                {
                    hallwayDirection = Vector2.left;
                    hallwayStartingPoint = new Vector2(room1.roomFootprint.xMin, yValue);
                    hallwayEndingPoint = new Vector2(room2.roomFootprint.xMax, yValue);
                    color = Color.blue;
                }
            }
            else if (DoRectsTouchInY(room1.ContainingRegion.RegionFootprint, room2.ContainingRegion.RegionFootprint, .01f))
            {
                float minX = Mathf.Max(room1.roomFootprint.xMin, room2.roomFootprint.xMin+1.5f);
                float maxX = Mathf.Min(room1.roomFootprint.xMax, room2.roomFootprint.xMax-1.5f);
                if (maxX - minX < 2)
                {
                    Debug.Log("too close to connect");
                    return false;
                }
                float xValue = Mathf.Round(Random.Range(minX, maxX));
                if (room1.roomFootprint.position.y < room2.roomFootprint.position.y)
                {
                    hallwayDirection = Vector2.up;
                    hallwayStartingPoint = new Vector2(xValue, room1.roomFootprint.yMax);
                    hallwayEndingPoint = new Vector2(xValue, room2.roomFootprint.yMin);
                    color = Color.green;
                }
                else
                {
                    hallwayDirection = Vector2.down;
                    hallwayStartingPoint = new Vector2(xValue, room1.roomFootprint.yMin);
                    hallwayEndingPoint = new Vector2(xValue, room2.roomFootprint.yMax);
                    color = Color.blue;
                }
            }
            else
            {
                Debug.Log("Rooms are not adjacent and so cannot be connected with a corridor.");
                Debug.DrawLine(hallwayStartingPoint, hallwayEndingPoint, Color.yellow, 20000);
            }

            Vector2 hallwayDimensions =  hallwayStartingPoint - hallwayEndingPoint;
            hallwayDimensions += Vector2.Perpendicular(hallwayDirection);
            Rect corridorRect = new Rect(hallwayStartingPoint, hallwayDimensions*-1);
            DungeonCorridor newCorridor = new DungeonCorridor(room1, room2, corridorRect);
            DebugDrawRect(corridorRect, Color.yellow, 20000);
            return true;
        }

        public void ScaleUpDungeon(float scaleFactor)
        {
            throw new System.Exception("Not written yet");
        }

        public void DebugDrawDungeon()
        {
            throw new System.Exception("Not written yet");
        }
    }

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

        DungeonRegion()
        {
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
            depthCount++;
            if (depthCount > 1000)
            {
                return null;
            }
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

        public static DungeonRegion CreateRootRegion(Vector2 dungeonSize, float minimumRoomArea, float minimumRoomLength, float maximumRoomArea,
                                              float chanceToStopSplitting, int minimumTreeDepthToStopSplitting, Dungeon parentDungeon)
        {
            DungeonRegion rootRegion = new DungeonRegion();
            rootRegion.ParentDungeon = parentDungeon;
            rootRegion.RegionFootprint = new Rect(-dungeonSize * .5f, dungeonSize);
            rootRegion.MinimumRoomArea = minimumRoomArea;
            rootRegion.MinimumRoomLength = minimumRoomLength;
            rootRegion.MaximumRoomArea = maximumRoomArea;
            rootRegion.ChanceToStopSplitting = chanceToStopSplitting;
            rootRegion.MinimumTreeDepthToStopSplitting = minimumTreeDepthToStopSplitting;
            rootRegion.treeDepth = 0;
            rootRegion.IsRoomRegion = false;
            rootRegion.MySiblingRegion = null;
            return rootRegion;
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

            Vector2 newRegionSize1;
            Vector2 newRegionSize2;
            Vector2 newRegionOrigin1;
            Vector2 newRegionOrigin2;

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
            if (footprint.size.x < MinimumRoomLength || footprint.size.y < MinimumRoomLength || footprint.size.x * footprint.size.y < MinimumRoomArea)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            room1.Corridors.Add(this);
            room2.Corridors.Add(this);            
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
            if (this.AdjacentRooms.Contains(roomToTest) || roomToTest.AdjacentRooms.Contains(this))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public void Start()
    {
        Dungeon newDungeon = new Dungeon(SizeOfDungeonToGenerate, MinimumAreaForDungeonRoom, MaximumAreaForDungeonRoom, MinimumLengthForDungeonRoom,
                                        PercentChanceToStopSplittingRoom, MinimumDepthToStopSplitting, DungeonCorridorWidth);
        bool splitARegionThisIteration = true;
        int loopCounter = 0;
        while (splitARegionThisIteration && loopCounter < 100)
        {
            loopCounter++;
            splitARegionThisIteration = false;
            List<DungeonRegion> currentLeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
            foreach (DungeonRegion dungeonRegion in currentLeafRegions)
            {
                if (dungeonRegion.Split())
                {
                    splitARegionThisIteration = true;
                }
            }
        }

        List<DungeonRegion> LeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
        foreach (DungeonRegion dungeonRegion in LeafRegions)
        {
            DungeonRoom newRoom = new DungeonRoom(dungeonRegion);
            newDungeon.Rooms.Add(newRoom);
            dungeonRegion.SetRoom(newRoom);
        }

        foreach (DungeonRoom room in newDungeon.Rooms)
        {
            foreach (DungeonRoom room2 in newDungeon.Rooms)
            {
                if (room != room2)
                {
                    if (DoRectsTouchWithinEpsilon(room.roomFootprint, room2.roomFootprint, .01f))
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
        foreach (DungeonRoom room in newDungeon.Rooms)
        {
            room.roomFootprint.position *= 1.1f;
            DebugDrawRect(room.roomFootprint, Color.green, 2000);
        }

        //Connect a room in each region to an adjacent room in its sibling region.
        foreach (DungeonRegion subRegion in newDungeon.AllSubRegions)
        {
            DungeonRegion siblingRegion = subRegion.MySiblingRegion;
            List<DungeonRoom> siblingRooms = siblingRegion.GetAllRoomsInRegion();
            bool madeConnection = false;

            foreach (DungeonRoom room in subRegion.GetAllRoomsInRegion())
            {
                foreach (DungeonRoom siblingRoom in siblingRegion.GetAllRoomsInRegion())
                {
                    if (room.AmIAdjacentTo(siblingRoom) && madeConnection == false && room.AmIConnectedByACorridorTo(siblingRoom) == false)
                    {
                        if (newDungeon.CreateCorridorBetweenRooms(room, siblingRoom))
                        {
                            madeConnection = true;
                        }
                    }
                }
            }
        }
    }
}


