using System.Collections.Generic;
using UnityEngine;


public class Dynamite : Booster
{
    public override List<GameObject> Resolve(GameScene scene, int idx)
    {
        var tiles = new List<GameObject>();
        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        AddTile(tiles, scene, x - 1, y - 1);
        AddTile(tiles, scene, x, y - 1);
        AddTile(tiles, scene, x + 1, y - 1);
        AddTile(tiles, scene, x - 1, y);
        AddTile(tiles, scene, x, y);
        AddTile(tiles, scene, x + 1, y);
        AddTile(tiles, scene, x - 1, y + 1);
        AddTile(tiles, scene, x, y + 1);
        AddTile(tiles, scene, x + 1, y + 1);

        var combo = GetCombo(scene, x, y);
        if (combo)
        {
            AddTile(tiles, scene, x - 2, y - 2);
            AddTile(tiles, scene, x - 1, y - 2);
            AddTile(tiles, scene, x, y - 2);
            AddTile(tiles, scene, x + 1, y - 2);
            AddTile(tiles, scene, x + 2, y - 2);
            AddTile(tiles, scene, x - 2, y - 1);
            AddTile(tiles, scene, x + 2, y - 1);
            AddTile(tiles, scene, x - 2, y);
            AddTile(tiles, scene, x + 2, y);
            AddTile(tiles, scene, x - 2, y + 1);
            AddTile(tiles, scene, x + 2, y + 1);
            AddTile(tiles, scene, x - 2, y + 2);
            AddTile(tiles, scene, x - 1, y + 2);
            AddTile(tiles, scene, x, y + 2);
            AddTile(tiles, scene, x + 1, y + 2);
            AddTile(tiles, scene, x + 2, y + 2);
        }

        return tiles;
    }

    public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
    {
        var x = idx % scene.level.width;
        var y = idx / scene.level.width;
        var particles = gamePools.dynamiteParticlesPool.GetObject();
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
            scene.tileEntities[idx].GetComponent<Dynamite>() != null)
        {
            return true;
        }

        return false;
    }
}