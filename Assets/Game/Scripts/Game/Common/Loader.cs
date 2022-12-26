using UnityEngine;


public class Loader : MonoBehaviour
{
    public PuzzleMatchManager gameManager;

    private void Awake()
    {
        if (PuzzleMatchManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }
}