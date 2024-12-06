using UnityEngine;

public class AnimatedE : MonoBehaviour
{
    public float amplitude = 0.5f; 
    public float frequency = 1f;  

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Animate the "E" vertically with a sine wave
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * frequency) * amplitude, 0f);
    }
}
