using UnityEngine;
using UnityEngine.UI;

public class MaterialUI : MonoBehaviour
{
    [SerializeField] private BallMaterialController ballController;

    // Same order as BallMaterialType: Regular, Stone, Diamond
    [SerializeField] private Image[] slotBackgrounds;
    [SerializeField] private GameObject[] lockIcons;

    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;

    void Update()
    {
        RefreshUI();   // checks every frame, so it catches level-ups and 1/2/3 key presses
    }

    void RefreshUI()
    {
        if (ballController == null) return;

        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            BallMaterialType type = (BallMaterialType)i;   // 0 = Regular, 1 = Stone, 2 = Diamond
            bool unlocked = ballController.IsMaterialUnlocked(type);
            bool selected = ballController.currentMaterial == type;

            // Show the lock only when the material is NOT unlocked
            lockIcons[i].SetActive(!unlocked);

            // Highlight the selected one, grey out locked ones
            if (selected)
                slotBackgrounds[i].color = selectedColor;
            else if (unlocked)
                slotBackgrounds[i].color = unlockedColor;
            else
                slotBackgrounds[i].color = lockedColor;
        }
    }
}