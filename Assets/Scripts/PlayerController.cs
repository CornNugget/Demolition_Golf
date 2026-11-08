using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves forward/backward and rotates with WASD/Arrow keys.
/// </summary>
public class PlayerController : MonoBehaviour
{

    public float shootForce = 5.0f;

    public float mouseSensitivityX = 100f;
    public float mouseSensitivityY = 100f;

    public float keySensitivityX = .7f;
    public float keySensitivityY = .7f;

    public bool aiming = false;
    private bool shoot = false;
    private Rigidbody rb;
    private Transform t;

    public bool powered = true;
    public Trajectory trajectory;

    public bool tester = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        t = GetComponent<Transform>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");
        // Keeps the cursor hidden and locked to the center
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Get the X and Y mouse movement values
        float xRot = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
        float yRot = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            yRot += keySensitivityY;
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            yRot -= keySensitivityY;
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            xRot -= keySensitivityX;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            xRot += keySensitivityX;
        }

        // Limit the amount the X and Y value can change
        yRot = Mathf.Clamp(yRot, -15f, 15f);
        xRot = Mathf.Clamp(xRot, -50f, 50f);

        // Update the euler angles to in the direction of the mouse movement
        Vector3 angles = t.eulerAngles;
        angles.x -= yRot;
        // Limit the up and down movement
        if(angles.x > 45.1f && angles.x < 360 - 45.1f)
        {
            if(yRot < 0)
            {
                angles.x = 45;
            }
            else
            {
                angles.x = 360 - 45;
            }
        }
        angles.y += xRot;
        t.eulerAngles = angles;

        if (Input.GetMouseButtonDown(0))
        {
            tester = true;
        }
        else
        {
            tester = false;
        }

        // When left click is pressed, switch aiming mode and wait for mouse to be released
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {

            // When pressed in aiming mode, set shoot to true
            if (aiming)
            {
                shoot = true;
            }

            aiming = !aiming;
        }

    }

    private void FixedUpdate()
    {
        // if shoot is true, shoot the ball forward and power it
        if (shoot)
        {
            shoot = !shoot;
            powered = true;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(t.forward * shootForce, ForceMode.VelocityChange);
            if (GameAnalytics.Instance != null) GameAnalytics.Instance.ShotTaken();
        }

        if (aiming)
        {
            trajectory.Draw(transform.position, t.forward*shootForce);
        }
        else
        {
            trajectory.Hide();
        }
    }
}
