using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class EndGamePopup : Popup
{
    [SerializeField] private Text levelText;

    [SerializeField] private Text scoreText;


    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(levelText);
        Assert.IsNotNull(scoreText);
    }

    public void OnReplayButtonPressed()
    {
        var gameScene = parentScene as GameScene;
        if (gameScene != null)
        {
            gameScene.RestartGame();
            Close();
        }
    }

    public void SetLevel(int level)
    {
        levelText.text = "Level " + level;
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }
}