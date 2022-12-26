using UnityEngine;


public class BackToLevelPopup : Popup
{
    protected override void Start()
    {
        base.Start();

        var settingsButton = GameObject.Find("SettingsButton");
        if (settingsButton != null)
        {
            settingsButton.transform.SetParent(GameObject.Find("Canvas").transform, false);
            settingsButton.GetComponent<RectTransform>().SetAsLastSibling();
        }
    }


    public void OnExitButtonPressed()
    {
        var settingsButton = GameObject.Find("SettingsButton");
        if (settingsButton != null)
        {
            settingsButton.transform.SetParent(GameObject.Find("BackgroundCanvas").transform);
            settingsButton.GetComponent<RectTransform>().SetAsLastSibling();
        }

        parentScene.CloseCurrentPopup();
        parentScene.OpenPopup<ExitGamePopup>("Popups/ExitGamePopup");
    }
}