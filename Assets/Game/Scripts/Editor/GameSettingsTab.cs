using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


public class GameSettingsTab : EditorTab
{
    private ReorderableList tileScoreOverridesList;
    private TileScoreOverride currentTileScoreOverride;

    private ReorderableList resolutionOverridesList;
    private ResolutionOverride currentResolutionOverride;

    private const string editorPrefsName = "game_settings_path";

    private int selectedTabIndex;

    private Vector2 scrollPos;

    private int newLevel;

    public GameSettingsTab(Match3ManagementEditor editor) : base(editor)
    {
        if (EditorPrefs.HasKey(editorPrefsName))
        {
            var path = EditorPrefs.GetString(editorPrefsName);
            parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
            if (parentEditor.gameConfig != null)
            {
                CreateTileScoreOverridesList();
            }
        }

        newLevel = PlayerPrefs.GetInt("next_level");
    }


    public override void Draw()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 100;

        GUILayout.Space(15);

        DrawMenu();

        GUILayout.Space(15);

        var prevSelectedIndex = selectedTabIndex;
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
            new[] { "Game", "Player preferences" }, GUILayout.Width(500));

        if (selectedTabIndex != prevSelectedIndex)
        {
            GUI.FocusControl(null);
        }

        if (selectedTabIndex == 0)
        {
            DrawGameTab();
        }
        else
        {
            DrawPreferencesTab();
        }

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndScrollView();
    }


    private void DrawGameTab()
    {
        if (parentEditor.gameConfig != null)
        {
            GUILayout.Space(15);

            DrawBoosterSettings();

            GUILayout.Space(15);
        }
    }


    private void DrawPreferencesTab()
    {
        if (parentEditor.gameConfig != null)
        {
            GUILayout.Space(15);

            DrawPreferencesSettings();
        }
    }


    private void DrawMenu()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("New", GUILayout.Width(100), GUILayout.Height(50)))
        {
            parentEditor.gameConfig = new GameConfiguration();
            CreateTileScoreOverridesList();
        }

        if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(50)))
        {
            var path = EditorUtility.OpenFilePanel("Open game configuration",
                Application.dataPath + "/Resources",
                "json");
            if (!string.IsNullOrEmpty(path))
            {
                parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
                if (parentEditor.gameConfig != null)
                {
                    CreateTileScoreOverridesList();
                    EditorPrefs.SetString(editorPrefsName, path);
                }
            }
        }

        if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50)))
        {
            SaveGameConfiguration(Application.dataPath + "/Resources");
        }

        GUILayout.EndHorizontal();
    }


    private void DrawBoosterSettings()
    {
        var gameConfig = parentEditor.gameConfig;

        EditorGUILayout.LabelField("Boosters", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        EditorGUILayout.HelpBox(
            "Enter the number of matches that need to occur in order for the booster to appear.", MessageType.Info);
        GUILayout.EndHorizontal();

        foreach (var booster in Enum.GetValues(typeof(BoosterType)))
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(StringUtils.DisplayCamelCaseString(booster.ToString()));
            var neededMatches = gameConfig.boosterNeededMatches;
            neededMatches[(BoosterType)booster] =
                EditorGUILayout.IntField(neededMatches[(BoosterType)booster], GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }
    }


    private void DrawPreferencesSettings()
    {
        EditorGUILayout.LabelField("PlayerPrefs", EditorStyles.boldLabel);
        if (GUILayout.Button("Delete PlayerPrefs", GUILayout.Width(120), GUILayout.Height(30)))
        {
            PlayerPrefs.DeleteAll();
        }

        GUILayout.Space(15);

        EditorGUILayout.LabelField("EditorPrefs", EditorStyles.boldLabel);
        if (GUILayout.Button("Delete EditorPrefs", GUILayout.Width(120), GUILayout.Height(30)))
        {
            EditorPrefs.DeleteAll();
        }
    }


    private void CreateTileScoreOverridesList()
    {
        tileScoreOverridesList = SetupReorderableList("Score overrides", parentEditor.gameConfig.tileScoreOverrides,
            ref currentTileScoreOverride,
            (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.ToString());
            },
            (x) => { currentTileScoreOverride = x; },
            () =>
            {
                var menu = new GenericMenu();


                menu.ShowAsContext();
            },
            (x) => { currentTileScoreOverride = null; });
    }


    public void SaveGameConfiguration(string path)
    {
#if UNITY_EDITOR
        var fullPath = path + "/game_configuration.json";
        SaveJsonFile(fullPath, parentEditor.gameConfig);
        EditorPrefs.SetString(editorPrefsName, fullPath);
        AssetDatabase.Refresh();
#endif
    }
}