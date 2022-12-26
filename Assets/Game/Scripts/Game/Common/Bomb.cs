using System;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Booster
{
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    public Direction direction;

    protected bool hasMatchingCombo;
    protected bool hasNonMatchingCombo;

    public override List<GameObject> Resolve(GameScene scene, int idx)
    {
        var tiles = new List<GameObject>();
        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        switch (direction)
        {
            case Direction.Horizontal:
            {
                for (var i = 0; i < scene.level.width; i++)
                {
                    AddTile(tiles, scene, i, y);
                }

                var combo = GetCombo(scene, x, y);
                switch (combo)
                {
                    case ComboType.Matching:
                        for (var i = 0; i < scene.level.width; i++)
                        {
                            AddTile(tiles, scene, i, y - 1);
                        }

                        for (var i = 0; i < scene.level.width; i++)
                        {
                            AddTile(tiles, scene, i, y + 1);
                        }

                        hasMatchingCombo = true;
                        break;

                    case ComboType.NonMatching:
                        for (var j = 0; j < scene.level.height; j++)
                        {
                            AddTile(tiles, scene, x, j);
                        }

                        hasNonMatchingCombo = true;
                        break;
                }
            }
                break;

            case Direction.Vertical:
            {
                for (var j = 0; j < scene.level.height; j++)
                {
                    AddTile(tiles, scene, x, j);
                }

                var combo = GetCombo(scene, x, y);
                switch (combo)
                {
                    case ComboType.Matching:
                        for (var j = 0; j < scene.level.height; j++)
                        {
                            AddTile(tiles, scene, x - 1, j);
                        }

                        for (var j = 0; j < scene.level.height; j++)
                        {
                            AddTile(tiles, scene, x + 1, j);
                        }

                        hasMatchingCombo = true;
                        break;

                    case ComboType.NonMatching:
                        for (var i = 0; i < scene.level.width; i++)
                        {
                            AddTile(tiles, scene, i, y);
                        }

                        hasNonMatchingCombo = true;
                        break;
                }
            }
                break;
        }

        return tiles;
    }

    public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        if (hasMatchingCombo)
        {
            if (direction == Direction.Horizontal)
            {
                ShowFx(gamePools, scene, x, y - 1, direction);
                ShowFx(gamePools, scene, x, y, direction);
                ShowFx(gamePools, scene, x, y + 1, direction);
            }
            else if (direction == Direction.Vertical)
            {
                ShowFx(gamePools, scene, x - 1, y, direction);
                ShowFx(gamePools, scene, x, y, direction);
                ShowFx(gamePools, scene, x + 1, y, direction);
            }
        }
        else if (hasNonMatchingCombo)
        {
            ShowFx(gamePools, scene, x, y, Direction.Horizontal);
            ShowFx(gamePools, scene, x, y, Direction.Vertical);
        }
        else
        {
            ShowFx(gamePools, scene, x, y, direction);
        }
    }

    protected virtual void ShowFx(GamePools gamePools, GameScene scene, int x, int y, Direction fxDirection)
    {
        if (!IsValidTile(scene.level, x, y))
        {
            return;
        }

        GameObject particles;
        if (fxDirection == Direction.Horizontal)
        {
            particles = gamePools.horizontalBombParticlesPool.GetObject();
        }
        else
        {
            particles = gamePools.verticalBombParticlesPool.GetObject();
        }

        particles.AddComponent<AutoKillPooled>();
        var tileIndex = x + (y * scene.level.width);
        var hitPos = scene.tilePositions[tileIndex];
        particles.transform.position = hitPos;
        if (fxDirection == Direction.Horizontal)
        {
            var newPos = particles.transform.position;
            newPos.x = scene.levelLocation.position.x;
            particles.transform.position = newPos;
        }
        else
        {
            var newPos = particles.transform.position;
            newPos.y = scene.levelLocation.position.y;
            particles.transform.position = newPos;
        }

        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }
    }

    protected enum ComboType
    {
        None,
        Matching,
        NonMatching
    }

    protected ComboType GetCombo(GameScene scene, int x, int y)
    {
        var up = new TileDef(x, y - 1);
        var down = new TileDef(x, y + 1);
        var left = new TileDef(x - 1, y);
        var right = new TileDef(x + 1, y);

        var matchingCombos = 0;
        var nonMatchingCombos = 0;

        if (IsCombo(scene, up.x, up.y))
        {
            if (IsMatchingCombo(scene, up.x, up.y)) matchingCombos += 1;
            else nonMatchingCombos += 1;
        }

        if (IsCombo(scene, down.x, down.y))
        {
            if (IsMatchingCombo(scene, down.x, down.y)) matchingCombos += 1;
            else nonMatchingCombos += 1;
        }

        if (IsCombo(scene, left.x, left.y))
        {
            if (IsMatchingCombo(scene, left.x, left.y)) matchingCombos += 1;
            else nonMatchingCombos += 1;
        }

        if (IsCombo(scene, right.x, right.y))
        {
            if (IsMatchingCombo(scene, right.x, right.y)) matchingCombos += 1;
            else nonMatchingCombos += 1;
        }

        if (nonMatchingCombos > 0)
        {
            return ComboType.NonMatching;
        }

        if (matchingCombos > 0)
        {
            return ComboType.Matching;
        }

        return ComboType.None;
    }

    protected bool IsCombo(GameScene scene, int x, int y)
    {
        var idx = x + (y * scene.level.width);
        if (IsValidTile(scene.level, x, y) &&
            scene.tileEntities[idx] != null &&
            scene.tileEntities[idx].GetComponent<Bomb>() != null)
        {
            return true;
        }

        return false;
    }

    protected bool IsMatchingCombo(GameScene scene, int x, int y)
    {
        var idx = x + (y * scene.level.width);
        if (IsValidTile(scene.level, x, y) &&
            scene.tileEntities[idx] != null &&
            scene.tileEntities[idx].GetComponent<Bomb>() != null)
        {
            var bomb = scene.tileEntities[idx].GetComponent<Bomb>();
            return bomb.direction == direction;
        }

        return false;
    }
}