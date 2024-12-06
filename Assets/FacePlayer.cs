using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Reference to Finn (the player)
    public Animator animator; // Animator for Gita's animations

    [Header("Configuration")]
    public float directionThreshold = 0.1f; // Threshold for significant movement
    public float horizontalDominanceThreshold = 1f; // Ratio for dominant horizontal movement

    private Vector2 lastDirection; // To store Gita's last meaningful direction
    private bool isFacingRight; // Tracks whether the sprite is facing right

    void Start()
    {
        // Initialize isFacingRight based on initial localScale.x
        isFacingRight = transform.localScale.x < 0; // Assuming negative x is facing right
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // Calculate direction to the player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Update last meaningful direction if movement is significant
        if (directionToPlayer.magnitude > directionThreshold)
        {
            lastDirection = directionToPlayer;
        }

        // Update animations and handle flipping
        UpdateAnimationAndFlipping(lastDirection);
    }

    private void UpdateAnimationAndFlipping(Vector2 direction)
    {
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on Gita.");
            return;
        }

        // Determine dominant movement direction
        bool isHorizontalDominant = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) * horizontalDominanceThreshold;


        if (isHorizontalDominant)
        {
            // Left/right direction
            float horizontalValue = direction.x > 0 ? 1f : -1f;
            animator.SetFloat("Horizontal", horizontalValue);
            animator.SetFloat("Vertical", 0f);

            // Flip sprite only if facing changes
            if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            {
                FlipSprite(direction.x > 0);
            }
        }
        else
        {
            // Up/down direction
            float verticalValue = direction.y > 0 ? 1f : -1f;
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", verticalValue);

            // Do not flip sprite for vertical movement
        }

        // Gita is stationary, so Speed is always 0
        animator.SetFloat("Speed", 0f);
    }

    private void FlipSprite(bool facingRight)
    {
        // Update the facing direction
        isFacingRight = facingRight;

        // Flip the sprite by setting the X scale
        Vector3 localScale = transform.localScale;
        localScale.x = facingRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        Debug.Log($"Flipping sprite. Facing Right: {facingRight}, New localScale.x: {transform.localScale.x}");
    }
}
