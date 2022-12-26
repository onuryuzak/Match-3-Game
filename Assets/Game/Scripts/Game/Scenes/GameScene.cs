using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using FullSerializer;


/// <summary>
/// This class contains the logic associated to the game scene.
/// </summary>
public class GameScene : BaseScene
{
    public GamePools gamePools;

    public GameUi gameUi;
    public ObjectPool scoreTextPool;

    public float blockFallSpeed;

    public float horizontalSpacing;
    public float verticalSpacing;

    public Transform levelLocation;

    public Sprite backgroundSprite;
    public Color backgroundColor;

    [HideInInspector] public Level level;

    [HideInInspector] public List<GameObject> tileEntities = new List<GameObject>();
    public readonly List<GameObject> blockers = new List<GameObject>();
    [HideInInspector] public List<Vector2> tilePositions = new List<Vector2>();

    private readonly GameState gameState = new GameState();

    private GameConfiguration gameConfig;

    private bool gameStarted;
    private bool gameFinished;

    private float accTime;

    private float blockWidth;
    private float blockHeight;

    private bool suggestedMatch;
    private readonly List<GameObject> suggestedMatchBlocks = new List<GameObject>();

    private int currentLimit;

    private bool currentlyAwardingBoosters;

    private const float timeBetweenMatchSuggestions = 5.0f;
    private const float endGamePopupDelay = 0.75f;

    private bool goalsComplete;
    private bool levelFinishForChangeSprite;

    private int ingameBoosterBgTweenId;
    private Camera mainCamera;

    /// <summary>
    /// Unity's Start method.
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;

        var serializer = new fsSerializer();
        gameConfig = FileUtils.LoadJsonFile<GameConfiguration>(serializer, "game_configuration");
        level = FileUtils.LoadJsonFile<Level>(serializer,
            "Levels/" + PuzzleMatchManager.instance.lastSelectedLevel);

        ResetLevelData();
        StartGame();
        CreateBackgroundTiles();
    }

    /// <summary>
    /// Unity's Update method.
    /// </summary>
    private void Update()
    {
        if (!gameStarted || gameFinished || currentlyAwardingBoosters)
        {
            return;
        }

        if (currentPopups.Count > 0)
        {
            return;
        }

        accTime += Time.deltaTime;
        if (accTime >= timeBetweenMatchSuggestions)
        {
            accTime = 0.0f;
            HighlightRandomMatch();
        }

        if (Input.GetMouseButtonDown(0))
        {
            suggestedMatch = false;
            accTime = 0.0f;
            foreach (var block in suggestedMatchBlocks)
            {
                if (block.activeSelf)
                {
                    block.GetComponent<Animator>().SetTrigger("Reset");
                }
            }

            var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Block"))
            {
                var hitIdx = tileEntities.FindIndex(x => x == hit.collider.gameObject);
                var hitBlock = hit.collider.gameObject.GetComponent<TileEntity>();
                if (blockers[hitIdx] != null)
                {
                    return;
                }

                if (IsBoosterBlock(hitBlock))
                {
                    DestroyBooster(hitBlock);
                }
                else
                {
                    DestroyBlock(hitBlock);
                }
            }
        }
    }

    /// <summary>
    /// Spawns a new tile entity.
    /// </summary>
    /// <param name="go">The game object to spawn.</param>
    /// <returns>The spawned game object.</returns>
    private GameObject CreateBlock(GameObject go)
    {
        go.GetComponent<TileEntity>().Spawn();
        return go;
    }

    /// <summary>
    /// Destroys the specified block.
    /// </summary>
    /// <param name="blockToDestroy">The block to destroy.</param>
    private void DestroyBlock(TileEntity blockToDestroy)
    {
        var blockIdx = tileEntities.FindIndex(x => x == blockToDestroy.gameObject);
        var blocksToDestroy = new List<GameObject>();
        GetMatches(blockToDestroy.gameObject, blocksToDestroy);
        var score = 0;
        if (blocksToDestroy.Count > 0)
        {
            foreach (var block in blocksToDestroy)
            {
                var idx = tileEntities.FindIndex(x => x == block);
                score += DestroyTileEntity(block.GetComponent<TileEntity>(), idx);
            }

            ShowScoreText(score, blockToDestroy.transform.position);
            UpdateScore(score);
            CreateBooster(blocksToDestroy.Count, blockIdx);
            PerformMove(true);
            StartCoroutine(ApplyGravityAsync());
        }
        else
        {
            blockToDestroy.GetComponent<Animator>().SetTrigger("NoMatches");
        }
    }


    /// <summary>
    /// Destroys the specified booster.
    /// </summary>
    /// <param name="blockToDestroy">The booster to destroy.</param>
    private void DestroyBooster(TileEntity blockToDestroy)
    {
        var score = 0;

        var blocksToDestroy = new List<GameObject>();
        var usedBoosters = new List<GameObject>();
        DestroyBoosterRecursive(blockToDestroy, blocksToDestroy, usedBoosters);

        foreach (var block in blocksToDestroy)
        {
            var idx = tileEntities.FindIndex(x => x == block);

            score += DestroyTileEntity(block.GetComponent<TileEntity>(), idx, false);
        }

        ShowScoreText(score, blockToDestroy.transform.position);
        UpdateScore(score);
        PerformMove(true);
        StartCoroutine(ApplyGravityAsync());
    }

    /// <summary>
    /// Internal recursive method used to destroy the specified booster.
    /// </summary>
    /// <param name="blockToDestroy">The booster to destroy.</param>
    /// <param name="blocksToDestroy">The accumulated blocks that have been destroyed so far.</param>
    /// <param name="usedBoosters">The boosters already destroyed.</param>
    private void DestroyBoosterRecursive(TileEntity blockToDestroy, List<GameObject> blocksToDestroy,
        List<GameObject> usedBoosters)
    {
        var blockIdx = tileEntities.FindIndex(x => x == blockToDestroy.gameObject);
        var newBlocksToDestroy = blockToDestroy.GetComponent<Booster>().Resolve(this, blockIdx);
        usedBoosters.Add(blockToDestroy.gameObject);

        blockToDestroy.GetComponent<Booster>().ShowFx(gamePools, this, blockIdx);

        if (!currentlyAwardingBoosters)
        {
            foreach (var block in newBlocksToDestroy)
            {
                if (block.GetComponent<Booster>() != null && !usedBoosters.Contains(block))
                {
                    usedBoosters.Add(block);
                    DestroyBoosterRecursive(block.GetComponent<TileEntity>(), blocksToDestroy, usedBoosters);
                }
            }
        }

        foreach (var block in newBlocksToDestroy)
        {
            if (!blocksToDestroy.Contains(block))
            {
                blocksToDestroy.Add(block);
            }
        }
    }

    /// <summary>
    /// Destroys the specified tile entity.
    /// </summary>
    /// <param name="tileEntity">The tile entity to destroy.</param>
    /// <param name="tileIndex">The index of the tile entity to destroy.</param>
    /// <param name="playSound">True if the destruction sound should be played; false otherwise.</param>
    /// <returns>The score obtained by the destruction of the tile entity.</returns>
    private int DestroyTileEntity(TileEntity tileEntity, int tileIndex, bool playSound = true)
    {
        var block = tileEntity.GetComponent<Block>();
        if (block != null)
        {
            gameState.collectedBlocks[block.type] += 1;
        }

        var tileScore = gameConfig.GetScore(tileEntity);

        var particles = gamePools.GetParticles(tileEntity);
        if (particles != null)
        {
            particles.transform.position = tileEntity.transform.position;
            particles.GetComponent<TileParticles>().fragmentParticles.Play();
        }

        tileEntity.Explode();
        tileEntities[tileIndex] = null;
        tileEntity.GetComponent<PooledObject>().pool.ReturnObject(tileEntity.gameObject);
        return tileScore;
    }

    /// <summary>
    /// Updates the score of the current game.
    /// </summary>
    /// <param name="score">The score.</param>
    private void UpdateScore(int score)
    {
        gameState.score += score;
        gameUi.scoreText.text = gameState.score.ToString();
        gameUi.progressBar.UpdateProgressBar(gameState.score);
    }

    /// <summary>
    /// Resets the level data. This is particularly useful when replaying a level.
    /// </summary>
    private void ResetLevelData()
    {
        gameStarted = false;
        gameFinished = false;
        currentlyAwardingBoosters = false;

        gameState.Reset();

        gameUi.limitText.text = "";
        gameUi.scoreText.text = gameState.score.ToString();
        gameUi.progressBar.Fill(level.score1, level.score2, level.score3);

        foreach (var pool in gamePools.GetComponentsInChildren<ObjectPool>())
        {
            pool.Reset();
        }

        tileEntities.Clear();
        blockers.Clear();
        tilePositions.Clear();

        for (var j = 0; j < level.height; j++)
        {
            for (var i = 0; i < level.width; i++)
            {
                var tileIndex = i + (j * level.width);
                var tileToGet = gamePools.GetTileEntity(level, level.tiles[tileIndex]);
                var tile = CreateBlock(tileToGet.gameObject);
                var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                blockWidth = spriteRenderer.bounds.size.x;
                blockHeight = spriteRenderer.bounds.size.y;
                tile.transform.position = new Vector2(i * (blockWidth + horizontalSpacing),
                    -j * (blockHeight + verticalSpacing));
                tileEntities.Add(tile);
                spriteRenderer.sortingOrder = level.height - j;
            }
        }

        var totalWidth = (level.width - 1) * (blockWidth + horizontalSpacing);
        var totalHeight = (level.height - 1) * (blockHeight + verticalSpacing);
        foreach (var block in tileEntities)
        {
            var newPos = block.transform.position;
            newPos.x -= totalWidth / 2;
            newPos.y += totalHeight / 2;
            newPos.y += levelLocation.position.y;
            block.transform.position = newPos;
            tilePositions.Add(newPos);
        }

        for (var j = 0; j < level.height; j++)
        {
            for (var i = 0; i < level.width; i++)
            {
                blockers.Add(null);
            }
        }

        var zoomLevel = gameConfig.GetZoomLevel();
        mainCamera.orthographicSize = (totalWidth * zoomLevel) * (Screen.height / (float)Screen.width) * 0.5f;

        StartGame();
    }

    /// <summary>
    /// Creates the background tiles of the level.
    /// </summary>
    private void CreateBackgroundTiles()
    {
        var backgroundTiles = new GameObject("BackgroundTiles");
        for (var j = 0; j < level.height; j++)
        {
            for (var i = 0; i < level.width; i++)
            {
                var tileIndex = i + (j * level.width);
                var tile = level.tiles[tileIndex];
                var blockTile = tile as BlockTile;
                if (blockTile != null && blockTile.type == BlockType.Empty)
                {
                    continue;
                }

                var go = new GameObject("Background");
                go.transform.parent = backgroundTiles.transform;
                var sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = backgroundSprite;
                sprite.color = backgroundColor;
                sprite.sortingLayerName = "Game";
                sprite.sortingOrder = -2;
                sprite.transform.position = tileEntities[tileIndex].transform.position;
            }
        }
    }

    /// <summary>
    /// Highlights a random match as a suggestion to the player when he is idle for some time.
    /// </summary>
    private void HighlightRandomMatch()
    {
        if (suggestedMatch)
        {
            return;
        }

        suggestedMatchBlocks.Clear();
        var tries = 0;
        do
        {
            var randomIdx = UnityEngine.Random.Range(0, tileEntities.Count);
            if (tileEntities[randomIdx] != null)
            {
                // Prevent infinite loops when no matches are possible.
                tries += 1;
                if (tries >= 100)
                {
                    break;
                }

                if (IsColorBlock(tileEntities[randomIdx].GetComponent<TileEntity>()))
                {
                    GetMatches(tileEntities[randomIdx], suggestedMatchBlocks);
                }

                // Prevent matches with all the blocks having blockers.
                var isValidMatch = false;
                foreach (var tile in suggestedMatchBlocks)
                {
                    var idx = tileEntities.FindIndex(x => x == tile);
                    if (blockers[idx] == null)
                    {
                        isValidMatch = true;
                        break;
                    }
                }

                if (!isValidMatch)
                {
                    suggestedMatchBlocks.Clear();
                }
            }
        } while (suggestedMatchBlocks.Count == 0);

        if (suggestedMatchBlocks.Count > 0)
        {
            foreach (var match in suggestedMatchBlocks)
            {
                match.GetComponent<Animator>().SetTrigger("SuggestedMatch");
            }

            suggestedMatch = true;
        }
        else
        {
            OpenPopup<RegenLevelPopup>("Popups/RegenLevelPopup");
            StartCoroutine(RegenerateLevel());
        }
    }

    /// <summary>
    /// Regenerates the level when no matches are possible.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator RegenerateLevel()
    {
        yield return new WaitForSeconds(2.0f);
        for (var i = 0; i < level.width; i++)
        {
            for (var j = 0; j < level.height; j++)
            {
                var idx = i + (j * level.width);
                var block = tileEntities[idx];
                if (block != null)
                {
                    if (IsColorBlock(block.GetComponent<TileEntity>()))
                    {
                        block.GetComponent<PooledObject>().pool.ReturnObject(block);
                        var newBlock = CreateNewBlock();
                        tileEntities[idx] = newBlock;
                        newBlock.transform.position = tilePositions[idx];
                    }
                }
            }
        }
    }


    /// <summary>
    /// Calculates the matches of the specified block.
    /// </summary>
    /// <param name="go">The block to check.</param>
    /// <param name="matchedTiles">A list containing the matched tiles.</param>
    private void GetMatches(GameObject go, List<GameObject> matchedTiles)
    {
        var idx = tileEntities.FindIndex(x => x == go);
        var i = idx % level.width;
        var j = idx / level.width;

        var topTile = new TileDef(i, j - 1);
        var bottomTile = new TileDef(i, j + 1);
        var leftTile = new TileDef(i - 1, j);
        var rightTile = new TileDef(i + 1, j);
        var surroundingTiles = new List<TileDef> { topTile, bottomTile, leftTile, rightTile };

        var hasMatch = false;
        foreach (var surroundingTile in surroundingTiles)
        {
            if (IsValidTileEntity(surroundingTile))
            {
                var tileIndex = (level.width * surroundingTile.y) + surroundingTile.x;
                var tile = tileEntities[tileIndex];
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && block.type == go.GetComponent<Block>().type)
                    {
                        hasMatch = true;
                    }
                }
            }
        }

        if (!hasMatch)
        {
            return;
        }

        if (!matchedTiles.Contains(go))
        {
            matchedTiles.Add(go);
        }

        foreach (var surroundingTile in surroundingTiles)
        {
            if (IsValidTileEntity(surroundingTile))
            {
                var tileIndex = (level.width * surroundingTile.y) + surroundingTile.x;
                var tile = tileEntities[tileIndex];
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && block.type == go.GetComponent<Block>().type &&
                        !matchedTiles.Contains(tile))
                    {
                        GetMatches(tile, matchedTiles);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new booster at the specified index.
    /// </summary>
    /// <param name="numMatchedBlocks">The number of matched blocks.</param>
    /// <param name="blockIdx">The index of the block.</param>
    private void CreateBooster(int numMatchedBlocks, int blockIdx)
    {
        var eligibleBoosters = new List<KeyValuePair<BoosterType, int>>();
        foreach (var pair in gameConfig.boosterNeededMatches)
        {
            if (numMatchedBlocks >= pair.Value)
            {
                eligibleBoosters.Add(pair);
            }
        }

        if (eligibleBoosters.Count > 0)
        {
            var max = eligibleBoosters.Max(x => x.Value);
            eligibleBoosters.RemoveAll(x => x.Value != max);
            var idx = UnityEngine.Random.Range(0, eligibleBoosters.Count);
            var booster = eligibleBoosters[idx];
            CreateBooster(GetBoosterPool(booster.Key).GetObject(), blockIdx);
        }
    }

    /// <summary>
    /// Returns the booster pool corresponding to the specified booster type.
    /// </summary>
    /// <param name="type">The booster type.</param>
    /// <returns>The booster pool corresponding to the specified booster type.</returns>
    private ObjectPool GetBoosterPool(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.HorizontalBomb:
                return gamePools.horizontalBombPool;

            case BoosterType.VerticalBomb:
                return gamePools.verticalBombPool;

            case BoosterType.Dynamite:
                return gamePools.dynamitePool;

            case BoosterType.ColorBomb:
                return gamePools.colorBombPool;
        }

        return null;
    }

    /// <summary>
    /// Creates the booster of the specified type at the specified index.
    /// </summary>
    /// <param name="type">The type of booster to create.</param>
    /// <param name="blockIdx">The index at which to create the booster.</param>
    private void CreateBooster(BoosterType type, int blockIdx)
    {
        ObjectPool boosterPool = null;
        switch (type)
        {
            case BoosterType.HorizontalBomb:
                boosterPool = gamePools.horizontalBombPool;
                break;

            case BoosterType.VerticalBomb:
                boosterPool = gamePools.verticalBombPool;
                break;

            case BoosterType.Dynamite:
                boosterPool = gamePools.dynamitePool;
                break;

            case BoosterType.ColorBomb:
                boosterPool = gamePools.colorBombPool;
                break;
        }

        Assert.IsNotNull(boosterPool);
        var booster = CreateBlock(boosterPool.GetObject());
        CreateBooster(booster, blockIdx);
    }

    /// <summary>
    /// Creates a booster at the specified index.
    /// </summary>
    /// <param name="booster">The booster to create.</param>
    /// <param name="blockIdx">The index at which to create the booster.</param>
    private void CreateBooster(GameObject booster, int blockIdx)
    {
        booster.transform.position = tilePositions[blockIdx];
        tileEntities[blockIdx] = booster;
        var j = blockIdx / level.height;
        booster.GetComponent<SpriteRenderer>().sortingOrder = level.height - j;

        var particles = gamePools.boosterSpawnParticlesPool.GetObject();
        particles.AddComponent<AutoKillPooled>();
        particles.transform.position = booster.transform.position;
        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }
    }

    /// <summary>
    /// Utility coroutine to apply the gravity to the level.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ApplyGravityAsync()
    {
        yield return new WaitForSeconds(0.1f);
        ApplyGravity();
        yield return new WaitForSeconds(0.5f);
        CheckEndGame();
    }

    /// <summary>
    /// Applies the gravity to the level.
    /// </summary>
    private void ApplyGravity()
    {
        for (var i = 0; i < level.width; i++)
        {
            for (var j = level.height - 1; j >= 0; j--)
            {
                var tileIndex = i + (j * level.width);
                if (tileEntities[tileIndex] == null ||
                    IsEmptyBlock(tileEntities[tileIndex].GetComponent<TileEntity>()))
                {
                    continue;
                }

                // Find bottom.
                var bottom = -1;
                for (var k = j; k < level.height; k++)
                {
                    var idx = i + (k * level.width);
                    if (tileEntities[idx] == null)
                    {
                        bottom = k;
                    }
                }

                if (bottom != -1)
                {
                    var tile = tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var numTilesToFall = bottom - j;
                        tileEntities[tileIndex + (numTilesToFall * level.width)] = tileEntities[tileIndex];
                        var tween = LeanTween.move(tile,
                            tilePositions[tileIndex + level.width * numTilesToFall],
                            blockFallSpeed);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() =>
                        {
                            if (tile.activeSelf)
                            {
                                tile.GetComponent<Animator>().SetTrigger("Falling");
                            }
                        });
                        tileEntities[tileIndex] = null;
                    }
                }
            }
        }

        for (var i = 0; i < level.width; i++)
        {
            var numEmpties = 0;
            for (var j = 0; j < level.height; j++)
            {
                var idx = i + (j * level.width);
                if (tileEntities[idx] == null)
                {
                    numEmpties += 1;
                }
            }

            if (numEmpties > 0)
            {
                for (var j = 0; j < level.height; j++)
                {
                    var tileIndex = i + (j * level.width);
                    var isEmptyTile = false;
                    if (tileEntities[tileIndex] != null)
                    {
                        var blockTile = tileEntities[tileIndex].GetComponent<Block>();
                        if (blockTile != null)
                        {
                            isEmptyTile = blockTile.type == BlockType.Empty;
                        }
                    }

                    if (tileEntities[tileIndex] == null && !isEmptyTile)
                    {
                        var tile = CreateNewBlock();
                        var pos = tilePositions[i];
                        pos.y = tilePositions[i].y + (numEmpties * (blockHeight + verticalSpacing));
                        --numEmpties;
                        tile.transform.position = pos;
                        var tween = LeanTween.move(tile,
                            tilePositions[tileIndex],
                            blockFallSpeed);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() =>
                        {
                            if (tile.activeSelf)
                            {
                                tile.GetComponent<Animator>().SetTrigger("Falling");
                            }
                        });
                        tileEntities[tileIndex] = tile;
                        StartCoroutine(ChangeMatchingBlocksSprite());
                    }

                    if (tileEntities[tileIndex] != null)
                    {
                        tileEntities[tileIndex].GetComponent<SpriteRenderer>().sortingOrder = level.height - j;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new block.
    /// </summary>
    /// <returns>The newly created block.</returns>
    private GameObject CreateNewBlock()
    {
        return CreateBlock(gamePools.GetTileEntity(level, new BlockTile { type = BlockType.RandomBlock })
            .gameObject);
    }

    private IEnumerator ChangeMatchingBlocksSprite()
    {
        yield return new WaitForSeconds(0.75f);

        if (!levelFinishForChangeSprite)
        {
            foreach (var tile in tileEntities)
            {
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && block.type != BlockType.Empty && !tile.GetComponent<Booster>())
                    {
                        var matchingBlocks = new List<GameObject>();
                        GetMatches(tile, matchingBlocks);
                        if (matchingBlocks.Count >= 0)
                        {
                            Sprite newSprite = tile.GetComponent<Block>().defaultIcon;
                            if (matchingBlocks.Count >= level.spriteATresHold &&
                                matchingBlocks.Count < level.spriteBTresHold)
                            {
                                newSprite = tile.GetComponent<Block>().firstIcon;
                            }
                            else if (matchingBlocks.Count >= level.spriteBTresHold &&
                                     matchingBlocks.Count < level.spriteCTresHold)
                            {
                                newSprite = tile.GetComponent<Block>().secondIcon;
                            }
                            else if (matchingBlocks.Count >= level.spriteCTresHold)
                            {
                                newSprite = tile.GetComponent<Block>().thirdIcon;
                            }

                            tile.GetComponent<SpriteRenderer>().sprite = newSprite;
                            foreach (var matchingBlock in matchingBlocks)
                            {
                                matchingBlock.GetComponent<SpriteRenderer>().sprite = newSprite;
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Updates the state of the game based on the last move performed by the player.
    /// </summary>
    /// <param name="updateLimits">True if the current limits of the level should be updated; false otherwise.</param>
    private void PerformMove(bool updateLimits)
    {
        if (currentlyAwardingBoosters)
        {
            return;
        }

        if (level.limitType == LimitType.Moves && updateLimits)
        {
            --currentLimit;
            if (currentLimit < 0)
            {
                currentLimit = 0;
            }

            UpdateLimitText();
        }
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void StartGame()
    {
        currentLimit = level.moveLimit;
        UpdateLimitText();
        if (level.limitType == LimitType.Moves)
        {
            gameUi.limitTitleText.text = "Moves";
        }

        gameStarted = true;
    }

    /// <summary>
    /// Ends the current game.
    /// </summary>
    private void EndGame()
    {
        gameFinished = true;
    }

    /// <summary>
    /// Restarts the current game.
    /// </summary>
    public void RestartGame()
    {
        ResetLevelData();
    }

    /// <summary>
    /// Checks if the game has finished.
    /// </summary>
    private void CheckEndGame()
    {
        if (currentlyAwardingBoosters)
        {
            return;
        }

        if (gameFinished)
        {
            return;
        }

        if (gameState.score >= gameUi.progressBar.star1 && currentLimit == 0)
        {
            goalsComplete = true;
        }
        else if (currentLimit > 0 && gameState.score >= gameUi.progressBar.star3)
        {
            goalsComplete = true;
        }


        if (currentLimit == 0)
        {
            EndGame();
        }

        if (gameState.score < gameUi.progressBar.star1)
        {
            goalsComplete = false;
        }


        if (goalsComplete)
        {
            EndGame();

            var nextLevel = PlayerPrefs.GetInt("next_level");
            if (nextLevel == 0)
            {
                nextLevel = 1;
            }

            if (level.id == nextLevel)
            {
                PlayerPrefs.SetInt("next_level", level.id + 1);
                PuzzleMatchManager.instance.unlockedNextLevel = true;
            }
            else
            {
                PuzzleMatchManager.instance.unlockedNextLevel = false;
            }

            if (level.limitType == LimitType.Moves && level.awardBoostersWithRemainingMoves && currentLimit > 0)
            {
                levelFinishForChangeSprite = true;
                StartCoroutine(AwardBoosters());
            }
            else
            {
                StartCoroutine(OpenWinPopupAsync());
            }
        }
        else
        {
            if (gameFinished)
            {
                StartCoroutine(OpenNoMovesPopupAsync());
            }
        }
    }

    /// <summary>
    /// Awards boosters at the end of a level.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator AwardBoosters()
    {
        yield return new WaitForSeconds(1.0f);
        OpenPopup<BoosterAwardPopup>("Popups/BoosterAwardPopup");
        yield return new WaitForSeconds(1.5f);
        currentlyAwardingBoosters = true;
        while (currentLimit > 0)
        {
            int randomIdx;
            do
            {
                randomIdx = UnityEngine.Random.Range(0, tileEntities.Count);
            } while (tileEntities[randomIdx] != null &&
                     !IsColorBlock(tileEntities[randomIdx].GetComponent<TileEntity>()));

            var tile = tileEntities[randomIdx];
            tileEntities[randomIdx] = null;
            if (tile != null)
            {
                tile.GetComponent<PooledObject>().pool.ReturnObject(tile.gameObject);
            }

            CreateBooster(level.awardedBoosterType, randomIdx);

            currentLimit -= 1;
            UpdateLimitText();
            yield return new WaitForSeconds(0.25f);
        }

        do
        {
            GameObject tileToExplode = null;
            foreach (var tile in tileEntities)
            {
                if (tile != null && tile.GetComponent<Booster>() != null)
                {
                    tileToExplode = tile;
                    break;
                }
            }

            if (tileToExplode != null)
            {
                DestroyBooster(tileToExplode.GetComponent<Booster>());
                yield return new WaitForSeconds(0.5f);
            }
        } while (tileEntities.Find(x => x != null && IsBoosterBlock(x.GetComponent<TileEntity>())) != null);

        OpenWinPopup();
    }

    /// <summary>
    /// Opens the win popup.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator OpenWinPopupAsync()
    {
        yield return new WaitForSeconds(endGamePopupDelay);
        OpenWinPopup();
    }

    /// <summary>
    /// Opens the popup for buying additional moves or time.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator OpenNoMovesPopupAsync()
    {
        yield return new WaitForSeconds(endGamePopupDelay);
        OpenNoMovesPopup();
    }

    /// <summary>
    /// Opens the win popup.
    /// </summary>
    private void OpenWinPopup()
    {
        OpenPopup<WinPopup>("Popups/WinPopup", popup =>
        {
            var levelStars = PlayerPrefs.GetInt("level_stars_" + level.id);
            if (gameState.score >= level.score3)
            {
                popup.SetStars(3);
                PlayerPrefs.SetInt("level_stars_" + level.id, 3);
            }
            else if (gameState.score >= level.score2)
            {
                popup.SetStars(2);
                if (levelStars < 3)
                {
                    PlayerPrefs.SetInt("level_stars_" + level.id, 2);
                }
            }
            else if (gameState.score >= level.score1)
            {
                popup.SetStars(1);
                if (levelStars < 2)
                {
                    PlayerPrefs.SetInt("level_stars_" + level.id, 1);
                }
            }
            else
            {
                popup.SetStars(0);
            }

            popup.SetLevel(level.id);

            var levelScore = PlayerPrefs.GetInt("level_score_" + level.id);
            if (levelScore < gameState.score)
            {
                PlayerPrefs.SetInt("level_score_" + level.id, gameState.score);
            }

            popup.SetScore(gameState.score);
        });
    }

    /// <summary>
    /// Opens the popup for buying additional moves or time.
    /// </summary>
    private void OpenNoMovesPopup()
    {
        OpenPopup<NoMovesPopup>("Popups/NoMovesPopup",
            popup => { popup.SetGameScene(this); });
    }

    /// <summary>
    /// Opens the lose popup.
    /// </summary>
    public void OpenLosePopup()
    {
        OpenPopup<LosePopup>("Popups/LosePopup", popup =>
        {
            popup.SetLevel(level.id);
            popup.SetScore(gameState.score);
        });
    }

    /// <summary>
    /// Returns true if the specified tile entity is valid and false otherwise.
    /// </summary>
    /// <param name="tileEntity">The tile entity.</param>
    /// <returns>True if the specified tile entity is valid; false otherwise.</returns>
    private bool IsValidTileEntity(TileDef tileEntity)
    {
        return tileEntity.x >= 0 && tileEntity.x < level.width &&
               tileEntity.y >= 0 && tileEntity.y < level.height;
    }

    /// <summary>
    /// Returns true if the specified tile entity is a color block and false otherwise.
    /// </summary>
    /// <param name="tileEntity">The tile entity.</param>
    /// <returns>True if the specified tile entity is a color block; false otherwise.</returns>
    private bool IsColorBlock(TileEntity tileEntity)
    {
        var block = tileEntity as Block;
        return block != null &&
               (block.type == BlockType.Blue ||
                block.type == BlockType.Green ||
                block.type == BlockType.Pink ||
                block.type == BlockType.Purple ||
                block.type == BlockType.Yellow ||
                block.type == BlockType.Red);
    }

    /// <summary>
    /// Returns true if the specified tile entity is a booster block and false otherwise.
    /// </summary>
    /// <param name="tileEntity">The tile entity.</param>
    /// <returns>True if the specified tile entity is a booster block; false otherwise.</returns>
    private bool IsBoosterBlock(TileEntity tileEntity)
    {
        return tileEntity is Booster;
    }

    /// <summary>
    /// Returns true if the specified tile entity is an empty block and false otherwise.
    /// </summary>
    /// <param name="tileEntity">The tile entity.</param>
    /// <returns>True if the specified tile entity is an empty block; false otherwise.</returns>
    private bool IsEmptyBlock(TileEntity tileEntity)
    {
        var block = tileEntity as Block;
        return block != null && block.type == BlockType.Empty;
    }

    /// <summary>
    /// Shows a score text on the specified position.
    /// </summary>
    /// <param name="score">The score to show.</param>
    /// <param name="pos">The position in which to show the score text.</param>
    private void ShowScoreText(int score, Vector2 pos)
    {
        var scoreText = scoreTextPool.GetObject();
        var canvasRect = canvas.GetComponent<RectTransform>();
        scoreText.transform.SetParent(canvas.transform, false);
        scoreText.GetComponent<Text>().text = string.Format("+{0}", score);
        var viewportPos = mainCamera.WorldToViewportPoint(pos);
        var screenPos = new Vector2(
            (viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
        scoreText.GetComponent<RectTransform>().anchoredPosition = screenPos;
        scoreText.GetComponent<ScoreText>().StartAnimation();
    }

    /// <summary>
    /// Updates the limit text.
    /// </summary>
    private void UpdateLimitText()
    {
        if (level.limitType == LimitType.Moves)
        {
            gameUi.limitText.text = currentLimit.ToString();
        }
    }
}