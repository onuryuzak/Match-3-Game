using System.Collections.Generic;


public enum LimitType
{
    Moves
}

public class Level
{
    public int id;

    public int width;
    public int height;
    public List<LevelTile> tiles = new List<LevelTile>();

    public LimitType limitType;
    public int moveLimit;
    public List<ColorBlockType> availableColors = new List<ColorBlockType>();

    public int spriteATresHold;
    public int spriteBTresHold;
    public int spriteCTresHold;
    
    public int score1;
    public int score2;
    public int score3;

    public bool awardBoostersWithRemainingMoves;
    public BoosterType awardedBoosterType;
}