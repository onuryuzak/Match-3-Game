using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class NoMovesPopup : Popup
{
    [SerializeField] private Text title1Text;

    private GameScene gameScene;


    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(title1Text);
    }

    public void SetGameScene(GameScene scene)
    {
        gameScene = scene;
        if (gameScene.level.limitType == LimitType.Moves)
        {
            title1Text.text = "Out of moves!";
        }
    }

    public void OnExitButtonPressed()
    {
        Close();
        gameScene.OpenLosePopup();
    }
}