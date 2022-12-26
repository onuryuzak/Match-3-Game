using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WinPopup : EndGamePopup
{
    [SerializeField] private Image star1;

    [SerializeField] private Image star2;

    [SerializeField] private Image star3;


    [SerializeField] private Sprite disabledStarSprite;


    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(star1);
        Assert.IsNotNull(star2);
        Assert.IsNotNull(star3);
        Assert.IsNotNull(disabledStarSprite);
    }

    public void SetStars(int stars)
    {
        if (stars == 0)
        {
            star1.sprite = disabledStarSprite;
            star2.sprite = disabledStarSprite;
            star3.sprite = disabledStarSprite;
        }
        else if (stars == 1)
        {
            star2.sprite = disabledStarSprite;
            star3.sprite = disabledStarSprite;
        }
        else if (stars == 2)
        {
            star3.sprite = disabledStarSprite;
        }
    }
}