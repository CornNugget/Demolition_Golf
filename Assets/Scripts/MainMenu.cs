using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private string[] levelScenes;

    private void Start()
    {
        RefreshLevelButtons();
    }

    private void RefreshLevelButtons()
    {
        // StartMenu only uses the navigation methods and has no level arrays.
        if (levelButtons == null || levelScenes == null)
            return;

        int count = Mathf.Min(levelButtons.Length, levelScenes.Length);

        for (int i = 0; i < count; i++)
        {
            if (levelButtons[i] == null)
                continue;

            // Level 1 is always available. Later levels require
            // completion of the immediately preceding scene.
            levelButtons[i].interactable =
                i == 0 || IsCompleted(levelScenes[i - 1]);
        }
    }

    private bool IsCompleted(string sceneName)
    {
        return PlayerPrefs.GetInt("LevelCompleted_" + sceneName, 0) == 1;
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        foreach (string sceneName in levelScenes)
        {
            PlayerPrefs.DeleteKey("LevelCompleted_" + sceneName);
        }

        PlayerPrefs.Save();
        RefreshLevelButtons();
        Debug.Log("Level progress reset.");
    }
}
