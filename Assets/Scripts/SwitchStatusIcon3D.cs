using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SwitchStatusIcon3D : MonoBehaviour
{
    [SerializeField] private SwitchActivator3D targetSwitch;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private Image iconImage;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
        iconImage.preserveAspect = true;
    }

    private void Start()
    {
        RefreshIcon();
    }

    private void Update()
    {
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        bool activated = false;

        if (targetSwitch != null)
        {
            activated = targetSwitch.IsActivated;
        }

        Sprite nextSprite;

        if (activated == true)
        {
            nextSprite = unlockedSprite;
        }
        else
        {
            nextSprite = lockedSprite;
        }

        if (nextSprite == null)
        {
            return;
        }

        if (iconImage.sprite != nextSprite)
        {
            iconImage.sprite = nextSprite;
        }
    }
}