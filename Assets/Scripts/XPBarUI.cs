using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] private BallLevelSystem levelSystem;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;

    [Header("XP Gain Message")]
    [SerializeField] private TMP_Text gainedXPText;
    [SerializeField] [Min(0.1f)] private float messageDuration = 1.5f;

    [Header("Bar Animation")]
    [SerializeField] [Min(0.1f)] private float fillSpeed = 0.5f;

    private float messageTimeRemaining;
    private int previousLevel;

    private void Awake()
    {
        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.wholeNumbers = false;

            xpSlider.interactable = false;
            xpSlider.transition = Selectable.Transition.None;
            xpSlider.direction = Slider.Direction.LeftToRight;

            if (xpSlider.fillRect != null)
            {
                Image fillImage = xpSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.type = Image.Type.Simple;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (levelSystem != null)
        {
            levelSystem.XPGained += ShowXPGain;
        }
    }

    private void OnDisable()
    {
        if (levelSystem != null)
        {
            levelSystem.XPGained -= ShowXPGain;
        }

        if (gainedXPText != null)
        {
            gainedXPText.text = "";
        }
        messageTimeRemaining = 0f;
    }

    private void Start()
    {
        if (levelSystem == null)
        {
            Debug.LogWarning("XPBarUI: assign the player's BallLevelSystem.", this);
        }

        if (xpSlider == null)
        {
            Debug.LogWarning("XPBarUI: assign the XP Slider.", this);
        }
        else if (xpSlider.fillRect == null)
        {
            Debug.LogWarning("XPBarUI: assign Fill to the Slider's Fill Rect.", this);
        }

        if (levelSystem != null)
        {
            previousLevel = levelSystem.CurrentLevel;

            if (xpSlider != null)
            {
                float progress = (float)levelSystem.CurrentXP /
                    Mathf.Max(1, levelSystem.XPPerLevel);
                xpSlider.value = Mathf.Clamp01(progress);
            }
        }

        if (gainedXPText != null)
        {
            gainedXPText.text = "";
        }
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
        UpdateGainMessage();
    }

    private void RefreshUI()
    {
        if (levelSystem == null)
        {
            return;
        }

        int currentXP = levelSystem.CurrentXP;
        int requiredXP = Mathf.Max(1, levelSystem.XPPerLevel);

        if (xpSlider != null)
        {
            float progress = (float)currentXP / requiredXP;

            if (previousLevel != levelSystem.CurrentLevel)
            {
                xpSlider.value = 0f;
            }

            xpSlider.value = Mathf.MoveTowards(
                xpSlider.value,
                Mathf.Clamp01(progress),
                fillSpeed * Time.unscaledDeltaTime);
        }

        previousLevel = levelSystem.CurrentLevel;

        if (levelText != null)
        {
            if (levelSystem.IsMaxLevel == true)
            {
                levelText.text = "Level " + levelSystem.CurrentLevel + " (MAX)";
            }
            else
            {
                levelText.text = "Level " + levelSystem.CurrentLevel;
            }
        }

        if (xpText != null)
        {
            xpText.text = currentXP + " / " + requiredXP + " XP";
        }
    }

    private void ShowXPGain(int amount)
    {
        if (gainedXPText == null)
        {
            return;
        }

        gainedXPText.text = "+" + amount + " XP";
        messageTimeRemaining = messageDuration;
    }

    private void UpdateGainMessage()
    {
        if (messageTimeRemaining <= 0f)
        {
            return;
        }

        messageTimeRemaining = messageTimeRemaining - Time.unscaledDeltaTime;

        if (messageTimeRemaining <= 0f)
        {
            if (gainedXPText != null)
            {
                gainedXPText.text = "";
            }
        }
    }
}
