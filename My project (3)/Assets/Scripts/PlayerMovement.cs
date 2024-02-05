using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this line

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3.0F;
    public float sprintSpeed = 6.0F; // The speed when sprinting
    public float rotateSpeed = 3.0F;
    public float jumpHeight = 10.0f;
    public float gravityValue = -9.81f;
    public LayerMask groundLayer; // Layer to represent the ground
    public float groundDistance = 0.2f; // Distance to check for ground
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private CharacterController controller;
    private Transform groundCheck; // Transform to represent the point from where to check for ground
    public Camera playerCamera; // Add this line
    public float crouchHeight = 0.5f; // The height of the controller when crouching
    private float originalHeight; // The original height of the controller
    public LayerMask ladderMask; // Layer to represent the ladder
    private bool isClimbing;
    public LayerMask platformMask; // Add this line at the top of your script

    // Variables for mouse look
    public float mouseSensitivity = 100.0f;
    public Transform playerBody;
    private float xRotation = 0.0f;
    
    public float health = 100f; // Player's health
    public TextMeshProUGUI deathText; // Reference to the TextMeshProUGUI object
    public Vector3 respawnPosition; // Position where the player will respawn
    private bool isDead = false; // Whether the player is dead

    public Item[] items; // The items the player can hold
    public int currentItem = 0; // The index of the currently held item


    [System.Serializable]
    public class Item
    {
        public string name;
        public int id;
        public Sprite icon;
        // Add other properties as needed...
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundCheck = transform.GetChild(0); // Assuming the groundCheck transform is the first child of the player
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        originalHeight = controller.height; // Save the original height of the controller
        // Set the respawn position to the player's initial position
        respawnPosition = transform.position;


        Item newItem = new Item();
        newItem.name = "New Item";
        newItem.id = 1;

        List<Item> itemList = new List<Item>(items);
        itemList.Add(newItem);
        items = itemList.ToArray();
    }

    // Update is called once per frame
    void Update()
    {


        groundedPlayer = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveHorizontal, 0, moveVertical);
        move = playerBody.transform.TransformDirection(move); // Make the movement relative to the player's rotation

        // Check if the sprint key is held down
        if (Input.GetKey(KeyCode.LeftShift))
        {
            controller.Move(move * Time.deltaTime * sprintSpeed);
        }
        else
        {
            controller.Move(move * Time.deltaTime * speed);
        }
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        // Crouch
        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height = crouchHeight;
        }
        else
        {
            controller.height = originalHeight;
        }

        // Ladder climbing
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f, ladderMask))
        {
            isClimbing = true;
        }
        else
        {
            isClimbing = false;
        }

        if (isClimbing)
        {
            moveVertical = Input.GetAxis("Vertical");
            Vector3 mover = new Vector3(0, moveVertical, 0);
            controller.Move(mover * Time.deltaTime * speed);
            playerVelocity.y = 0; // Prevent the player from falling due to gravity

            // Check if the player is pressing the shift key
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Move the player down
                controller.Move(-Vector3.up * Time.deltaTime * speed);
            }

            // Check if the jump button is pressed
            if (Input.GetButtonDown("Jump"))
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }
            else
            {
                playerVelocity.y = 0; // Prevent the player from falling due to gravity
            }

            // Check if the player is at the top of the ladder
            if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.5f, platformMask))
            {
                // Add a small upward force to help the player climb off the ladder onto the platform
                controller.Move(Vector3.up * Time.deltaTime * speed);
            }
        }

            if (health <= 0 && !isDead)
            {
                // Player is dead
                Debug.Log("Player is dead");
                deathText.text = "You have died. Press R to respawn."; // Display the death message
                isDead = true;
            }

            // Check if the player is dead and the 'R' key is pressed
            if (isDead && Input.GetKeyDown(KeyCode.R))
            {
                // Respawn the player
                Respawn();
            }


            // Check for the 'switch item' key (let's say it's 'Q')
            if(Input.GetKeyDown(KeyCode.Q))
            {
                SwitchItem();
            }

        }

            
        

    public void TakeDamage(float damage)
    {
        // Decrease the player's health by the damage amount
        health -= damage;

        // Check if the player's health is less than or equal to 0 and the player is not already dead
        if (health <= 0 && !isDead)
        {
            // Player is dead
            Debug.Log("Player is dead");
            deathText.text = "You have died. Press R to respawn."; // Display the death message
            isDead = true;
        }
    }

    
    void Respawn()
    {
        // Reset the player's health
        health = 100f;

        // Teleport the player to the respawn position
        Vector3 moveVector = respawnPosition - transform.position;
        controller.Move(moveVector);

        // Clear the death message
        deathText.text = "";

        // Set the player as not dead
        isDead = false;
    }

    void SwitchItem()
    {
        currentItem = (currentItem + 1) % items.Length; // Move to the next item, loop back to the start if at the end
        // Update the player's held item here...
    }



}

    