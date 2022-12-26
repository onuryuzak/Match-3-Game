using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class LevelScene : BaseScene
{
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameObject scrollView;


    private void Awake()
    {
        Assert.IsNotNull(scrollRect);
        Assert.IsNotNull(scrollView);
    }


    private void Start()
    {
        scrollRect.vertical = false;

        var nextLevel = PlayerPrefs.GetInt("next_level");
        if (nextLevel == 0)
        {
            nextLevel = 1;
        }

        LevelButton currentButton = null;
        var levelButtons = scrollView.GetComponentsInChildren<LevelButton>();
        foreach (var button in levelButtons)
        {
            if (button.numLevel != nextLevel)
            {
                continue;
            }

            currentButton = button;
            break;
        }

        if (currentButton == null)
        {
            currentButton = levelButtons[levelButtons.Length - 1];
        }

        var newPos = scrollView.GetComponent<RectTransform>().anchoredPosition;
        newPos.y =
            scrollRect.transform.InverseTransformPoint(scrollView.GetComponent<RectTransform>().position).y -
            scrollRect.transform.InverseTransformPoint(currentButton.transform.position).y;
        if (newPos.y < scrollView.GetComponent<RectTransform>().anchoredPosition.y)
        {
            scrollView.GetComponent<RectTransform>().anchoredPosition = newPos;
        }

        var targetPos = currentButton.transform.position + new Vector3(0, 0.75f, 0);

        LevelButton prevButton = null;
        if (PuzzleMatchManager.instance.unlockedNextLevel)
        {
            foreach (var button in scrollView.GetComponentsInChildren<LevelButton>())
            {
                if (button.numLevel != PuzzleMatchManager.instance.lastSelectedLevel)
                {
                    continue;
                }

                prevButton = button;
                break;
            }
        }

        if (prevButton != null)
        {
            var sequence = LeanTween.sequence();
            sequence.append(0.5f);
            sequence.append(() => scrollRect.vertical = true);
        }
        else
        {
            scrollRect.vertical = true;
        }
    }
}