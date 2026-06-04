using UnityEngine;

public class PulseIndicator : MonoBehaviour
{
    public float scaleAmount = 0.1f;
    public float speed = 2f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float scale =
            1 + Mathf.Sin(Time.time * speed) * scaleAmount;

        transform.localScale =
            originalScale * scale;
    }
}