using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAnimation : MonoBehaviour
{
    [Header("Overlay para el parpadeo (Image negro encima de todo)")]
    public Image overlayImage;

    [Header("Pantalla que se mueve")]
    public RectTransform screenRect;

    public Vector2 screenStartPosition;
    public Vector2 screenEndPosition;
    public float screenMoveDuration = 0.4f;

    [Range(0f, 0.3f)]
    public float fadeDuration = 0.08f;

    [Header("Tarjeta que sale volando")]
    public RectTransform cardOut;
    public Vector2 cardOutTarget;
    public float arcHeight = 80f;
    public float cardOutDuration = 0.35f;

    [Header("Tarjeta nueva que entra")]
    public RectTransform cardIn;
    public bool animateCardIn = true;
    public Vector2 cardInStartOffset = new Vector2(0, -60f);
    public float cardInDuration = 0.25f;

    private Vector2 cardInFinalPosition;

    private void Start()
    {
        if (cardIn != null)
            cardInFinalPosition = cardIn.anchoredPosition;
    }

    public void PlayAnimation()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        // 1. Fade IN (oscurecer)
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 2. Reset pantalla mientras está oscuro
        screenRect.anchoredPosition = screenStartPosition;

        // 3. Mover pantalla
        yield return StartCoroutine(MoveScreen());

        // 4. Fade OUT (volver a transparente)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // 5. CardOut DESPUÉS del fade
        yield return StartCoroutine(AnimateCardOut());
    }

    // FADE
    private IEnumerator Fade(float alphaFrom, float alphaTo, float dur)
    {
        float time = 0f;
        Color c = overlayImage.color;

        while (time < dur)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / dur);

            c.a = Mathf.Lerp(alphaFrom, alphaTo, t);
            overlayImage.color = c;

            yield return null;
        }

        c.a = alphaTo;
        overlayImage.color = c;
    }

    // MOVIMIENTO DE PANTALLA
    private IEnumerator MoveScreen()
    {
        float time = 0f;

        while (time < screenMoveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / screenMoveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            screenRect.anchoredPosition =
                Vector2.Lerp(screenStartPosition, screenEndPosition, t);

            yield return null;
        }

        screenRect.anchoredPosition = screenEndPosition;
    }

    // CARD OUT
    private IEnumerator AnimateCardOut()
    {
        Vector2 startPos = cardOut.anchoredPosition;
        float time = 0f;

        while (time < cardOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / cardOutDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float x = Mathf.Lerp(startPos.x, cardOutTarget.x, t);
            float y = Mathf.Lerp(startPos.y, cardOutTarget.y, t);

            // arco
            y += arcHeight * 4f * t * (1f - t);

            cardOut.anchoredPosition = new Vector2(x, y);

            yield return null;
        }

        cardOut.anchoredPosition = cardOutTarget;
        cardOut.gameObject.SetActive(false);
    }

    // CARD IN (no se usa aún en el flujo)
    private IEnumerator AnimateCardIn()
    {
        Vector2 startPos = cardInFinalPosition + cardInStartOffset;
        cardIn.anchoredPosition = startPos;

        float time = 0f;

        while (time < cardInDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / cardInDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            cardIn.anchoredPosition =
                Vector2.Lerp(startPos, cardInFinalPosition, t);

            yield return null;
        }

        cardIn.anchoredPosition = cardInFinalPosition;
    }
}