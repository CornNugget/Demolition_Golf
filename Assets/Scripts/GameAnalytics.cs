using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UnityConsent;

// Starts Unity Analytics and records how long it takes to get the ball in the hole.
public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Analytics only collects data once consent is granted
        EndUserConsent.SetConsentState(new ConsentState {
            AnalyticsIntent = ConsentStatus.Granted,
            AdsIntent = ConsentStatus.Denied
        });
    }

    public void LevelCompleted(float clearTimeSeconds)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;

        LevelCompletedEvent levelCompleted = new LevelCompletedEvent
        {
            LevelName = SceneManager.GetActiveScene().name,
            ClearTimeSecondsF = clearTimeSeconds
        };
        AnalyticsService.Instance.RecordEvent(levelCompleted);
        AnalyticsService.Instance.Flush();
    }
}
