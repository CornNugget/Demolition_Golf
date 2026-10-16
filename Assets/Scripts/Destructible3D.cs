using Unity.VisualScripting;
using UnityEngine;

public enum ObstacleMaterialType
{
    Glass,
    Wood,
    Steel,
}

public class Destructible3D : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public GameObject onCollectEffect;

    [SerializeField] private ObstacleMaterialType obstacleType;
    [SerializeField] private Renderer obstacleRenderer;

    [Header("Materials")]
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material woodMaterial;
    [SerializeField] private Material steelMaterial;

    [Header("Health")]
    [SerializeField] private int glassHealth = 1;
    [SerializeField] private int woodHealth = 3;
    [SerializeField] private int steelHealth = 15;

    [Header("Destruction")]
    [Tooltip("How much of its speed the ball keeps after breaking this (0-1)")]
    [SerializeField] private float keepSpeed = 0.8f;
    [Tooltip("Mass of each broken piece")]
    [SerializeField] private float fragmentMass = 0.5f;
    [Tooltip("Speed given to the pieces, as a fraction of the ball's impact speed")]
    [SerializeField] private float burstStrength = 0.6f;
    [Tooltip("Tilts the burst upward so pieces tumble instead of sliding")]
    [SerializeField] private float burstLift = 0.3f;
    private Fracture fracture;
    private int health;
    private bool isDestroyed = false;

    private void Awake()
    {
        obstacleRenderer = GetComponentInChildren<Renderer>();
        fracture = GetComponent<Fracture>(); 
        if (fracture != null)
        {
            fracture.triggerOptions.triggerType = TriggerType.Keyboard;
            fracture.triggerOptions.triggerKey = KeyCode.None;
        }
        ApplyObstacleType();
    }

    private void ApplyObstacleType()
    {
        switch (obstacleType)
        {
            case ObstacleMaterialType.Wood:
                health = woodHealth;
                obstacleRenderer.sharedMaterial = woodMaterial;
                break;

            case ObstacleMaterialType.Steel:
                health = steelHealth;
                obstacleRenderer.sharedMaterial = steelMaterial;
                break;

            default:
                health = glassHealth;
                obstacleRenderer.sharedMaterial = glassMaterial;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        BallMaterialController ball = collision.collider.GetComponentInParent<BallMaterialController>();

        if (ball == null) return;
        if (obstacleType == ObstacleMaterialType.Steel && ball.currentMaterial == BallMaterialType.Regular) return;

        health -= ball.Damage;
        PlayerController player = ball.GetComponent<PlayerController>();
        if (player != null)
        {
            player.powered = false;
        }
        if (health <= 0)
        {
            Break(ball, collision); //function for physical destruction of object

            // Instantiate the particle effect
            //Instantiate(onCollectEffect, transform.position, transform.rotation);
        }
    }

        private void Break(BallMaterialController ball, Collision collision)
    {
        isDestroyed = true;

        Vector3 incoming = ball.LastVelocity;
        Vector3 hitPoint = collision.GetContact(0).point;
        float radius = obstacleRenderer.bounds.extents.magnitude * 2f;

        if (fracture != null)
        {
            Transform fragmentRoot = FractureAndFindFragments();
            if (fragmentRoot != null)
            {
                // Burst from just behind the hit point so pieces fly away from the ball
                Vector3 burstCenter = hitPoint - incoming.normalized * 0.5f;
                foreach (Rigidbody piece in fragmentRoot.GetComponentsInChildren<Rigidbody>())
                {
                    piece.mass = fragmentMass;
                    piece.AddExplosionForce(incoming.magnitude * burstStrength, burstCenter, radius, burstLift, ForceMode.VelocityChange);
                }
            }
        }

        // Return speed of golf ball so it can go through destroyed object
        if (collision.rigidbody != null)
        {
            collision.rigidbody.linearVelocity = incoming * keepSpeed;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private Transform FractureAndFindFragments()
    {
        // OpenFracture puts the pieces under a new "<name>Fragments" object next to this one,
        // but doesn't expose it, so find the child that appears
        Transform parent = transform.parent;
        if (parent == null)
        {
            fracture.CauseFracture();
            GameObject root = GameObject.Find(name + "Fragments");
            return root != null ? root.transform : null;
        }

        int childCountBefore = parent.childCount;
        fracture.CauseFracture();
        return parent.childCount > childCountBefore ? parent.GetChild(parent.childCount - 1) : null;
    }
}
