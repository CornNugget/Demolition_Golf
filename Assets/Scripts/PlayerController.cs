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

    public bool aiming = false;
    public bool wait = false;
    private bool shoot = false;
    private Rigidbody rb;
    private Transform t;

    public bool powered = true;
    public Trajectory trajectory;

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

        // Limit the amount the Y value can change
        yRot = Mathf.Clamp(yRot, -15f, 15f);

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

        if (!wait)
        {
            // When left click is pressed, switch aiming mode and wait for mouse to be released
            if (Input.GetMouseButtonDown(0))
            {
                wait = true;

                // When pressed in aiming mode, set shoot to true
                if (aiming)
                {
                    shoot = true;
                }

                aiming = !aiming;
            }
        }
        else
        {
            // leave wait when mouse is released
            if (Input.GetMouseButtonUp(0))
            {
                wait = false;
            }
        }

        
    }

    private void FixedUpdate()
    {
        // if shoot is true, shoot the ball forward and power it
        if (shoot)
        {
            shoot = !shoot;
            powered = true;
            rb.AddForce(t.forward * shootForce, ForceMode.VelocityChange);
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
