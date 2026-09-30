using System.Text;
using TMPro;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{

    [SerializeField]
    private SwitchActivator[] switches;


    [SerializeField]
    private GameObject goalBlocker;


    [SerializeField]
    private TMP_Text statusText;

    private bool isGoalUnlocked = false;

    public bool IsGoalUnlocked
    {
        get
        {
            return isGoalUnlocked;
        }
        private set
        {
            isGoalUnlocked = value;
        }
    }

    private string previousText;


    private void Start()
    {
        IsGoalUnlocked = false;

        if (goalBlocker != null)
        {
            goalBlocker.SetActive(true);
        }

        RefreshStatus();
    }

    
    private void Update()
    {
        RefreshStatus();
    }

    private void RefreshStatus()
    {
       
        int total;

        if (switches == null)
        {
            total = 0;
        }
        else
        {
            total = switches.Length;
        }

       
        int activatedCount = 0;


        bool allActivated;

        if (total > 0)
        {
            allActivated = true;
        }
        else
        {
            allActivated = false;
        }


        StringBuilder rows = new StringBuilder();


        for (int i = 0; i < total; i++)
        {
            SwitchActivator currentSwitch = switches[i];
            int switchNumber = i + 1;


            if (currentSwitch == null)
            {
                allActivated = false;
                rows.AppendLine("Switch " + switchNumber + ": NOT ASSIGNED");
                continue;
            }

            bool activated = currentSwitch.IsActivated;
            string switchStateText;

            if (activated == true)
            {
                activatedCount = activatedCount + 1;
                switchStateText = "ON";
            }
            else
            {
                allActivated = false;
                switchStateText = "OFF";
            }

            rows.AppendLine("Switch " + switchNumber + ": " + switchStateText);
        }

        
        if (allActivated == true && IsGoalUnlocked == false)
        {
            IsGoalUnlocked = true;

            if (goalBlocker != null)
            {
                goalBlocker.SetActive(false);
            }

            Debug.Log("All switches activated. Goal unlocked!", this);
        }


        string goalStateText;

        if (IsGoalUnlocked == true)
        {
            goalStateText = "OPEN";
        }
        else
        {
            goalStateText = "LOCKED";
        }

        string text = "Switches: " + activatedCount + "/" + total + "\n";
        text = text + rows.ToString();
        text = text + "Goal: " + goalStateText;

    
        if (statusText != null && text != previousText)
        {
            statusText.text = text;
            previousText = text;
        }
    }
}
