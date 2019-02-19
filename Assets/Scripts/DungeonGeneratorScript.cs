using System.Collections;
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

    public class Dungeon
    {
        public DungeonRegion RootRegion;
        List<DungeonCorridor> Corridors;
        List<DungeonRoom> Rooms;
        Vector2 DungeonSize;
        float MinimumRoomArea;
        float MinimumRoomLength;
        float MaximumRoomArea;
        float ChanceToStopSplitting;
        int MinimumTreeDepthToStopSplitting;


        public Dungeon(Vector2 size, float minArea, float maxArea, float minLength, float chanceToStopSplit, int minDepth)
        {
            this.DungeonSize = size;
            this.MinimumRoomArea = minArea;
            this.MaximumRoomArea = maxArea;
            this.MinimumRoomLength = minLength;
            this.ChanceToStopSplitting = chanceToStopSplit;
            this.MinimumTreeDepthToStopSplitting = minDepth;
            this.Corridors = new List<DungeonCorridor>();
            this.Rooms = new List<DungeonRoom>();
            this.RootRegion = DungeonRegion.CreateRootRegion(this.DungeonSize, this.MinimumRoomArea, this.MinimumRoomLength, 
                this.MaximumRoomArea, this.ChanceToStopSplitting, this.MinimumTreeDepthToStopSplitting);
        }
    }

    public class DungeonRegion
    {
        float MinimumRoomArea;
        float MinimumRoomLength;
        float MaximumRoomArea;
        float ChanceToStopSplitting;
        int MinimumTreeDepthToStopSplitting;
        bool IsRoomRegion;
        public Rect RegionFootprint;
        int treeDepth;

        DungeonRegion[] SubRegions;

        DungeonRegion()
        {
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

        DungeonRegion(Rect regionFootproint, DungeonRegion parentRegion){
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
                                              float chanceToStopSplitting, int minimumTreeDepthToStopSplitting)
        {
            DungeonRegion rootRegion = new DungeonRegion();
            rootRegion.RegionFootprint = new Rect(-dungeonSize * .5f, dungeonSize);
            rootRegion.MinimumRoomArea = minimumRoomArea;
            rootRegion.MinimumRoomLength = minimumRoomLength;
            rootRegion.MaximumRoomArea = maximumRoomArea;
            rootRegion.ChanceToStopSplitting = chanceToStopSplitting;
            rootRegion.MinimumTreeDepthToStopSplitting = minimumTreeDepthToStopSplitting;
            rootRegion.treeDepth = 0;
            rootRegion.IsRoomRegion = false;
            return rootRegion;
        }

        public bool Split()
        {
            if (this.SubRegions != null || this.IsRoomRegion)
            {
                return false;
            }
            if(this.treeDepth > this.MinimumTreeDepthToStopSplitting && this.RegionFootprint.size.x*this.RegionFootprint.size.y < this.MaximumRoomArea)
            {
                if (Random.value < this.ChanceToStopSplitting)
                {
                    Debug.Log("setting to roomRegion based on chanceToStopSplitting");
                    this.IsRoomRegion = true;
                    return false;
                }
            }

            if (this.RegionFootprint.size.x*RegionFootprint.size.y < this.MinimumRoomArea)
            {
                Debug.Log("setting to roomRegion based on minimumArea");
                IsRoomRegion = true;
                return false;
            }

            bool canSplitHorizontally = true;
            bool canSplitVertically = true;

            if (this.RegionFootprint.size.x < this.MinimumRoomLength*2)
            {
                canSplitHorizontally = false;
            }
            if (this.RegionFootprint.size.y < this.MinimumRoomLength*2)
            {
                canSplitVertically = false;
            }

            bool doSplitHorizontally = true;
            float splitDistance = 0;

            if (canSplitHorizontally == false && canSplitVertically == false)
            {
                Debug.Log("Can't split horizontally or vertically");              
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

            if (splitDistance <= 0)
            {
                Debug.Log("split distance is negative");
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
                 newRegionSize2 = new Vector2(this.RegionFootprint.size.x-splitDistance, RegionFootprint.size.y);
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
                Debug.Log("A region is too small");
                IsRoomRegion = true;
                return false;
            }

            SubRegions = new DungeonRegion[2];
            SubRegions[0] = new DungeonRegion(newRegion1Footprint, this);
            SubRegions[1] = new DungeonRegion(newRegion2Footprint, this);
            return true;
        }

        bool IsFootprintTooSmall(Rect footprint)
        {
           if (footprint.size.x < MinimumRoomLength || footprint.size.y < MinimumRoomLength || footprint.size.x*footprint.size.y < MinimumRoomArea)
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
        DungeonRoom[] ConnectedRooms;
        List<DungeonCorridor> ConnectedCorridors;
        Rect xCorridorFootprint;
        Rect yCorridorFootprint;
    }

    public class DungeonRoom
    {
        List<DungeonCorridor> corridors;
        DungeonRegion containingRegion;
        Rect roomFootprint;
    }


    void Start()
    {
        Dungeon newDungeon = new Dungeon(SizeOfDungeonToGenerate, MinimumAreaForDungeonRoom, MaximumAreaForDungeonRoom, MinimumLengthForDungeonRoom,
                                        PercentChanceToStopSplittingRoom, MinimumDepthToStopSplitting);
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
        Debug.Log("LoopCounter = " + loopCounter);

        List<DungeonRegion> LeafRegions = newDungeon.RootRegion.GetLeafRegions(0);
        foreach (DungeonRegion dungeonRegion in LeafRegions)
        {
            DebugDrawRect(dungeonRegion.RegionFootprint, Color.green, 2000);
        }
    }

    void DebugDrawRect(Rect rect, Color color, float duration)
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

}
