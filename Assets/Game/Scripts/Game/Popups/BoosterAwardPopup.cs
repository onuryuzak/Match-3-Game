using System.Collections;
using UnityEngine;


public class BoosterAwardPopup : Popup
{
    protected override void Start()
    {
        base.Start();
        StartCoroutine(AutoClose());
    }


    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(1.5f);
        Close();
    }
}