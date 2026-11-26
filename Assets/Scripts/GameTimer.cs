using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public enum TimerMode
    {
        CountUp = 0,
        CountDown = 1
    }

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TimerMode timerMode = TimerMode.CountUp;
    [SerializeField] [Min(1f)] private float durationSeconds = 120f;
    [SerializeField] private LevelFailedUI failedUI;

    private float elapsed;
    public float ElapsedTime => elapsed;
    public bool HasExpired { get; private set; }
    private bool running = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (running == false)
        {
            return;
        }

        elapsed += Time.deltaTime;

        if (timerMode == TimerMode.CountDown)
        {
            elapsed = Mathf.Min(elapsed, durationSeconds);
        }

        UpdateDisplay();

        if (timerMode == TimerMode.CountDown && elapsed >= durationSeconds)
        {
            running = false;
            HasExpired = true;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (failedUI != null)
            {
                failedUI.Show();
            }
            else
            {
                Debug.LogWarning("Assign Level Failed UI on the GameTimer component.", this);
            }
        }
    }

    private void UpdateDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        if (timerMode == TimerMode.CountDown)
        {
            // Round up so 00:00 appears only when the countdown has expired.
            float remaining = Mathf.Max(0f, durationSeconds - elapsed);
            timerText.text = Format(Mathf.CeilToInt(remaining));
        }
        else
        {
            timerText.text = Format(elapsed);
        }
    }

     public static string Format(float t)
    {
        int time = Mathf.FloorToInt(t);
        int minutes = time/ 60;
        int seconds = time % 60;
        return $"{minutes:00}:{seconds:00}";
    }
    public void StopTimer()
    {
        running = false;
    }
    
}
