using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum BallMaterialType
{
    Regular,
    Stone,
    Diamond
}

public class BallMaterialController : MonoBehaviour
{
    [SerializeField] private Renderer ballRenderer;

    [Header("Material")]
    [SerializeField] private Material regularMaterial;
    [SerializeField] private Material stoneMaterial;
    [SerializeField] private Material diamondMaterial;

    [Header("Mass")]
    [SerializeField] private float regularMass = 1f;
    [SerializeField] private float stoneMass = 3f;
    [SerializeField] private float diamondMass = 5f;

    [Header("Movement Speed")]
    [SerializeField] private float regularSpeed = 5f;
    [SerializeField] private float stoneSpeed = 3.5f;
    [SerializeField] private float diamondSpeed = 2.5f;

    [Header("Jump Force")]
    [SerializeField] private float regularJump = 5f;
    [SerializeField] private float stoneJump = 3.5f;
    [SerializeField] private float diamondJump = 2.5f;

    [Header("Damage")]
    [SerializeField] private int regularDamage = 1;
    [SerializeField] private int stoneDamage = 3;
    [SerializeField] private int diamondDamage = 10;

    [FormerlySerializedAs("currentMaterial")]
    [SerializeField] private BallMaterialType equippedMaterial = BallMaterialType.Regular;

    [Header("Level System")]
    [SerializeField] private BallLevelSystem levelSystem;
    public BallMaterialType currentMaterial
    {
        get
        {
            return equippedMaterial;
        }
    }

    public Vector3 LastVelocity { get; private set; } //used to record ball velocity before collision with a destructible object
    private PlayerController player;
    private Rigidbody rb;

    public int Damage
    {
        get
        {
            switch (currentMaterial)
            {
                case BallMaterialType.Stone:
                    return stoneDamage;

                case BallMaterialType.Diamond:
                    return diamondDamage;

                default:
                    return regularDamage;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (levelSystem == null)
        {
            levelSystem = GetComponent<BallLevelSystem>();
        }

        equippedMaterial = BallMaterialType.Regular;
        ApplyBallType();
    }

    private void Start()
    {
        if (levelSystem == null)
        {
            Debug.LogWarning("Add BallLevelSystem to the player to unlock Stone and Diamond.", this);
        }
    }

    private void FixedUpdate()
    {
        LastVelocity = rb.linearVelocity;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            TrySwitchMaterial(BallMaterialType.Regular);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            TrySwitchMaterial(BallMaterialType.Stone);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            TrySwitchMaterial(BallMaterialType.Diamond);
        }
    }

    public bool IsMaterialUnlocked(BallMaterialType type)
    {
        if (type == BallMaterialType.Regular)
        {
            return true;
        }

        if (levelSystem == null)
        {
            return false;
        }

        return levelSystem.IsMaterialUnlocked(type);
    }

    public bool TrySwitchMaterial(BallMaterialType type)
    {
        if (IsMaterialUnlocked(type) == false)
        {
            Debug.Log(type + " is not unlocked yet.", this);
            return false;
        }

        equippedMaterial = type;
        ApplyBallType();
        return true;
    }

    public void UpgradeTo(BallMaterialType type)
    {
        TrySwitchMaterial(type);
    }

    private void ApplyBallType()
    {
        player = GetComponent<PlayerController>();
        switch (currentMaterial)
        {
            case BallMaterialType.Stone:
                ballRenderer.sharedMaterial = stoneMaterial;
                rb.mass = stoneMass;
                //player.speed = stoneSpeed;
                player.shootForce = stoneJump;
                break;

            case BallMaterialType.Diamond:
                ballRenderer.sharedMaterial = diamondMaterial;
                rb.mass = diamondMass;
                //player.speed = diamondSpeed;
                player.shootForce = diamondJump;
                break;

            default:
                ballRenderer.sharedMaterial = regularMaterial;
                rb.mass = regularMass;
                //player.speed = regularSpeed;
                player.shootForce = regularJump;
                break;
        }
    }
}
