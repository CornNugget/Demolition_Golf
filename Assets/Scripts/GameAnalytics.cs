using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UnityConsent;

// Starts Unity Analytics and records the level's custom events.
// There's one in each level scene, so the counters start fresh every level.
public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance { get; private set; }

    private int shotsTaken;
    private int switchesActivated;

    private static bool Ready => UnityServices.State == ServicesInitializationState.Initialized;
    private static string LevelName => SceneManager.GetActiveScene().name;
    // The level timer starts when the scene loads, so this matches it
    private static float LevelTime => Time.timeSinceLevelLoad;

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

        if (!Ready) return;
        AnalyticsService.Instance.RecordEvent(new LevelStartedEvent { LevelName = LevelName });
    }

    public void ShotTaken()
    {
        shotsTaken++;
    }

    public void SwitchActivated(string switchName)
    {
        switchesActivated++;
        if (!Ready) return;

        AnalyticsService.Instance.RecordEvent(new SwitchActivatedEvent
        {
            LevelName = LevelName,
            SwitchName = switchName,
            SwitchOrder = switchesActivated,
            TimeSeconds = LevelTime
        });
    }

    public void MaterialUnlocked(BallMaterialType material)
    {
        if (!Ready) return;

        AnalyticsService.Instance.RecordEvent(new MaterialUnlockedEvent
        {
            LevelName = LevelName,
            Material = material.ToString(),
            TimeSeconds = LevelTime
        });
    }

    public void LevelCompleted(float clearTimeSeconds)
    {
        if (!Ready) return;

        LevelCompletedEvent levelCompleted = new LevelCompletedEvent
        {
            LevelName = LevelName,
            ClearTimeSecondsF = clearTimeSeconds,
            ShotsTaken = shotsTaken
        };
        AnalyticsService.Instance.RecordEvent(levelCompleted);
        AnalyticsService.Instance.Flush();
    }
}
