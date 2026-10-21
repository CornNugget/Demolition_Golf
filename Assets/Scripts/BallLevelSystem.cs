using System;
using UnityEngine;

public class BallLevelSystem : MonoBehaviour
{

    public event Action<int> XPGained;

    [Header("Level Settings")]

    [SerializeField]
    [Min(1)]
    private int xpPerLevel = 100;

    [SerializeField]
    [Min(1)]
    private int maxLevel = 3;

    [Header("Material Unlock Levels")]

    [SerializeField]
    [Min(1)]
    private int stoneUnlockLevel = 2;

    [SerializeField]
    [Min(1)]
    private int diamondUnlockLevel = 3;

    [Header("Runtime Status")]

    [SerializeField]
    private int currentLevel = 1;

    [SerializeField]
    private int currentXP = 0;

    public int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
    }

    public int CurrentXP
    {
        get
        {
            return currentXP;
        }
    }

    public int XPPerLevel
    {
        get
        {
            return xpPerLevel;
        }
    }

    public bool IsMaxLevel
    {
        get
        {
            return currentLevel >= maxLevel;
        }
    }

    public int GetMaterialUnlockLevel(BallMaterialType type)
    {
        if (type == BallMaterialType.Regular)
        {
            return 1;
        }
        else if (type == BallMaterialType.Stone)
        {
            return stoneUnlockLevel;
        }
        else if (type == BallMaterialType.Diamond)
        {
            return diamondUnlockLevel;
        }

        return int.MaxValue;
    }

    public bool IsMaterialUnlocked(BallMaterialType type)
    {
        int requiredLevel = GetMaterialUnlockLevel(type);

        if (currentLevel >= requiredLevel)
        {
            return true;
        }

        return false;
    }

    private void Awake()
    {
        currentLevel = 1;
        currentXP = 0;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int requiredXP = Mathf.Max(1, xpPerLevel);


        if (IsMaxLevel == true)
        {
            int previousXP = currentXP;
            currentXP = Mathf.Min(currentXP + amount, requiredXP);

            int gainedXP = currentXP - previousXP;
            if (gainedXP > 0)
            {
                if (XPGained != null)
                {
                    XPGained(gainedXP);
                }
            }

            return;
        }

        int remainingXP = (maxLevel - currentLevel) * requiredXP;
        remainingXP = remainingXP + requiredXP - currentXP;
        int acceptedXP = Mathf.Min(amount, remainingXP);

        currentXP = currentXP + acceptedXP;

        Debug.Log("Gained " + acceptedXP + " XP.", this);

        while (currentXP >= requiredXP)
        {
            if (IsMaxLevel == true)
            {
                break;
            }

            currentXP = currentXP - requiredXP;

            currentLevel = currentLevel + 1;

            Debug.Log("Level up! Current level: " + currentLevel, this);
        }

        if (IsMaxLevel == true)
        {
            currentXP = Mathf.Min(currentXP, requiredXP);
        }

        if (XPGained != null)
        {
            XPGained(acceptedXP);
        }
    }

}
