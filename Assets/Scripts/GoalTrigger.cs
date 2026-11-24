using Unity.VisualScripting;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GameTimer timer;
    [SerializeField] private LevelCompleteUI completeUI;
    private bool levelComplete;
  
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Entered: " + collision.name + ", tag: " + collision.tag);
        if(levelComplete) return;
        if (timer.HasExpired) return;
        if(!collision.CompareTag("Player")) return;
        levelComplete = true;
        Cursor.lockState = CursorLockMode.None;
        timer.StopTimer();
        if (GameAnalytics.Instance != null) GameAnalytics.Instance.LevelCompleted(timer.ElapsedTime);
        completeUI.Show(timer.ElapsedTime);
    }
}
