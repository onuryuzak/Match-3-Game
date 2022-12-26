using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public abstract class TileScoreOverride
{
    public int score;

#if UNITY_EDITOR
    public virtual void Draw()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Score");
        score = EditorGUILayout.IntField(score, GUILayout.Width(30));
        GUILayout.EndHorizontal();
    }
#endif
}

public class BlockScore : TileScoreOverride
{
    public BlockType type;

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Type");
        type = (BlockType)EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        base.Draw();

        GUILayout.EndVertical();
    }
#endif

    public override string ToString()
    {
        return string.Format("{0}: {1}", type, score);
    }
}


public class BoosterScore : TileScoreOverride
{
    public BoosterType type;

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Type");
        type = (BoosterType)EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        base.Draw();

        GUILayout.EndVertical();
    }
#endif

    public override string ToString()
    {
        return string.Format("{0}: {1}", type, score);
    }
}


[Serializable]
public class GameConfiguration
{

    public int defaultTileScore;
    public List<TileScoreOverride> tileScoreOverrides = new List<TileScoreOverride>();

    public Dictionary<BoosterType, int> boosterNeededMatches = new Dictionary<BoosterType, int>();

    public Dictionary<BoosterType, int> ingameBoosterAmount = new Dictionary<BoosterType, int>();

    public float defaultZoomLevel;
    public float defaultCanvasScalingMatch;
    public List<ResolutionOverride> resolutionOverrides = new List<ResolutionOverride>();

    /// <summary>
    /// Constructor.
    /// </summary>
    public GameConfiguration()
    {
        boosterNeededMatches.Add(BoosterType.HorizontalBomb, 5);
        boosterNeededMatches.Add(BoosterType.VerticalBomb, 5);
        boosterNeededMatches.Add(BoosterType.Dynamite, 6);
        boosterNeededMatches.Add(BoosterType.ColorBomb, 7);

        ingameBoosterAmount.Add(BoosterType.HorizontalBomb, 1);
        ingameBoosterAmount.Add(BoosterType.VerticalBomb, 1);
        ingameBoosterAmount.Add(BoosterType.Dynamite, 1);
        ingameBoosterAmount.Add(BoosterType.ColorBomb, 1);
    }


    public int GetScore(TileEntity tile)
    {
        if (tile is Block)
        {
            var scores = tileScoreOverrides.FindAll(x => x is BlockScore);
            foreach (var score in scores)
            {
                var blockScore = score as BlockScore;
                if (blockScore != null && blockScore.type == ((Block)tile).type)
                {
                    return blockScore.score;
                }
            }
        }
        else if (tile is Booster)
        {
            var scores = tileScoreOverrides.FindAll(x => x is BoosterScore);
            foreach (var score in scores)
            {
                var boosterScore = score as BoosterScore;
                if (boosterScore != null && boosterScore.type == ((Booster)tile).type)
                {
                    return boosterScore.score;
                }
            }
        }

        return defaultTileScore;
    }


    public float GetZoomLevel()
    {
        var zoomLevel = defaultZoomLevel;
        foreach (var resolution in resolutionOverrides)
        {
            if (resolution.width == Screen.width && resolution.height == Screen.height)
            {
                zoomLevel = resolution.zoomLevel;
                break;
            }
        }

        return zoomLevel;
    }
}