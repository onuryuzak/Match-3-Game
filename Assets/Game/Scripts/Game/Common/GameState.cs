using System;
using System.Collections.Generic;


public class GameState
{
    public int score;
    public Dictionary<BlockType, int> collectedBlocks = new Dictionary<BlockType, int>();


    public void Reset()
    {
        score = 0;
        collectedBlocks.Clear();
        foreach (var value in Enum.GetValues(typeof(BlockType)))
        {
            collectedBlocks.Add((BlockType)value, 0);
        }
    }
}