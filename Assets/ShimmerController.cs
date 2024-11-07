using System.Collections;
using UnityEngine;

public class ShimmerController : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        // Start the shimmer cycle
        StartCoroutine(ShimmerLoop());
    }

    private IEnumerator ShimmerLoop()
    {
        while (true)
        {
            // Play the shimmer animation
            animator.SetBool("isShimmering", true);

            // Wait until the animation ends
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            // Stop the shimmer animation and wait for 3 seconds
            animator.SetBool("isShimmering", false);
            yield return new WaitForSeconds(3f);
        }
    }
}
