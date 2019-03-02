using UnityEngine;

/*
 * This class is a simple GameObject interface to the DungeonGenerator.  This class exists to get parameter input from the inspector and pass it to a 
 * DungeonGenerator.
 * 
 * One of the more persistent confusions I have with Unity architecture is when to make a GameObject, when to make code a MonoBehaviour, etc, so I'm
 * not totally sold on this design.
 * 
 * I wanted to separate the actual Dungeon generation logic from Unity/MonoBehaviour specific things but wanted to be able to access paramenters
 * through the inspector so that's what this class is for.
 */

public class DungeonGenerationScript : MonoBehaviour
{
    [SerializeField]
    Vector2 SizeOfDungeonToGenerate;

    [SerializeField]
    float MinimumAreaToSplitRegion;    //If a DungeonRegions's area is less then this value it will stop splitting into SubRegions during the
                                        //generation process
    [SerializeField]
    float MinimumLengthToSplitRegion;  //If a DungeonRegions's x or y dimension is less then this value it will stop splitting into SubRegions 
                                        //during the generation process
    [SerializeField]
    float ChanceToStopSplittingRoom;    //Once the DungeonRegions have split a certain number of times, they have a percent chance to stop splitting
                                        //even if they are large enough that they could split.  This adds variety in room size
    [SerializeField]
    int MinimumDepthToStopSplitting;    //The number of Splits that need to occur before we check whether we should stop splitting

    [SerializeField]
    float MaximumAreaForDungeonRoom;    //DungeonRegion must be smaller than this value before we start checking whether to randomly stop splitting

    [SerializeField]
    float DungeonCorridorWidth;        

    [SerializeField]
    float MinimumPercentOfRegionForRoom;//After DungeonRegions are defined through splits, leaf regions contain rooms.  Rooms take up a subset of                                       
    [SerializeField]                    //the space in their region.  These variables set the minimum and maximum amount of the region to have the
    float MaximumPercentOfRegionForRoom;//room occupy.

    [SerializeField]                    //An amount to scale the Dungeon after it is generated.  This might in some cases be better than generating a 
    int DungeonScaleFactor;            //larger dungeon.
    public void Start()
    {
        DungeonGenerator generator = new DungeonGenerator(SizeOfDungeonToGenerate, MinimumAreaToSplitRegion, MinimumLengthToSplitRegion,
            ChanceToStopSplittingRoom, MinimumDepthToStopSplitting, MaximumAreaForDungeonRoom, DungeonCorridorWidth, MinimumPercentOfRegionForRoom,
            MaximumPercentOfRegionForRoom, DungeonScaleFactor);
        Dungeon newDungeon = generator.MakeDungeon();
    }


}







