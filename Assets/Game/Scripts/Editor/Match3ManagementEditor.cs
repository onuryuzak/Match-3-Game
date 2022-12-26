using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Match3ManagementEditor : EditorWindow
{
    public GameConfiguration gameConfig;

    private readonly List<EditorTab> tabs = new List<EditorTab>();

    private int selectedTabIndex = -1;
    private int prevSelectedTabIndex = -1;


    [MenuItem("Tools/Match-3 Management/Editor", false, 0)]
    private static void Init()
    {
        var window = GetWindow(typeof(Match3ManagementEditor));
        window.titleContent = new GUIContent("Match-3 Management");
    }


    private void OnEnable()
    {
        tabs.Add(new GameSettingsTab(this));
        tabs.Add(new LevelEditorTab(this));
        selectedTabIndex = 0;
    }


    private void OnGUI()
    {
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
            new[] { "Game settings", "Level editor" });
        if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
        {
            var selectedEditor = tabs[selectedTabIndex];
            if (selectedTabIndex != prevSelectedTabIndex)
            {
                selectedEditor.OnTabSelected();
                GUI.FocusControl(null);
            }

            selectedEditor.Draw();
            prevSelectedTabIndex = selectedTabIndex;
        }
    }
}