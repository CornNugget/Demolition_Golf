using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string levelSelectScene = "StartMenu";
    private bool isPaused;
    private bool previousCursorVisible;
    private CursorLockMode previousLock;


    void Start()
    {
        pausePanel.SetActive(false);

    }


    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current == null)return;
        if(Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    public void TogglePause()
    {
        if(isPaused) Resume();
        else Pause();
    }
    public void Pause()
    {
        if(Time.timeScale == 0f) return;
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;

    }
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelSelectScene);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
