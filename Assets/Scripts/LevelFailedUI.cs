using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFailedUI : MonoBehaviour
{
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private string levelSelectScene = "LevelsMenu";

    private void Awake()
    {
        if (failedPanel != null)
        {
            failedPanel.SetActive(false);
        }
    }

    public void Show()
    {
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSelectScene);
    }
}
