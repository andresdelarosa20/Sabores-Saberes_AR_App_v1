using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimation : MonoBehaviour
{
    [Header("Animation Type")]
    public string animationType = "FadeIn";

    [Header("Settings")]
    public float duration = 0.5f;
    public float distance = 500f;

    private RectTransform rectTransform;
    private Image image;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        switch (animationType)
        {
            case "FadeIn":
                yield return StartCoroutine(Fade(0, 1));
                break;

            case "FadeOut":
                yield return StartCoroutine(Fade(1, 0));
                break;

            case "SlideFromLeft":
                yield return StartCoroutine(Slide(
                    originalPosition + Vector2.left * distance,
                    originalPosition));
                break;

            case "SlideFromRight":
                yield return StartCoroutine(Slide(
                    originalPosition + Vector2.right * distance,
                    originalPosition));
                break;

            case "SlideFromTop":
                yield return StartCoroutine(Slide(
                    originalPosition + Vector2.up * distance,
                    originalPosition));
                break;

            case "SlideFromBottom":
                yield return StartCoroutine(Slide(
                    originalPosition + Vector2.down * distance,
                    originalPosition));
                break;

            case "ScaleIn":
                yield return StartCoroutine(Scale(
                    Vector3.zero,
                    originalScale));
                break;

            case "ScaleOut":
                yield return StartCoroutine(Scale(
                    originalScale,
                    Vector3.zero));
                break;

            case "Pop":
                yield return StartCoroutine(PopEffect());
                break;
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        if (image == null)
            yield break;

        Color color = image.color;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(start, end, time / duration);
            image.color = color;

            yield return null;
        }

        color.a = end;
        image.color = color;
    }

    private IEnumerator Slide(Vector2 startPos, Vector2 endPos)
    {
        rectTransform.anchoredPosition = startPos;

        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, time / duration);

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }

    private IEnumerator Scale(Vector3 startScale, Vector3 endScale)
    {
        transform.localScale = startScale;

        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            transform.localScale =
                Vector3.Lerp(startScale, endScale, time / duration);

            yield return null;
        }

        transform.localScale = endScale;
    }

    private IEnumerator PopEffect()
    {
        transform.localScale = Vector3.zero;

        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            float scale = Mathf.Lerp(0f, 1.2f, time / duration);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        transform.localScale = originalScale;
    }
}