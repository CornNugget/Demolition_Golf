using Unity.VisualScripting;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GameTimer timer;
    [SerializeField] private LevelCompleteUI completeUI;
    private bool levelComplete;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(levelComplete) return;
        if(!collision.CompareTag("Player")) return;
        levelComplete = true;
        timer.StopTimer();
        completeUI.Show(timer.ElapsedTime);
    }
}
