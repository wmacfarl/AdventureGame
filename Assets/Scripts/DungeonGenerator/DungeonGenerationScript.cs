using System.Collections.Generic;
using UnityEngine;


public class DungeonGenerationScript : MonoBehaviour
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
    float ChanceToStopSplittingRoom;

    [SerializeField]
    int MinimumDepthToStopSplitting;

    [SerializeField]
    float DungeonCorridorWidth;

    [SerializeField]
    float MinimumPercentOfRegionForRoom;

    [SerializeField]
    float MaximumPercentOfRegionForRoom;

    public void Start()
    {
        DungeonGenerator generator = new DungeonGenerator(MinimumAreaForDungeonRoom, MaximumAreaForDungeonRoom, MinimumLengthForDungeonRoom,
            SizeOfDungeonToGenerate, ChanceToStopSplittingRoom, MinimumDepthToStopSplitting, DungeonCorridorWidth, MinimumPercentOfRegionForRoom,
            MaximumPercentOfRegionForRoom);
        Dungeon newDungeon = generator.MakeDungeon();
    }


}







