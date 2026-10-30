using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Loads a level by its scene name (typed into the button's On Click box)
    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;   // in case a previous level was paused
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit pressed");   // Quit does nothing in the editor, so this proves it works
        Application.Quit();
    }
}