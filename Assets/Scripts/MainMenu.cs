using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;   // Level 1, 2, 3... in order
    [SerializeField] private string[] levelScenes;    // the scene name for each level, same order

    void Start()
    {
        bool tutorialDone = PlayerPrefs.GetInt("TutorialDone", 0) == 1;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            bool isUnlocked = levelNumber == 1 || tutorialDone;
            string buttonName = i == 0 ? "Tutorial" : "Level " + i;
            
            levelButtons[i].interactable = isUnlocked;

            TMP_Text label = levelButtons[i].GetComponentInChildren<TMP_Text>();
            label.text = isUnlocked ? "Level " + levelNumber : "Level " + levelNumber + " - Locked";

            string sceneName = levelScenes[i];
            levelButtons[i].onClick.AddListener(() => LoadLevel(sceneName));
        }
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit pressed");
        Application.Quit();
    }

    [ContextMenu("Reset Progress")]
    void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TutorialDone");
        Debug.Log("Progress reset");
    }
}