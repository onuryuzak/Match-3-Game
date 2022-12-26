using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


public class LevelEditorTab : EditorTab
{
    private int prevWidth = -1;
    private int prevHeight = -1;

    private enum BrushType
    {
        Block,
        Booster
    }

    private BrushType currentBrushType;
    private BlockType currentBlockType;
    private BoosterType currentBoosterType;

    private enum BrushMode
    {
        Tile,
        Row,
        Column,
        Fill
    }

    private BrushMode currentBrushMode = BrushMode.Tile;

    private readonly Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();

    private Level currentLevel;

    private ReorderableList availableColorBlocksList;
    private ColorBlockType currentColorBlock;

    private Vector2 scrollPos;


    public LevelEditorTab(Match3ManagementEditor editor) : base(editor)
    {
        var editorImagesPath = new DirectoryInfo(Application.dataPath + "/Resources/Game");
        var fileInfo = editorImagesPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
        foreach (var file in fileInfo)
        {
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            tileTextures[filename] = Resources.Load("Game/" + filename) as Texture;
        }
    }


    public override void Draw()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90;

        GUILayout.Space(15);

        DrawMenu();

        if (currentLevel != null)
        {
            var level = currentLevel;
            prevWidth = level.width;

            GUILayout.Space(15);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            DrawGeneralSettings();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            DrawGoalSettings();
            GUILayout.Space(15);
            DrawAvailableColorBlockSettings();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(15);

            DrawLevelEditor();
        }

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndScrollView();
    }


    private void DrawMenu()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("New", GUILayout.Width(100), GUILayout.Height(50)))
        {
            currentLevel = new Level();
            InitializeNewLevel();
            CreateAvailableColorBlocksList();
        }

        if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(50)))
        {
            var path = EditorUtility.OpenFilePanel("Open level",
                Application.dataPath + "/Resources/Levels",
                "json");
            if (!string.IsNullOrEmpty(path))
            {
                currentLevel = LoadJsonFile<Level>(path);
                CreateAvailableColorBlocksList();
            }
        }

        if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50)))
        {
            SaveLevel(Application.dataPath + "/Resources");
        }

        GUILayout.EndHorizontal();
    }

    private void DrawGeneralSettings()
    {
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "The general settings of this level.",
            MessageType.Info);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Level number", "The number of this level."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.id = EditorGUILayout.IntField(currentLevel.id, GUILayout.Width(30));
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Limit type", "The limit type of this level."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.limitType = (LimitType)EditorGUILayout.EnumPopup(currentLevel.limitType, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (currentLevel.limitType == LimitType.Moves)
        {
            EditorGUILayout.LabelField(new GUIContent("Moves", "The maximum number of moves of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
        }


        currentLevel.moveLimit = EditorGUILayout.IntField(currentLevel.moveLimit, GUILayout.Width(30));
        GUILayout.EndHorizontal();


        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Star 1 score", "The score needed to reach the first star."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.score1 = EditorGUILayout.IntField(currentLevel.score1, GUILayout.Width(70));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Star 2 score", "The score needed to reach the second star."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.score2 = EditorGUILayout.IntField(currentLevel.score2, GUILayout.Width(70));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Star 3 score", "The score needed to reach the third star."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.score3 = EditorGUILayout.IntField(currentLevel.score3, GUILayout.Width(70));
        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            new GUIContent("Icon A treshold", "The treshold required to make the default icon the icon A."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.spriteATresHold = EditorGUILayout.IntField(currentLevel.spriteATresHold, GUILayout.Width(70));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            new GUIContent("Icon B treshold", "The treshold required to make the default icon the icon A."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.spriteBTresHold = EditorGUILayout.IntField(currentLevel.spriteBTresHold, GUILayout.Width(70));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            new GUIContent("Icon C treshold", "The treshold required to make the default icon the icon A."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.spriteCTresHold = EditorGUILayout.IntField(currentLevel.spriteCTresHold, GUILayout.Width(70));
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
    }


    private void DrawGoalSettings()
    {
        EditorGUILayout.LabelField("Goals", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "This list defines the goals needed to be achieved by the player in order to complete this level.",
            MessageType.Info);
        GUILayout.EndHorizontal();


        if (currentLevel.limitType == LimitType.Moves)
        {
            EditorGUIUtility.labelWidth = 100;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Award boosters",
                    "Enable this if you want boosters equal to the number of remaining moves to be awarded to the player at the end of the game."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.awardBoostersWithRemainingMoves =
                EditorGUILayout.Toggle(currentLevel.awardBoostersWithRemainingMoves);
            GUILayout.EndHorizontal();

            if (currentLevel.awardBoostersWithRemainingMoves)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Booster", "The type of booster to award."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentLevel.awardedBoosterType =
                    (BoosterType)EditorGUILayout.EnumPopup(currentLevel.awardedBoosterType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = 90;
        }
    }


    private void DrawAvailableColorBlockSettings()
    {
        GUILayout.BeginVertical();

        EditorGUILayout.LabelField("Available color blocks", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "This list defines the available color blocks when a new random color block is created.",
            MessageType.Info);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(250));
        if (availableColorBlocksList != null)
        {
            availableColorBlocksList.DoLayoutList();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
    }


    private void DrawLevelEditor()
    {
        EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "The layout settings of this level.",
            MessageType.Info);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Width", "The width of this level."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.width = EditorGUILayout.IntField(currentLevel.width, GUILayout.Width(30));
        GUILayout.EndHorizontal();

        prevHeight = currentLevel.height;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Height", "The height of this level."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentLevel.height = EditorGUILayout.IntField(currentLevel.height, GUILayout.Width(30));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Brush type", "The current type of brush."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentBrushType = (BrushType)EditorGUILayout.EnumPopup(currentBrushType, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        if (currentBrushType == BrushType.Block)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Block", "The current type of block."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBlockType = (BlockType)EditorGUILayout.EnumPopup(currentBlockType, GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        else if (currentBrushType == BrushType.Booster)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Booster", "The current type of booster."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBoosterType =
                (BoosterType)EditorGUILayout.EnumPopup(currentBoosterType, GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Brush mode", "The current brush mode."),
            GUILayout.Width(EditorGUIUtility.labelWidth));
        currentBrushMode = (BrushMode)EditorGUILayout.EnumPopup(currentBrushMode, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (prevWidth != currentLevel.width || prevHeight != currentLevel.height)
        {
            currentLevel.tiles = new List<LevelTile>(currentLevel.width * currentLevel.height);
            for (var i = 0; i < currentLevel.width; i++)
            {
                for (var j = 0; j < currentLevel.height; j++)
                {
                    currentLevel.tiles.Add(new BlockTile() { type = BlockType.RandomBlock });
                }
            }
        }

        for (var i = 0; i < currentLevel.height; i++)
        {
            GUILayout.BeginHorizontal();
            for (var j = 0; j < currentLevel.width; j++)
            {
                var tileIndex = (currentLevel.width * i) + j;
                CreateButton(tileIndex);
            }

            GUILayout.EndHorizontal();
        }
    }


    private void InitializeNewLevel()
    {
        foreach (var type in Enum.GetValues(typeof(ColorBlockType)))
        {
            currentLevel.availableColors.Add((ColorBlockType)type);
        }
    }


    private void CreateAvailableColorBlocksList()
    {
        availableColorBlocksList = SetupReorderableList("Color blocks", currentLevel.availableColors,
            ref currentColorBlock, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                    x.ToString());
            },
            (x) => { currentColorBlock = x; },
            () =>
            {
                var menu = new GenericMenu();
                foreach (var type in Enum.GetValues(typeof(ColorBlockType)))
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.ToString())), false,
                        CreateColorBlockTypeCallback, type);
                }

                menu.ShowAsContext();
            },
            (x) => { currentColorBlock = ColorBlockType.Blue; });
        availableColorBlocksList.onRemoveCallback = l =>
        {
            if (currentLevel.availableColors.Count == 1)
            {
                EditorUtility.DisplayDialog("Warning", "You need at least one color block type.", "Ok");
            }
            else
            {
                if (!EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete this item?", "Yes", "No"))
                {
                    return;
                }

                currentColorBlock = ColorBlockType.Blue;
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }

    private void CreateColorBlockTypeCallback(object obj)
    {
        var color = (ColorBlockType)obj;
        if (currentLevel.availableColors.Contains(color))
        {
            EditorUtility.DisplayDialog("Warning", "This color block type is already present in the list.", "Ok");
        }
        else
        {
            currentLevel.availableColors.Add(color);
        }
    }

    private void CreateButton(int tileIndex)
    {
        var tileTypeName = string.Empty;
        if (currentLevel.tiles[tileIndex] is BlockTile)
        {
            var blockTile = (BlockTile)currentLevel.tiles[tileIndex];
            tileTypeName = blockTile.type.ToString();
        }
        else if (currentLevel.tiles[tileIndex] is BoosterTile)
        {
            var boosterTile = (BoosterTile)currentLevel.tiles[tileIndex];
            tileTypeName = boosterTile.type.ToString();
        }

        if (tileTextures.ContainsKey(tileTypeName))
        {
            if (GUILayout.Button(tileTextures[tileTypeName], GUILayout.Width(60), GUILayout.Height(60)))
            {
                DrawTile(tileIndex);
            }
        }
        else
        {
            if (GUILayout.Button("", GUILayout.Width(60), GUILayout.Height(60)))
            {
                DrawTile(tileIndex);
            }
        }
    }


    private void DrawTile(int tileIndex)
    {
        var x = tileIndex % currentLevel.width;
        var y = tileIndex / currentLevel.width;
        if (currentBrushType == BrushType.Block)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    currentLevel.tiles[tileIndex] = new BlockTile { type = currentBlockType };
                    break;

                case BrushMode.Row:
                    for (var i = 0; i < currentLevel.width; i++)
                    {
                        var idx = i + (y * currentLevel.width);
                        currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                    }

                    break;

                case BrushMode.Column:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        var idx = x + (j * currentLevel.width);
                        currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                    }

                    break;

                case BrushMode.Fill:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                        }
                    }

                    break;
            }
        }
        else if (currentBrushType == BrushType.Booster)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    currentLevel.tiles[tileIndex] = new BoosterTile { type = currentBoosterType };
                    break;

                case BrushMode.Row:
                    for (var i = 0; i < currentLevel.width; i++)
                    {
                        var idx = i + (y * currentLevel.width);
                        currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                    }

                    break;

                case BrushMode.Column:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        var idx = x + (j * currentLevel.width);
                        currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                    }

                    break;

                case BrushMode.Fill:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                        }
                    }

                    break;
            }
        }
    }


    public void SaveLevel(string path)
    {
#if UNITY_EDITOR
        SaveJsonFile(path + "/Levels/" + currentLevel.id + ".json", currentLevel);
        AssetDatabase.Refresh();
#endif
    }
}