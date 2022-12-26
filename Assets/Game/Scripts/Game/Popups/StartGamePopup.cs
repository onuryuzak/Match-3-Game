using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using FullSerializer;


public class StartGamePopup : Popup
{
    [SerializeField] private Text levelText;

    [SerializeField] private Sprite enabledStarSprite;

    [SerializeField] private Image star1Image;

    [SerializeField] private Image star2Image;

    [SerializeField] private Image star3Image;


    private int numLevel;


    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(levelText);
        Assert.IsNotNull(enabledStarSprite);
        Assert.IsNotNull(star1Image);
        Assert.IsNotNull(star2Image);
        Assert.IsNotNull(star3Image);
    }


    public void LoadLevelData(int levelNum)
    {
        numLevel = levelNum;

        var serializer = new fsSerializer();
        var level = FileUtils.LoadJsonFile<Level>(serializer, "Levels/" + numLevel);
        levelText.text = "Level " + numLevel;
        var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
        if (stars == 1)
        {
            star1Image.sprite = enabledStarSprite;
        }
        else if (stars == 2)
        {
            star1Image.sprite = enabledStarSprite;
            star2Image.sprite = enabledStarSprite;
        }
        else if (stars == 3)
        {
            star1Image.sprite = enabledStarSprite;
            star2Image.sprite = enabledStarSprite;
            star3Image.sprite = enabledStarSprite;
        }
    }

    public void OnPlayButtonPressed()
    {
        PuzzleMatchManager.instance.lastSelectedLevel = numLevel;
        GetComponent<SceneTransition>().PerformTransition();
    }

    public void OnCloseButtonPressed()
    {
        Close();
    }
}