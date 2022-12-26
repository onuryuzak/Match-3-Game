using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class AlertPopup : Popup
{
    [SerializeField] private Text titleText;

    [SerializeField] private Text bodyText;

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(titleText);
        Assert.IsNotNull(bodyText);
    }


    public void OnButtonPressed()
    {
        Close();
    }


    public void OnCloseButtonPressed()
    {
        Close();
    }

    public void SetTitle(string text)
    {
        titleText.text = text;
    }


    public void SetText(string text)
    {
        bodyText.text = text;
    }
}