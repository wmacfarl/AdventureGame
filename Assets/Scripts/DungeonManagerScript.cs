using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManagerScript : MonoBehaviour
{
    public GameObject RoomPrefab;
    public float minimumWidthToHeightRatio = .25f;
    List<DungeonRegion> DungeonRegions;
    public Vector2 minimumRegionSize;
    public Vector2 DungeonSize;
    public float minimumRegionAreaToSplit;
    public float maximumRegionArea;
    public float chanceToStopSplitting;


    public class DungeonRegion
    {
        public bool doneSplitting;
        public Vector2 position;
        Vector2 minimumRegionSize;
        public Vector2 dimensions;
        float minimumWidthToHeightRatio = .25f;

        public DungeonRegion Region1;
        public DungeonRegion Region2;

        public DungeonRegion(Vector2 position, Vector2 dimensions, float minimumWidthToHeightRatio, Vector2 minimumRegionSize)
        {
            this.minimumWidthToHeightRatio = minimumWidthToHeightRatio;
            this.position = position;
            this.dimensions = dimensions;
            this.minimumRegionSize = minimumRegionSize;
            this.doneSplitting = false;
        }

        public bool Split()
        {
            if (doneSplitting)
            {
                return false;
            }
            if (Region1 != null || Region2 != null)
            {
                return false;
            }

            bool CanSplitHorizontally = true;
            if (this.dimensions.x < minimumRegionSize.x)
            {
                CanSplitHorizontally = false;
            }
            bool CanSplitVertically = true;
            if (this.dimensions.y < minimumRegionSize.y)
            {
                CanSplitVertically = false;
            }

            if (CanSplitHorizontally == false && CanSplitVertically == false)
            {
                return false;
            }

            bool ShouldSplitHorizontally = CanSplitHorizontally;
            if (CanSplitHorizontally == false && CanSplitVertically == true)
            {
                ShouldSplitHorizontally = false;
            }else if (CanSplitHorizontally == true && CanSplitVertically == false)
            {
                ShouldSplitHorizontally = false;
            }
            else {
                if (dimensions.x > dimensions.y)
                {
                    ShouldSplitHorizontally = (Random.value > .3f);
                }
                else
                {
                    ShouldSplitHorizontally = (Random.value < .3f);

                }
            }

            if (ShouldSplitHorizontally)
            {
                float size1 = Random.Range(minimumRegionSize.x, dimensions.x - minimumRegionSize.x);
                float size2 = dimensions.x - size1;
                if (size1 < minimumRegionSize.x || size2 < minimumRegionSize.x)
                {
                    return false;
                }
                Region1 = new DungeonRegion(position, new Vector2(size1, dimensions.y), minimumWidthToHeightRatio, minimumRegionSize);
                Region2 = new DungeonRegion(position + new Vector2(size1, 0), new Vector2(size2, dimensions.y), 
                                            minimumWidthToHeightRatio, minimumRegionSize);
            }
            else
            {
                float size1 = Random.Range(minimumRegionSize.y, dimensions.y - minimumRegionSize.y);
                float size2 = dimensions.y - size1;
                if (size1 < minimumRegionSize.y || size2 < minimumRegionSize.y)
                {
                    return false;
                }
                Debug.Log("Negative Size!");
                Region1 = new DungeonRegion(position, new Vector2(dimensions.x, size1), minimumWidthToHeightRatio, minimumRegionSize);
                Region2 = new DungeonRegion(position + new Vector2(0, size1), new Vector2(dimensions.x, size2),
                                            minimumWidthToHeightRatio, minimumRegionSize);
            }

            return true;
        }
    }

    private void Start()
    {
        DungeonRegions = new List<DungeonRegion>();
        DungeonRegion rootRegion = new DungeonRegion(new Vector2(), DungeonSize, minimumWidthToHeightRatio, minimumRegionSize);
        DungeonRegions.Add(rootRegion);
        bool successfullySplit = true;
        int loopCounter = 0;
        while (successfullySplit && loopCounter < 15)
        {
            successfullySplit = false;
            loopCounter++;
           
                List<DungeonRegion> regionsToAdd = new List<DungeonRegion>();
                foreach (DungeonRegion region in DungeonRegions)
                {
                if (region.doneSplitting == false)
                {
                    if (region.Region1 == null && region.Region2 == null)
                    {
                        if (region.dimensions.x * region.dimensions.y > minimumRegionAreaToSplit)
                        {
                            if (Random.value < chanceToStopSplitting && region.dimensions.x*region.dimensions.y < maximumRegionArea)
                            {
                                region.doneSplitting = true;
                            }
                            if (region.Split())
                            {
                                successfullySplit = true;
                                regionsToAdd.Add(region.Region1);
                                regionsToAdd.Add(region.Region2);
                            }
                        }
                    }
                }               
            }
            DungeonRegions.AddRange(regionsToAdd);
        }

        GenerateDungeonGameObjects();
    }

    void GenerateDungeonGameObjects()
    {
        foreach (DungeonRegion region in DungeonRegions)
        {
            if (region.Region1 == null && region.Region2 == null)
            {
                GameObject newRoom = GameObject.Instantiate(RoomPrefab);
                newRoom.transform.position = region.position;
                newRoom.transform.localScale = region.dimensions*Random.Range(.6f,.9f);
            }else if (region.Region1 != null && region.Region2 == null)
            {
                Debug.Log("only region2 is null!");
            }
        else if (region.Region2 != null && region.Region1 == null)
        {
            Debug.Log("only region1 is null!");
        }
    }

}
}

