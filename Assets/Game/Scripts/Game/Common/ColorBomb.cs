using System.Collections.Generic;
using UnityEngine;


public class ColorBomb : Booster
{
    public override List<GameObject> Resolve(GameScene scene, int idx)
    {
        var tiles = new List<GameObject>();

        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        var combo = GetCombo(scene, x, y);
        if (combo)
        {
            for (var j = 0; j < scene.level.height; j++)
            {
                for (var i = 0; i < scene.level.width; i++)
                {
                    var tileIndex = i + (j * scene.level.width);
                    var tile = scene.tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var block = tile.GetComponent<Block>();
                        if (block != null &&
                            (block.type == BlockType.Blue ||
                             block.type == BlockType.Green ||
                             block.type == BlockType.Pink ||
                             block.type == BlockType.Purple ||
                             block.type == BlockType.Yellow ||
                             block.type == BlockType.Red))
                        {
                            AddTile(tiles, scene, i, j);
                        }
                    }
                }
            }

            AddTile(tiles, scene, x, y);

            var up = new TileDef(x, y - 1);
            var down = new TileDef(x, y + 1);
            var left = new TileDef(x - 1, y);
            var right = new TileDef(x + 1, y);

            if (IsCombo(scene, up.x, up.y))
            {
                AddTile(tiles, scene, x, y - 1);
            }

            if (IsCombo(scene, down.x, down.y))
            {
                AddTile(tiles, scene, x, y + 1);
            }

            if (IsCombo(scene, left.x, left.y))
            {
                AddTile(tiles, scene, x - 1, y);
            }

            if (IsCombo(scene, right.x, right.y))
            {
                AddTile(tiles, scene, x + 1, y);
            }
        }
        else
        {
            var randomIdx = Random.Range(0, scene.level.availableColors.Count);
            var randomBlock = scene.level.availableColors[randomIdx];
            var randomType = BlockType.Blue;
            switch (randomBlock)
            {
                case ColorBlockType.Blue:
                    randomType = BlockType.Blue;
                    break;
                case ColorBlockType.Green:
                    randomType = BlockType.Green;
                    break;
                case ColorBlockType.Pink:
                    randomType = BlockType.Pink;
                    break;
                case ColorBlockType.Purple:
                    randomType = BlockType.Purple;
                    break;
                case ColorBlockType.Yellow:
                    randomType = BlockType.Yellow;
                    break;
                case ColorBlockType.Red:
                    randomType = BlockType.Red;
                    break;
            }

            for (var j = 0; j < scene.level.height; j++)
            {
                for (var i = 0; i < scene.level.width; i++)
                {
                    var tileIndex = i + (j * scene.level.width);
                    var tile = scene.tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var block = tile.GetComponent<Block>();
                        if (block != null && block.type == randomType)
                        {
                            AddTile(tiles, scene, i, j);
                        }
                    }
                }
            }

            AddTile(tiles, scene, x, y);
        }

        return tiles;
    }

    public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        var particles = gamePools.colorBombParticlesPool.GetObject();
        particles.AddComponent<AutoKillPooled>();
        var tileIndex = x + (y * scene.level.width);
        var hitPos = scene.tilePositions[tileIndex];
        particles.transform.position = hitPos;

        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }
    }

    protected bool GetCombo(GameScene scene, int x, int y)
    {
        var up = new TileDef(x, y - 1);
        var down = new TileDef(x, y + 1);
        var left = new TileDef(x - 1, y);
        var right = new TileDef(x + 1, y);

        if (IsCombo(scene, up.x, up.y) ||
            IsCombo(scene, down.x, down.y) ||
            IsCombo(scene, left.x, left.y) ||
            IsCombo(scene, right.x, right.y))
        {
            return true;
        }

        return false;
    }

    protected bool IsCombo(GameScene scene, int x, int y)
    {
        var idx = x + (y * scene.level.width);
        if (IsValidTile(scene.level, x, y) &&
            scene.tileEntities[idx] != null &&
            scene.tileEntities[idx].GetComponent<ColorBomb>() != null)
        {
            return true;
        }

        return false;
    }
}