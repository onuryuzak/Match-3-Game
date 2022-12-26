using UnityEngine;
using UnityEngine.UI;


public class ProgressStar : MonoBehaviour
{
    public Image image;
    public Sprite onSprite;
    public Sprite offSprite;


    private void Awake()
    {
        image.sprite = offSprite;
    }


    public void Activate()
    {
        image.sprite = onSprite;
    }
}