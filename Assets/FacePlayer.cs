using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public Transform player; // Reference to Finn (the player)
    public Animator animator; // Animator for Gita's animations
    public float directionThreshold = 0.1f; // Threshold for significant movement
    public float horizontalDominanceThreshold = 1.2f; // Ratio for dominant horizontal movement

    private Vector2 lastDirection; // To store Gita's last meaningful direction
    private bool isFacingRight = true; // Tracks whether the sprite is facing right

    void Update()
    {
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
            animator.SetFloat("Horizontal", direction.x > 0 ? 1 : -1);
            animator.SetFloat("Vertical", 0);

            // Flip sprite only if facing changes
            if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            {
                FlipSprite(direction.x > 0);
            }
        }
        else
        {
            // Up/down direction
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", direction.y > 0 ? 1 : -1);

            // Do not flip sprite for vertical movement
        }

        // Gita is stationary, so Speed is always 0
        animator.SetFloat("Speed", 0);
    }

    private void FlipSprite(bool facingRight)
    {
        // Update facing direction and flip sprite horizontally
        isFacingRight = facingRight;
        transform.localScale = new Vector3(facingRight ? -1.5f : 1.5f, 1.5f, 1.5f);
    }
}
