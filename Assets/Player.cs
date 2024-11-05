using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    private Vector2 movement;
    private Vector2 lastDirection; // To store the last direction player was facing

    void Update()
    {
        // Get input for movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement to maintain consistent speed
        if (movement.magnitude > 1)
        {
            movement = movement.normalized;
        }

        // Set animation parameters based on movement input
        UpdateAnimationParameters();
	   
	   // Check if the player presses the "E" key to switch scenes
	   if (Input.GetKeyDown(KeyCode.E))
        {
            LoadMakingOrderScene();
        }
    }

    void FixedUpdate()
    {
        // Move the player
        transform.Translate(movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the Player GameObject.");
            return;
        }

        // Calculate movement speed based on input magnitude
        float speed = movement.magnitude;

        // Update animator's Speed parameter to control idle vs. walking
        animator.SetFloat("Speed", speed);

        // If moving, update the direction parameters and last direction
        if (speed > 0)
        {
            // Update lastDirection based on current movement
            lastDirection = movement;

            // Set Vertical or Horizontal based on the direction the player is moving
            if (Mathf.Abs(movement.y) > Mathf.Abs(movement.x))
            {
                // Prioritize up/down movement
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", movement.y);
            }
            else
            {
                // Prioritize sideways movement
                animator.SetFloat("Horizontal", movement.x);
                animator.SetFloat("Vertical", 0);
            }
        }
        else
        {
            // When idle, keep the last direction values but set Speed to 0
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);
        }

        // Flip the player's sprite based on horizontal movement direction
        if (lastDirection.x > 0)
        {
            transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
        }
        else if (lastDirection.x < 0)
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }
    
    private void LoadMakingOrderScene()
    {
        SceneManager.LoadScene("MakingOrder");
    }
}
