using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAnimation : MonoBehaviour
{
    [Header("Fade")]
    public Image fadeImage;

    [Header("Card Movement")]
    public RectTransform card;

    public Vector2 startPosition;
    public Vector2 endPosition;

    [Tooltip("Altura máxima de la curva")]
    public float arcHeight = 150f;

    public float duration = 1f;

    public void PlayAnimation()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float time = 0;

        Color startColor = fadeImage.color;

        card.anchoredPosition = startPosition;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // Fade
            Color c = startColor;
            c.a = 1 - t;
            fadeImage.color = c;

            // Movimiento curvo
            float x = Mathf.Lerp(
                startPosition.x,
                endPosition.x,
                t
            );

            float y = Mathf.Lerp(
                startPosition.y,
                endPosition.y,
                t
            );

            // Curva parabólica
            y += arcHeight * 4 * t * (1 - t);

            card.anchoredPosition =
                new Vector2(x, y);

            yield return null;
        }

        card.anchoredPosition = endPosition;
    }
}
