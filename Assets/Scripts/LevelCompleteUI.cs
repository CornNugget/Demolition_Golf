using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private TMP_Text bestTimeText;

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
        finalTimeText.text = "Your Time: " + GameTimer.Format(finalTime);
        // Best time for this level
        string bestKey = "BestTime_" + SceneManager.GetActiveScene().name;
        bool hasBest = PlayerPrefs.HasKey(bestKey);                 // played this level before?
        float best = PlayerPrefs.GetFloat(bestKey, finalTime);
        bool isNewBest = hasBest && finalTime < best;

        // Save the time if it's the first one, or better than the old best
        if (!hasBest || isNewBest)
        {
            PlayerPrefs.SetFloat(bestKey, finalTime);
            PlayerPrefs.Save();
        }
        if (bestTimeText != null)
        {
            if (!hasBest)
                bestTimeText.text = "";                              // first time: just show "Your time"
            else if (isNewBest)
                bestTimeText.text = "New best time!";
            else
                bestTimeText.text = "Best: " + GameTimer.Format(best);
        }
                Time.timeScale = 0f;
                // Hide the Next Level button on the last level
                string next = LevelOrder.GetNext(SceneManager.GetActiveScene().name);
                if (nextLevelButton != null) nextLevelButton.SetActive(next != null);
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
        SceneManager.LoadScene("LevelsMenu");
    }
    public void NextLevel()
    {
        Time.timeScale = 1f;

        string next = LevelOrder.GetNext(SceneManager.GetActiveScene().name);
        if (next != null) SceneManager.LoadScene(next);
        else SceneManager.LoadScene("LevelsMenu");   // last level: back to level select
    }
}
