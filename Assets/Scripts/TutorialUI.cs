using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject switchPanel;
    [SerializeField] private GameObject switchHintText;
    [SerializeField] private float hintDuration = 3f;
    private float hintTimeRemaining;
    [TextArea(2, 4)]
    [SerializeField] private string section1Text =
        "Move the mouse and left-click once to aim. Left-click again to shoot.";

    [TextArea(2, 4)]
    [SerializeField] private string section2Text =
        "Some objects require multiple hits to be destroyed.";

    [TextArea(2, 4)]
    [SerializeField] private string section3Text =
        "This indicates the remaining switches to unlock the final gate. Try it yourself.";

    private int currentSection = 0;

    private void Start()
    {
        objectiveText.text =
            "Objective: Unlock switches to open gates and exit";

        if (switchPanel != null)
        {
            switchPanel.SetActive(false);
        }
        if (switchHintText != null)
        {
            switchHintText.SetActive(false);
        }

        ShowSection(1);

    }

    private void Update()
    {
        if (hintTimeRemaining <= 0f)
        {
            return;
        }

        hintTimeRemaining -= Time.deltaTime;

        if (hintTimeRemaining <= 0f)
        {
            if (switchHintText != null)
            {
                switchHintText.SetActive(false);
            }
        }
    }

    public void ShowSection(int sectionNumber)
    {
        
        if (sectionNumber <= currentSection)
        {
            return;
        }

        if (sectionNumber < 1 || sectionNumber > 3)
        {
            return;
        }

        currentSection = sectionNumber;

        if (sectionNumber == 1)
        {
            instructionText.text = section1Text;
        }
        else if (sectionNumber == 2)
        {
            instructionText.text = section2Text;
        }
        else if (sectionNumber == 3)
        {
            instructionText.text = section3Text;
        }

        if (switchPanel != null)
        {
            if (sectionNumber == 3)
            {
                switchPanel.SetActive(true);
            }
            else
            {
                switchPanel.SetActive(false);
            }
        }

        if (switchHintText != null)
        {
            if (sectionNumber == 3)
            {
                switchHintText.SetActive(true);
                hintTimeRemaining = hintDuration;
            }
            else
            {
                switchHintText.SetActive(false);
                hintTimeRemaining = 0f;
            }
        }
    }
}