using UnityEngine;

public class SwitchManager3D : MonoBehaviour
{
    [SerializeField]
    private SwitchActivator3D[] switches;

    [SerializeField]
    private GameObject goalBlocker;

    private bool isGoalUnlocked = false;

    public bool IsGoalUnlocked
    {
        get
        {
            return isGoalUnlocked;
        }
    }

    private void Start()
    {
        isGoalUnlocked = false;

        if (goalBlocker != null)
        {
            goalBlocker.SetActive(true);
        }
    }

    private void Update()
    {
        if (isGoalUnlocked == true)
        {
            return;
        }

        CheckAllSwitches();
    }

    private void CheckAllSwitches()
    {
        if (switches == null)
        {
            return;
        }

        if (switches.Length == 0)
        {
            return;
        }

        for (int i = 0; i < switches.Length; i++)
        {
            SwitchActivator3D currentSwitch = switches[i];

            if (currentSwitch == null)
            {
                return;
            }

            if (currentSwitch.IsActivated == false)
            {
                return;
            }
        }

        UnlockGoal();
    }

    private void UnlockGoal()
    {
        isGoalUnlocked = true;

        if (goalBlocker != null)
        {
            goalBlocker.SetActive(false);
        }

        Debug.Log("All switches activated. Goal unlocked!", this);
    }
}