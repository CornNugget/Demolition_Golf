using UnityEngine;

[DisallowMultipleComponent]
public class FlagFloat : MonoBehaviour
{
    [Header("Flag Pole")]
    [SerializeField] private Transform flagPole;

    [Header("Floating Settings")]

    [SerializeField]
    [Min(0f)]
    private float floatHeight = 0.08f;

    [SerializeField]
    [Min(0f)]
    private float floatSpeed = 0.5f;

    private Vector3 initialLocalPosition;
    private Vector3 initialPoleLocalPosition;
    private float elapsedTime;

    private void OnEnable()
    {
        initialLocalPosition = transform.localPosition;

        if (flagPole != null)
        {
            initialPoleLocalPosition = flagPole.localPosition;
        }

        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime = elapsedTime + Time.deltaTime;

        float angle = elapsedTime * floatSpeed * Mathf.PI * 2f;
        float verticalOffset = Mathf.Sin(angle) * floatHeight;

        Vector3 nextPosition = initialLocalPosition;
        nextPosition.y = initialLocalPosition.y + verticalOffset;

        transform.localPosition = nextPosition;


        if (flagPole != null)
        {
            Vector3 nextPolePosition = initialPoleLocalPosition;
            nextPolePosition.y = initialPoleLocalPosition.y + verticalOffset;
            flagPole.localPosition = nextPolePosition;
        }
    }

    private void OnDisable()
    {
        transform.localPosition = initialLocalPosition;

        if (flagPole != null)
        {
            flagPole.localPosition = initialPoleLocalPosition;
        }
    }
}
