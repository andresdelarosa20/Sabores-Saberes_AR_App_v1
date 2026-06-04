using UnityEngine;

public class PulseIndicator : MonoBehaviour
{
    public enum AnimationType
    {
        Pulse,
        MoveLeftRight
    }

    [Header("Animation")]
    public AnimationType animationType;

    [Header("Pulse Settings")]
    public float scaleAmount = 0.1f;
    public float pulseSpeed = 2f;

    [Header("Movement Settings")]
    public float moveDistance = 20f;
    public float moveSpeed = 1f;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        switch (animationType)
        {
            case AnimationType.Pulse:
                PulseAnimation();
                break;

            case AnimationType.MoveLeftRight:
                MoveAnimation();
                break;
        }
    }

    private void PulseAnimation()
    {
        float scale =
            1 + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;

        transform.localScale = originalScale * scale;
    }

    private void MoveAnimation()
    {
        float offsetX =
            Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        transform.localPosition =
            originalPosition + new Vector3(offsetX, 0f, 0f);
    }
}