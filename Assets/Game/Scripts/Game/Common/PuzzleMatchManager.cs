using UnityEngine;


public class PuzzleMatchManager : MonoBehaviour
{
    public static PuzzleMatchManager instance;

    public GameConfiguration gameConfig;

    public int lastSelectedLevel;
    public bool unlockedNextLevel;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}