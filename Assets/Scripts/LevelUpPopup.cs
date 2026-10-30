using TMPro;
using UnityEngine;

public class LevelUpPopup : MonoBehaviour
{
    [SerializeField] private BallLevelSystem levelSystem;
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float showTime = 2.5f;

    private int lastLevel;
    private float timer;

    void Start()
    {
        if (levelSystem != null) lastLevel = levelSystem.CurrentLevel;
        popup.SetActive(false);
    }

    void Update()
    {
        if (levelSystem == null) return;

        // The level went up, so show the popup
        if (levelSystem.CurrentLevel > lastLevel)
        {
            lastLevel = levelSystem.CurrentLevel;
            popupText.text = GetMessage(lastLevel);
            popup.SetActive(true);
            timer = showTime;
        }

        // Count down, then hide it
        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f) popup.SetActive(false);
        }
    }

    string GetMessage(int level)
    {
        if (level == 2) return "Level 2!\nStone unlocked - Press 2";
        if (level == 3) return "Level 3!\nDiamond unlocked - Press 3";
        return "Level " + level + "!";
    }
}