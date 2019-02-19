using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DungeonManagerScript : MonoBehaviour
{
    public GameObject RoomPrefab;
    public GameObject CorridorPrefab;
    public float minimumWidthToHeightRatio = .25f;
    List<DungeonRegion> DungeonRegions;
    public Vector2 minimumRegionSize;
    public Vector2 DungeonSize;
    List<GameObject> DungeonRooms;
    List<GameObject> DungeonCorridors;
    public float minimumRegionAreaToSplit;
    public float maximumRegionArea;
    public float chanceToStopSplitting;
    
    public class DungeonCorridor
    {
        DungeonRoom room1;
        DungeonRoom room2;
        Rect xCorridorFootprint;
        Rect yCorridorFootprint;
    }

    public class DungeonRoom
    {
        List<DungeonCorridor> corridors;
        DungeonRegion containingRegion;
        Rect roomFootprint;
    }

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
                Region1 = new DungeonRegion(position, new Vector2(dimensions.x, size1), minimumWidthToHeightRatio, minimumRegionSize);
                Region2 = new DungeonRegion(position + new Vector2(0, size1), new Vector2(dimensions.x, size2),
                                            minimumWidthToHeightRatio, minimumRegionSize);
            }

            return true;
        }
    }

    private void Start()
    {
        DungeonCorridors = new List<GameObject>();
        DungeonRooms = new List<GameObject>();
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
        GameObject roomsGameobject = GameObject.Instantiate(new GameObject());
        roomsGameobject.name = "Rooms";
        foreach (DungeonRegion region in DungeonRegions)
        {
            if (region.Region1 == null && region.Region2 == null)
            {
                Vector2 dimensionsToSubtract = new Vector2(region.dimensions.x * Random.Range(.1f, .4f), region.dimensions.y * Random.Range(.1f, .4f));
                GameObject newRoom = GameObject.Instantiate(RoomPrefab);
                newRoom.transform.position = region.position+region.dimensions*.5f;
                BoxCollider2D collider = newRoom.GetComponent<BoxCollider2D>();
                collider.size = region.dimensions - dimensionsToSubtract;
                collider.size = RoundVectorComponents(collider.size);
                newRoom.transform.position = RoundVectorComponents(newRoom.transform.position);
                newRoom.transform.parent = roomsGameobject.transform;
                newRoom.GetComponent<SpriteRenderer>().size = collider.size;
                region.dimensions -= dimensionsToSubtract;
                region.position += dimensionsToSubtract * .5f;

                DungeonRooms.Add(newRoom);
            }
            else
            {
                GameObject corridor = GenerateCorridorBetweenRegions(region.Region1, region.Region2);
//                DungeonCorridors.Add(corridor);
            }
        }
    }

    Vector2 RoundVectorComponents(Vector2 vector)
    {
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    Vector3 RoundVectorComponents(Vector3 vector)
    {
        return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    GameObject GenerateCorridorBetweenRegions(DungeonRegion region1, DungeonRegion region2)
    {
        Vector2 corridorStartingPoint1 = region1.position + .5f * region1.dimensions;
        if (region1.Region1 != null)
        {
            if (Random.value > .5f)
            {
                corridorStartingPoint1 = region1.Region1.position + .5f * region1.Region1.dimensions;
            }
            else
            {
                corridorStartingPoint1 = region1.Region2.position + .5f * region1.Region2.dimensions;
            }
        }


        Vector2 corridorStartingPoint2 = region2.position + .5f * region2.dimensions;
        if (region2.Region1 != null)
        {
            if (Random.value > .5f)
            {
                corridorStartingPoint2 = region2.Region1.position + .5f * region2.Region1.dimensions;
            }
            else
            {
                corridorStartingPoint2 = region2.Region2.position + .5f * region2.Region2.dimensions;
            }
        }

        Debug.DrawLine(corridorStartingPoint1, corridorStartingPoint2, Color.green, 2000);

        GameObject newCorridor = GameObject.Instantiate(CorridorPrefab);
        BoxCollider2D boxCollider = newCorridor.GetComponent<BoxCollider2D>();
        newCorridor.transform.position = (corridorStartingPoint1 + corridorStartingPoint2) / 2;
        boxCollider.size = corridorStartingPoint2 - corridorStartingPoint1;
        if (boxCollider.size.x < 1)
        {
            boxCollider.size = new Vector2(1, boxCollider.size.y);
        }
        if (boxCollider.size.y < 1)
        {
            boxCollider.size = new Vector2(boxCollider.size.x, 1);
        }

        newCorridor.transform.position = RoundVectorComponents(newCorridor.transform.position);
        boxCollider.size = RoundVectorComponents(boxCollider.size);
        newCorridor.GetComponent<SpriteRenderer>().size = boxCollider.size;
        return newCorridor;
    }


}

