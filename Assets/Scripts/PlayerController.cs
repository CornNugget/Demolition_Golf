using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves forward/backward and rotates with WASD/Arrow keys.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Tooltip("Forward/back speed (units/sec).")]
    public float speed = 5.0f;

    [Tooltip("Turn speed (degrees/sec).")]
    public float rotationSpeed = 120.0f;

    public float jumpForce = 5.0f;

    public float mouseSensitivityX = 100f;
    public float mouseSensitivityY = 100f;

    private Rigidbody rb;

    public bool powered = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
        float xRot = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
        float yRot = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;

        yRot = Mathf.Clamp(yRot, -15f, 15f);
        //xRot = 0f;
        Transform t = GetComponent<Transform>();
        Vector3 angles = t.eulerAngles;
        angles.x -= yRot;
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
        //t.rotation.x += yRot;
        //t.Rotate(0f, xRot, 0f, Space.World);
        //t.Rotate(yRot, 0f, 0f, Space.World);

        //Quaternion turnRotation = Quaternion.Euler(yRot,xRot,0f);
        //rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void FixedUpdate()
    {
        powered = true;
        Vector2 moveInput = Vector2.zero;
        
        // Forward/backward
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;

        // Left/right (rotation)
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;

        // Move in facing direction 
        Vector3 movement = transform.forward * moveInput.y * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Y-axis rotation (invert when going backwards)
        float turnDirection = moveInput.x;
        if (moveInput.y < 0)
            turnDirection = -turnDirection;

        float turn = turnDirection * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}

/*
 using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
//Copied from the Unity Essentials Tutorial as a basis
public class PlayerController2D : MonoBehaviour
{
    // Public variables
    public float speed = 0f; // The speed at which the player moves
    public float speedScale = 2f;
    //public float slowdown = .99f;
    //public bool canMoveDiagonally = true; // Controls whether the player can move diagonally

    public InputActionReference moveAction;

    // Private variables 
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private Vector2 movement; // Stores the direction of player movement
    //private bool isMovingHorizontally = true; // Flag to track if the player is moving horizontally
    public bool aiming = false;
    public bool wait = false;
    private Camera cam;

    public bool powered = false;
    private GameObject arrow;


  
    private void OnMouseDown()
    {
        //if the ball is clicked when not in aiming mode, stop the ball and enter aiming mode
        //enabling the arrow
        if (!aiming)
        {
            speed = 0;
            rb.linearVelocity = movement * speed;
            wait = true;
            aiming = !aiming;
            GetComponent<Renderer>().material.color = Color.white;
            arrow.SetActive(true);
        }
        //if the ball is clicked while in aiming mode, exit aiming mode
        else
        {

            aiming = !aiming;
            arrow.SetActive(false);
        }
        
    }
    //don't do anything until the mouse is let go of
    private void OnMouseUp()
    {
        wait = false;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    void Start()
    {
        // Initialize the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        // Prevent the player from rotating
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //get the camera and the arrow object on start
        cam = Camera.main;
        arrow = GameObject.FindWithTag("Arrow");
        arrow.SetActive(false);
    }

    void Update()
    {

        // don't detect mouse presses until the mouse is released after the ball is clicked
        if (aiming && !wait)
        {
            //copied and edited from from unity documentation
            Vector3 mousePos = Input.mousePosition;
            Vector3 mousePoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            Vector3 relativePos = mousePoint - transform.position;
            relativePos.z = 0;
            //if the mouse is clicked, shoot the ball in the direction of the mouse
            if (Input.GetMouseButtonDown(0))
            {
                aiming = !aiming;
                speed = relativePos.magnitude * speedScale;
                movement = relativePos.normalized;
                powered = true;
                GetComponent<Renderer>().material.color = Color.yellow;

                rb.AddForce(relativePos * speedScale);
                arrow.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }
}

 */