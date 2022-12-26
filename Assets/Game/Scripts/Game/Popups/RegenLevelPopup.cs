using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class RegenLevelPopup : Popup
{
    [SerializeField] private Text text;


    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(text);
    }


    protected override void Start()
    {
        base.Start();
        StartCoroutine(AnimateText());
        StartCoroutine(AutoKill());
    }


    private IEnumerator AnimateText()
    {
        for (var i = 0; i < 100; i++)
        {
            text.text = "Regenerating level.";
            yield return new WaitForSeconds(0.4f);
            text.text = "Regenerating level..";
            yield return new WaitForSeconds(0.4f);
            text.text = "Regenerating level...";
            yield return new WaitForSeconds(0.4f);
        }
    }


    private IEnumerator AutoKill()
    {
        yield return new WaitForSeconds(3.0f);
        Close();
    }
}