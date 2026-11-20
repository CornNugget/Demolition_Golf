using UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraWallAvoidance : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("The player transform. If empty, uses this camera's parent.")]
    [SerializeField] private Transform player;

    [Tooltip("Local offset from the player used as the start of the wall check.")]
    [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 0.2f, 0f);

    [Header("Wall Detection")]
    [SerializeField] private LayerMask obstacleLayers = Physics.DefaultRaycastLayers;
    [SerializeField] [Min(0.01f)] private float checkRadius = 0.2f;
    [SerializeField] [Min(0f)] private float wallPadding = 0.05f;
    [SerializeField] [Min(0.1f)] private float returnSpeed = 4f;

    private Camera viewCamera;
    private Vector3 originalLocalPosition;
    private float currentDistance;

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();

        if (player == null)
        {
            player = transform.parent;
        }

        if (player == null)
        {
            Debug.LogWarning("CameraWallAvoidance: assign the player transform.", this);
            enabled = false;
            return;
        }

        originalLocalPosition = player.InverseTransformPoint(transform.position);
        Vector3 pivot = player.TransformPoint(pivotOffset);
        currentDistance = Vector3.Distance(pivot, transform.position);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 pivot = player.TransformPoint(pivotOffset);
        Vector3 desiredPosition = player.TransformPoint(originalLocalPosition);
        Vector3 cameraOffset = desiredPosition - pivot;
        float desiredDistance = cameraOffset.magnitude;

        if (desiredDistance <= 0.001f)
        {
            return;
        }

        Vector3 direction = cameraOffset / desiredDistance;

        // Cover the near-plane corners to reduce clipping at the edges of the view.
        float nearHeight = viewCamera.nearClipPlane *
            Mathf.Tan(viewCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        float nearWidth = nearHeight * viewCamera.aspect;
        float nearCornerRadius = Mathf.Sqrt(nearWidth * nearWidth + nearHeight * nearHeight);
        float radius = Mathf.Max(checkRadius, nearCornerRadius + 0.02f);

        RaycastHit[] hits = Physics.SphereCastAll(
            pivot, radius, direction, desiredDistance,
            obstacleLayers, QueryTriggerInteraction.Ignore);

        float safeDistance = desiredDistance;

        foreach (RaycastHit hit in hits)
        {
            // Ignore the player's colliders while checking solid obstacles.
            if (hit.collider.transform.IsChildOf(player))
            {
                continue;
            }

            float distanceToWall = Mathf.Max(0f, hit.distance - wallPadding);
            safeDistance = Mathf.Min(safeDistance, distanceToWall);
        }

        if (safeDistance < currentDistance)
        {
            // Move inward immediately to avoid clipping during the transition.
            currentDistance = safeDistance;
        }
        else
        {
            currentDistance = Mathf.MoveTowards(
                currentDistance, safeDistance, returnSpeed * Time.deltaTime);
        }

        // Preserve the camera's orientation and adjust only its distance.
        transform.position = pivot + direction * currentDistance;
    }
}
