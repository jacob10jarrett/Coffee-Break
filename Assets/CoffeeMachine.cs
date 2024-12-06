using UnityEngine;
using UnityEngine.SceneManagement;

public class CoffeeMachine : MonoBehaviour
{
    public GameObject interactionPrompt; // UI element for "Press E to make coffee"
    public Transform animatedPrompt;     // Reference to the "E" prompt above the machine

    private FlashingColor coffeeFlasher;
    private FlashingColor eFlasher;
    private bool isPlayerInRange = false;

    void Start()
    {
        // Initialize flashing components
        coffeeFlasher = GetComponent<FlashingColor>();
        if (animatedPrompt != null)
        {
            eFlasher = animatedPrompt.GetComponent<FlashingColor>();
        }

        // Start flashing effects
        coffeeFlasher?.StartFlashing();
        eFlasher?.StartFlashing();

        // Ensure the interaction prompt is hidden initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Ensure the animated "E" is visible at the start
        if (animatedPrompt != null)
        {
            animatedPrompt.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Allow interaction only when the player is in range
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            MakeCoffee();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Show the interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            // Hide the animated "E" since the player is now colliding with the machine
            if (animatedPrompt != null)
            {
                animatedPrompt.gameObject.SetActive(false);
            }

            // Stop flashing effects
            coffeeFlasher?.StopFlashing();
            eFlasher?.StopFlashing();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Hide the interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            // Show the animated "E" since the player is no longer colliding
            if (animatedPrompt != null)
            {
                animatedPrompt.gameObject.SetActive(true);
            }

            // Resume flashing effects
            coffeeFlasher?.StartFlashing();
            eFlasher?.StartFlashing();
        }
    }

    private void MakeCoffee()
    {
        SceneManager.LoadScene("MakingOrder");
    }
}
