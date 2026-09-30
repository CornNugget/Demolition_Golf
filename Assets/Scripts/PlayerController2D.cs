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
