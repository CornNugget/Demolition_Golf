using UnityEngine;

// Runs after CameraWallAvoidance, so the shake is added on top of the camera's final position
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float duration = 0.2f;   // how long one shake lasts (seconds)
    [SerializeField] private float strength = 0.1f;   // how far the camera moves

    private float timeLeft;

    void Awake()
    {
        Instance = this;
    }

    // Other scripts call this to start a shake
    public void Shake()
    {
        timeLeft = duration;
    }

    void LateUpdate()
    {
        if (timeLeft <= 0f) return;

        timeLeft -= Time.deltaTime;

        // Strongest at the start, fading to nothing
        float fade = timeLeft / duration;

        // A random small nudge up/down and left/right
        Vector2 offset = Random.insideUnitCircle * strength * fade;
        transform.position += transform.right * offset.x + transform.up * offset.y;
    }
}