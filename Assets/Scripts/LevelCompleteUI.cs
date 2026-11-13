using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TMP_Text finalTimeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        completePanel.SetActive(false);
    }
    public void Show(float finalTime)
    {
        string key = "LevelCompleted_" + SceneManager.GetActiveScene().name;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log("Saved progress: " + key);

        completePanel.SetActive(true);
        finalTimeText.text = "Your Timer: " + GameTimer.Format(finalTime);
        Time.timeScale = 0f;
    }
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
    // Update is called once per frame
    public void LevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
    }
}
