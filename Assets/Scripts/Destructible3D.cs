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
    private int health;
    private bool isDestroyed = false;

    private void Awake()
    {
        obstacleRenderer = GetComponentInChildren<Renderer>();
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

        if (health <= 0)
        {
            isDestroyed = true;
            // Destroy the destructible
            Destroy(gameObject, .1f);
        }
    }
}
