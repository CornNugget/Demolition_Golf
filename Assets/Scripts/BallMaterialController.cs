using UnityEngine;
using UnityEngine.InputSystem;

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

    public BallMaterialType currentMaterial = BallMaterialType.Regular;
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
        ApplyBallType();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            currentMaterial = BallMaterialType.Regular;
            ApplyBallType();
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            currentMaterial = BallMaterialType.Stone;
            ApplyBallType();
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            currentMaterial = BallMaterialType.Diamond;
            ApplyBallType();
        }
    }

    public void UpgradeTo(BallMaterialType type)
    {
        if ((int)type <= (int)currentMaterial)
            return;

        currentMaterial = type;
        ApplyBallType();
    }

    private void ApplyBallType()
    {
        player = GetComponent<PlayerController>();
        switch (currentMaterial)
        {
            case BallMaterialType.Stone:
                ballRenderer.sharedMaterial = stoneMaterial;
                rb.mass = stoneMass;
                player.speed = stoneSpeed;
                player.jumpForce = stoneJump;
                break;

            case BallMaterialType.Diamond:
                ballRenderer.sharedMaterial = diamondMaterial;
                rb.mass = diamondMass;
                player.speed = diamondSpeed;
                player.jumpForce = diamondJump;
                break;

            default:
                ballRenderer.sharedMaterial = regularMaterial;
                rb.mass = regularMass;
                player.speed = regularSpeed;
                player.jumpForce = regularJump;
                break;
        }
    }
}
