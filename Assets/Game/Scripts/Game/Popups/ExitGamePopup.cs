public class ExitGamePopup : Popup
{
    public void OnCloseButtonPressed()
    {
        Close();
    }

    public void OnExitButtonPressed()
    {
        GetComponent<SceneTransition>().PerformTransition();
    }

    /// <summary>
    /// Called when the resume button is pressed.
    /// </summary>
    public void OnResumeButtonPressed()
    {
        Close();
    }
}