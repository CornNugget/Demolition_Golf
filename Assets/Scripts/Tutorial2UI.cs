using TMPro;
using UnityEngine;

public class Tutorial2UI : MonoBehaviour
{
    [Header("Tutorial Text")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private BallLevelSystem levelSystem;

    [TextArea(1, 3)]
    [SerializeField] private string objectiveMessage =
        "Break obstacles. Earn XP. Unlock stronger materials.";

    [TextArea(2, 4)]
    [SerializeField] private string section1Message =
        "Destroy obstacles to earn XP. Watch the bar at the bottom - fill it to level up!";

    [TextArea(2, 4)]
    [SerializeField] private string section2Message =
        "Reach Level {level} to unlock Stone, then press 2 to equip it. Stone breaks wood more easily and can damage steel.";

    [TextArea(2, 4)]
    [SerializeField] private string stoneUnlockedMessage =
        "Stone unlocked! Press 2 to equip it and try it on wood or steel.";

    [TextArea(2, 4)]
    [SerializeField] private string section3Message =
        "Reach Level {level} to unlock Diamond, then press 3 to equip it. Use its higher damage to break steel and reach the exit!";

    [TextArea(2, 4)]
    [SerializeField] private string diamondUnlockedMessage =
        "Diamond unlocked! Press 3 to equip it. You can still press 1 or 2 to switch back.";

    [Header("Switch Panels")]
    [SerializeField] private GameObject section1Panel;
    [SerializeField] private GameObject section2Panel;
    [SerializeField] private GameObject section3Panel;

    private int currentSection = 0;

    public int CurrentSection
    {
        get
        {
            return currentSection;
        }
    }

    private void Start()
    {
        if (objectiveText != null)
        {
            objectiveText.text = objectiveMessage;
        }

        if (instructionText == null)
        {
            Debug.LogWarning("Tutorial2UI: assign Instruction Text to display section guidance.", this);
        }

        if (levelSystem == null)
        {
            Debug.LogWarning("Tutorial2UI: assign the player's BallLevelSystem to update material guidance.", this);
        }

        ShowSection(1);
    }

    private void Update()
    {
  
        RefreshInstruction();
    }

    public void ShowSection(int sectionNumber)
    {
        if (sectionNumber < 1 || sectionNumber > 3)
        {
            return;
        }

        if (sectionNumber == currentSection)
        {
            return;
        }

        currentSection = sectionNumber;
        RefreshInstruction();

        if (section1Panel != null)
        {
            if (sectionNumber == 1)
            {
                section1Panel.SetActive(true);
            }
            else
            {
                section1Panel.SetActive(false);
            }
        }

        if (section2Panel != null)
        {
            if (sectionNumber == 2)
            {
                section2Panel.SetActive(true);
            }
            else
            {
                section2Panel.SetActive(false);
            }
        }

        if (section3Panel != null)
        {
            if (sectionNumber == 3)
            {
                section3Panel.SetActive(true);
            }
            else
            {
                section3Panel.SetActive(false);
            }
        }
    }

    private void RefreshInstruction()
    {
        if (instructionText == null)
        {
            return;
        }

        string message;

        if (currentSection == 1)
        {
            message = section1Message;
        }
        else if (currentSection == 2)
        {
            if (levelSystem == null)
            {
                message = "Keep breaking objects to earn XP and unlock Stone.";
            }
            else if (levelSystem.IsMaterialUnlocked(BallMaterialType.Stone) == true)
            {
                message = stoneUnlockedMessage;
            }
            else
            {
                int unlockLevel = levelSystem.GetMaterialUnlockLevel(BallMaterialType.Stone);
                message = section2Message.Replace("{level}", unlockLevel.ToString());
            }
        }
        else if (currentSection == 3)
        {
            if (levelSystem == null)
            {
                message = "Keep breaking objects to earn XP and unlock Diamond.";
            }
            else if (levelSystem.IsMaterialUnlocked(BallMaterialType.Diamond) == true)
            {
                message = diamondUnlockedMessage;
            }
            else
            {
                int unlockLevel = levelSystem.GetMaterialUnlockLevel(BallMaterialType.Diamond);
                message = section3Message.Replace("{level}", unlockLevel.ToString());
            }
        }
        else
        {
            return;
        }

        if (instructionText.text != message)
        {
            instructionText.text = message;
        }
    }
}
