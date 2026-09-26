using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
//Copied from the Unity Essentials Tutorial as a basis
public class PlayerController2D1 : MonoBehaviour
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
    private bool wait = false;
    private Camera cam;

    public bool powered = false;

    private void OnMouseDown()
    {

        //while (Input.GetMouseButtonDown(0)) { }
        

        if (!aiming)
        {
            speed = 0;
            rb.linearVelocity = movement * speed;
            wait = true;
            aiming = !aiming;
        }
        else
        {
            //if (!wait)
            //{
            //    Vector3 mousePos = Input.mousePosition;
            //    Vector3 mousePoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            //    Vector3 relativePos = mousePoint - transform.position;



            //    aiming = !aiming;
            //    speed = relativePos.magnitude;
            //    movement = relativePos.normalized;

            //}
            aiming = !aiming;
        }
        
    }
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
        cam = Camera.main;
    }

    void Update()
    {
        if (aiming)
        {
            //copied and edited from from unity documentation
            if (!wait)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 mousePoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
                Vector3 relativePos = mousePoint - transform.position;

                if (Input.GetMouseButtonDown(0))
                {
                    aiming = !aiming;
                    speed = relativePos.magnitude * speedScale;
                    movement = relativePos.normalized;
                    powered = true;

                    rb.AddForce(relativePos * speedScale);
                }
            }

            // Get player input from keyboard or controller


            //Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

            //float horizontalInput = moveInput.x;
            //float verticalInput = moveInput.y;

            //// Check if diagonal movement is allowed
            //if (canMoveDiagonally)
            //{
            //    // Set movement direction based on input
            //    movement = new Vector2(horizontalInput, verticalInput);
            //    // Optionally rotate the player based on movement direction
            //    RotatePlayer(horizontalInput, verticalInput);
            //}
            //else
            //{
            //    // Determine the priority of movement based on input
            //    if (horizontalInput != 0)
            //    {
            //        isMovingHorizontally = true;
            //    }
            //    else if (verticalInput != 0)
            //    {
            //        isMovingHorizontally = false;
            //    }

            //    // Set movement direction and optionally rotate the player
            //    if (isMovingHorizontally)
            //    {
            //        movement = new Vector2(horizontalInput, 0);
            //        RotatePlayer(horizontalInput, 0);
            //    }
            //    else
            //    {
            //        movement = new Vector2(0, verticalInput);
            //        RotatePlayer(0, verticalInput);
            //    }
            //}
        }
    }

    void FixedUpdate()
    {
        //    // Apply movement to the player in FixedUpdate for physics consistency
        //    rb.linearVelocity = movement * speed;
        //    if (speed >= .05)
        //    {
        //        speed = speed * slowdown;
        //    }
        //    else
        //    {
        //        speed = 0;
        //    }
        speed = rb.linearVelocity.magnitude;


    }

    void RotatePlayer(float x, float y)
    {
        // If there is no input, do not rotate the player
        if (x == 0 && y == 0) return;

        // Calculate the rotation angle based on input direction
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        // Apply the rotation to the player
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }
}
