using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class GamePools : MonoBehaviour
{
    public ObjectPool block1Pool;
    public ObjectPool block2Pool;
    public ObjectPool block3Pool;
    public ObjectPool block4Pool;
    public ObjectPool block5Pool;
    public ObjectPool block6Pool;
    public ObjectPool emptyTilePool;

    public ObjectPool horizontalBombPool;
    public ObjectPool verticalBombPool;
    public ObjectPool dynamitePool;
    public ObjectPool colorBombPool;

    public ObjectPool block1ParticlesPool;
    public ObjectPool block2ParticlesPool;
    public ObjectPool block3ParticlesPool;
    public ObjectPool block4ParticlesPool;
    public ObjectPool block5ParticlesPool;
    public ObjectPool block6ParticlesPool;
    public ObjectPool boosterSpawnParticlesPool;
    public ObjectPool horizontalBombParticlesPool;
    public ObjectPool verticalBombParticlesPool;
    public ObjectPool dynamiteParticlesPool;
    public ObjectPool colorBombParticlesPool;

    private readonly List<ObjectPool> blockPools = new List<ObjectPool>();


    private void Awake()
    {
        Assert.IsNotNull(block1Pool);
        Assert.IsNotNull(block2Pool);
        Assert.IsNotNull(block3Pool);
        Assert.IsNotNull(block4Pool);
        Assert.IsNotNull(block5Pool);
        Assert.IsNotNull(block6Pool);
        Assert.IsNotNull(emptyTilePool);
        Assert.IsNotNull(horizontalBombPool);
        Assert.IsNotNull(verticalBombPool);
        Assert.IsNotNull(dynamitePool);
        Assert.IsNotNull(colorBombPool);
        Assert.IsNotNull(block1ParticlesPool);
        Assert.IsNotNull(block2ParticlesPool);
        Assert.IsNotNull(block3ParticlesPool);
        Assert.IsNotNull(block4ParticlesPool);
        Assert.IsNotNull(block5ParticlesPool);
        Assert.IsNotNull(block6ParticlesPool);
        Assert.IsNotNull(boosterSpawnParticlesPool);
        Assert.IsNotNull(horizontalBombParticlesPool);
        Assert.IsNotNull(verticalBombParticlesPool);
        Assert.IsNotNull(dynamiteParticlesPool);
        Assert.IsNotNull(colorBombParticlesPool);

        blockPools.Add(block1Pool);
        blockPools.Add(block2Pool);
        blockPools.Add(block3Pool);
        blockPools.Add(block4Pool);
        blockPools.Add(block5Pool);
        blockPools.Add(block6Pool);
    }

    public TileEntity GetTileEntity(Level level, LevelTile tile)
    {
        if (tile is BlockTile)
        {
            var blockTile = (BlockTile)tile;
            switch (blockTile.type)
            {
                case BlockType.Blue:
                    return block1Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Green:
                    return block2Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Pink:
                    return block3Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Purple:
                    return block4Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Yellow:
                    return block5Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.Red:
                    return block6Pool.GetObject().GetComponent<TileEntity>();

                case BlockType.RandomBlock:
                {
                    var randomIdx = Random.Range(0, level.availableColors.Count);
                    return blockPools[(int)level.availableColors[randomIdx]].GetObject().GetComponent<TileEntity>();
                }

                case BlockType.Empty:
                    return emptyTilePool.GetObject().GetComponent<TileEntity>();
            }
        }
        else if (tile is BoosterTile)
        {
            var boosterTile = (BoosterTile)tile;
            switch (boosterTile.type)
            {
                case BoosterType.HorizontalBomb:
                    return horizontalBombPool.GetObject().GetComponent<TileEntity>();

                case BoosterType.VerticalBomb:
                    return verticalBombPool.GetObject().GetComponent<TileEntity>();

                case BoosterType.Dynamite:
                    return dynamitePool.GetObject().GetComponent<TileEntity>();

                case BoosterType.ColorBomb:
                    return colorBombPool.GetObject().GetComponent<TileEntity>();
            }
        }

        return null;
    }

    public GameObject GetParticles(TileEntity tileEntity)
    {
        GameObject particles = null;
        var block = tileEntity as Block;
        if (block != null)
        {
            switch (block.type)
            {
                case BlockType.Blue:
                    particles = block1ParticlesPool.GetObject();
                    break;

                case BlockType.Green:
                    particles = block2ParticlesPool.GetObject();
                    break;

                case BlockType.Pink:
                    particles = block3ParticlesPool.GetObject();
                    break;

                case BlockType.Purple:
                    particles = block4ParticlesPool.GetObject();
                    break;

                case BlockType.Yellow:
                    particles = block5ParticlesPool.GetObject();
                    break;

                case BlockType.Red:
                    particles = block6ParticlesPool.GetObject();
                    break;

                default:
                    return null;
            }
        }

        return particles;
    }
}