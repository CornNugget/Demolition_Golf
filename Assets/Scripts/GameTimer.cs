using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private float elapsed;
    public float ElapsedTime => elapsed;
    private bool running = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        timerText.text = Format(elapsed);
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
