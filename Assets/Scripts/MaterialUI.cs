using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialUI : MonoBehaviour
{
    [SerializeField] private BallMaterialController ballController;

    // Same order as BallMaterialType: Regular, Stone, Diamond
    [SerializeField] private Image[] slotBackgrounds;
    [SerializeField] private GameObject[] lockIcons;
    [SerializeField] private TMP_Text[] labels;

    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f);

    void Update()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (ballController == null) return;

        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            BallMaterialType type = (BallMaterialType)i;
            bool unlocked = ballController.IsMaterialUnlocked(type);
            bool selected = ballController.currentMaterial == type;

            lockIcons[i].SetActive(!unlocked);

            if (selected)
                slotBackgrounds[i].color = selectedColor;
            else if (unlocked)
                slotBackgrounds[i].color = unlockedColor;
            else
                slotBackgrounds[i].color = lockedColor;

            // The loop counts 0,1,2 but the keys are 1,2,3, so add 1
            if (i < labels.Length && labels[i] != null)
            labels[i].text = selected ? type + "\nSelected" : "Press " + (i + 1) + "\n" + type;
        }
    }
}