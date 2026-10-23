using Unity.VisualScripting;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GameTimer timer;
    [SerializeField] private LevelCompleteUI completeUI;
    private bool levelComplete;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Entered: " + collision.name + ", tag: " + collision.tag);
        if(levelComplete) return;
        if(!collision.CompareTag("Player")) return;
        levelComplete = true;
        Cursor.lockState = CursorLockMode.None;
        timer.StopTimer();
        completeUI.Show(timer.ElapsedTime);
    }
}
