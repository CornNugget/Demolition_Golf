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
    
    [Header("XP Rewards")]

    [SerializeField]
    [Min(0)]
    private int glassXP = 10;

    [SerializeField]
    [Min(0)]
    private int woodXP = 25;

    [SerializeField]
    [Min(0)]
    private int steelXP = 50;

    [Header("Destruction")]
    [Tooltip("How much of its speed the ball keeps after breaking this (0-1)")]
    [SerializeField] private float keepSpeed = 0.8f;
    [Tooltip("Mass of each broken piece")]
    [SerializeField] private float fragmentMass = 0.5f;
    [Tooltip("Speed given to the pieces, as a fraction of the ball's impact speed")]
    [SerializeField] private float burstStrength = 0.6f;
    [Tooltip("Tilts the burst upward so pieces tumble instead of sliding")]
    [SerializeField] private float burstLift = 0.3f;
    [Tooltip("Broken pieces per cubic unit of the object's size")]
    [SerializeField] private float piecesPerUnit = 0.5f;
    [Tooltip("Upper limit on pieces, to keep breaking fast")]
    [SerializeField] private int maxPieces = 60;
    [Tooltip("Pieces longer than this get broken again")]
    [SerializeField] private float maxPieceSize = 0.75f;
    [Tooltip("Fraction of broken pieces that stay in the level (0-1); the rest vanish on impact")]
    [Range(0f, 1f)]
    [SerializeField] private float piecesKept = 0.5f;
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

        // Use the object's own material for the broken faces unless one is set on the Fracture component
        if (fracture != null && fracture.fractureOptions.insideMaterial == null)
        {
            fracture.fractureOptions.insideMaterial = obstacleRenderer.sharedMaterial;
        }
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

    private void GiveXP(BallMaterialController ball)
    {
        // 升级组件需要与 BallMaterialController 挂在同一个对象上。
        BallLevelSystem levelSystem = ball.GetComponent<BallLevelSystem>();

        if (levelSystem == null)
        {
            Debug.LogWarning("The ball is missing BallLevelSystem.", ball);
            return;
        }

        int reward;

        if (obstacleType == ObstacleMaterialType.Glass)
        {
            reward = glassXP;
        }
        else if (obstacleType == ObstacleMaterialType.Wood)
        {
            reward = woodXP;
        }
        else if (obstacleType == ObstacleMaterialType.Steel)
        {
            reward = steelXP;
        }
        else
        {
            return;
        }

        levelSystem.AddXP(reward);
    }

    private void Break(BallMaterialController ball, Collision collision)
    {
        isDestroyed = true;
        GiveXP(ball);

        if (fracture == null)
        {
            Destroy(gameObject, .1f);
            return;
        }

        Vector3 incoming = ball.LastVelocity;
        Vector3 hitPoint = collision.GetContact(0).point;
        float radius = obstacleRenderer.bounds.extents.magnitude * 2f;

        Transform fragmentRoot = FractureAndFindFragments();
        if (fragmentRoot != null) BreakLargePieces(fragmentRoot);
        // Push the pieces' new positions to the physics engine before applying force
        Physics.SyncTransforms();
        if (fragmentRoot != null)
        {
            // Burst from just behind the hit point so pieces fly away from the ball
            Vector3 burstCenter = hitPoint - incoming.normalized * 0.5f;
            foreach (Rigidbody piece in fragmentRoot.GetComponentsInChildren<Rigidbody>())
            {
                // Drop some pieces so the debris isn't overwhelming; reads as the object crumbling to dust
                if (Random.value > piecesKept)
                {
                    Destroy(piece.gameObject);
                    continue;
                }

                piece.mass = fragmentMass;
                piece.AddExplosionForce(incoming.magnitude * burstStrength, burstCenter, radius, burstLift, ForceMode.VelocityChange);
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
        BakeScaleIntoMesh();

        // Bigger objects get more pieces so every object breaks into similar-sized chunks
        Vector3 size = GetComponent<MeshFilter>().sharedMesh.bounds.size;
        float volume = size.x * size.y * size.z;
        fracture.fractureOptions.fragmentCount = Mathf.Clamp(Mathf.RoundToInt(volume * piecesPerUnit), 8, maxPieces);

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

    private void BakeScaleIntoMesh()
    {
        // OpenFracture cuts in the mesh's unscaled shape, so on a stretched object every cut
        // ends up parallel to its biggest faces. Baking the stretch into the mesh evens out the cuts.
        Vector3 scale = transform.localScale;
        if (scale == Vector3.one) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = Instantiate(meshFilter.sharedMesh);
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], scale);
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        transform.localScale = Vector3.one;
    }

    private void BreakLargePieces(Transform fragmentRoot)
    {
        // Random cuts can leave long slabs, so keep re-breaking any piece that's still too big
        int pieceCount = fragmentRoot.GetComponentsInChildren<MeshFilter>().Length;

        for (int pass = 0; pass < 4; pass++)
        {
            MeshFilter[] pieces = fragmentRoot.GetComponentsInChildren<MeshFilter>();
            // Biggest first, so if we hit the cap, the pieces left over are the smaller ones
            System.Array.Sort(pieces, (a, b) => LongestSide(b).CompareTo(LongestSide(a)));

            bool brokeAny = false;
            foreach (MeshFilter piece in pieces)
            {
                float longest = LongestSide(piece);
                if (longest <= maxPieceSize) break;

                int count = Mathf.Min(Mathf.CeilToInt(longest / maxPieceSize) * 2, maxPieces - pieceCount + 1);
                if (count < 2) return;

                Fracture pieceFracture = piece.gameObject.AddComponent<Fracture>();
                pieceFracture.triggerOptions = fracture.triggerOptions;
                pieceFracture.refractureOptions = new RefractureOptions();
                pieceFracture.callbackOptions = new CallbackOptions();
                pieceFracture.fractureOptions = new FractureOptions
                {
                    fragmentCount = count,
                    insideMaterial = fracture.fractureOptions.insideMaterial,
                    textureScale = fracture.fractureOptions.textureScale,
                    textureOffset = fracture.fractureOptions.textureOffset,
                };
                pieceFracture.CauseFracture();
                Destroy(piece.gameObject);

                pieceCount += count - 1;
                brokeAny = true;
            }
            if (!brokeAny) return;
        }
    }

    private static float LongestSide(MeshFilter piece)
    {
        Vector3 size = piece.sharedMesh.bounds.size;
        return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
    }
}
