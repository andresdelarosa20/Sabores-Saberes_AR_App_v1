using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CardReveal — Al hacer click en la card, el panel de texto se desliza
/// hacia un lado revelando la imagen. Click de nuevo para volver.
///
/// SETUP por cada Card:
///
///   [Card]  ← Button + CardReveal (este script)
///   ├── PaperPanel     ← RectTransform del papel/texto (asignar a paperPanel)
///   │   ├── TitleText
///   │   └── DescriptionText
///   └── CardImage      ← Image de fondo (se revela al deslizar el papel)
///
/// </summary>
public class CardReveal : MonoBehaviour
{
    [Header("═══ Referencias ═══")]
    [Tooltip("El RectTransform del panel de papel/texto que se desliza")]
    public RectTransform paperPanel;

    [Header("═══ Animación ═══")]
    [Tooltip("Dirección hacia donde se va el papel al revelar")]
    public SlideDirection slideDirection = SlideDirection.Right;

    [Tooltip("Distancia que se desplaza el papel (px). 0 = calculado automáticamente)")]
    public float slideDistance = 0f;

    [Range(0.1f, 1f)]
    public float revealDuration = 0.45f;

    [Range(0.1f, 1f)]
    public float hideDuration = 0.35f;

    public EasingType revealEasing = EasingType.EaseInOut;
    public EasingType hideEasing   = EasingType.EaseInOut;

    [Header("═══ Fade del papel ═══")]
    [Tooltip("¿El papel también se desvanece mientras se desliza?")]
    public bool fadeWhileSliding = true;

    [Range(0f, 1f)]
    [Tooltip("Opacidad mínima al estar completamente revelado")]
    public float minAlpha = 0f;

    // ─────────────────────────────────────────────
    public enum SlideDirection { Left, Right, Up, Down }
    public enum EasingType     { Linear, EaseIn, EaseOut, EaseInOut, BackOvershoot }

    // ─────────────────────────────────────────────
    private CanvasGroup _paperCG;
    private Vector2     _originalPosition;
    private Vector2     _hiddenPosition;
    private bool        _isRevealed  = false;
    private bool        _isAnimating = false;
    private Coroutine   _currentCoroutine;

    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (paperPanel == null)
        {
            Debug.LogError($"[CardReveal] '{name}': asigna el paperPanel en el Inspector.");
            return;
        }

        // CanvasGroup para el fade
        _paperCG = paperPanel.GetComponent<CanvasGroup>();
        if (_paperCG == null)
            _paperCG = paperPanel.gameObject.AddComponent<CanvasGroup>();

        // Guardar posición original
        _originalPosition = paperPanel.anchoredPosition;

        // Calcular distancia automática si no se especificó
        float distance = slideDistance > 0f
            ? slideDistance
            : GetAutoDistance();

        // Calcular posición oculta según dirección
        _hiddenPosition = _originalPosition + GetSlideVector(distance);

        // Conectar el botón
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(Toggle);
        else
            Debug.LogWarning($"[CardReveal] '{name}': no tiene componente Button. Llama a Toggle() manualmente.");
    }

    // ─────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────

    public void Toggle()
    {
        if (_isAnimating) return;
        if (_isRevealed) Hide();
        else Reveal();
    }

    public void Reveal()
    {
        if (_isAnimating) return;
        StopCurrent();
        _currentCoroutine = StartCoroutine(AnimateReveal());
    }

    public void Hide()
    {
        if (_isAnimating) return;
        StopCurrent();
        _currentCoroutine = StartCoroutine(AnimateHide());
    }

    public void ResetInstant()
    {
        StopCurrent();
        paperPanel.anchoredPosition = _originalPosition;
        if (_paperCG != null) _paperCG.alpha = 1f;
        _isRevealed  = false;
        _isAnimating = false;
    }

    // ─────────────────────────────────────────────
    //  COROUTINES
    // ─────────────────────────────────────────────

    private IEnumerator AnimateReveal()
    {
        _isAnimating = true;

        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Ease(Mathf.Clamp01(elapsed / revealDuration), revealEasing);

            paperPanel.anchoredPosition = Vector2.Lerp(_originalPosition, _hiddenPosition, t);

            if (fadeWhileSliding && _paperCG != null)
                _paperCG.alpha = Mathf.Lerp(1f, minAlpha, t);

            yield return null;
        }

        paperPanel.anchoredPosition = _hiddenPosition;
        if (fadeWhileSliding && _paperCG != null)
            _paperCG.alpha = minAlpha;

        _isRevealed  = true;
        _isAnimating = false;
    }

    private IEnumerator AnimateHide()
    {
        _isAnimating = true;

        float elapsed = 0f;
        Vector2 startPos = paperPanel.anchoredPosition;
        float   startAlpha = _paperCG != null ? _paperCG.alpha : 1f;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Ease(Mathf.Clamp01(elapsed / hideDuration), hideEasing);

            paperPanel.anchoredPosition = Vector2.Lerp(startPos, _originalPosition, t);

            if (fadeWhileSliding && _paperCG != null)
                _paperCG.alpha = Mathf.Lerp(startAlpha, 1f, t);

            yield return null;
        }

        paperPanel.anchoredPosition = _originalPosition;
        if (_paperCG != null) _paperCG.alpha = 1f;

        _isRevealed  = false;
        _isAnimating = false;
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    private float GetAutoDistance()
    {
        // Usa el ancho o alto del panel según la dirección
        switch (slideDirection)
        {
            case SlideDirection.Left:
            case SlideDirection.Right:
                return paperPanel.rect.width > 0 ? paperPanel.rect.width : 400f;
            case SlideDirection.Up:
            case SlideDirection.Down:
                return paperPanel.rect.height > 0 ? paperPanel.rect.height : 300f;
            default:
                return 400f;
        }
    }

    private Vector2 GetSlideVector(float distance)
    {
        switch (slideDirection)
        {
            case SlideDirection.Right: return Vector2.right  * distance;
            case SlideDirection.Left:  return Vector2.left   * distance;
            case SlideDirection.Up:    return Vector2.up     * distance;
            case SlideDirection.Down:  return Vector2.down   * distance;
            default:                   return Vector2.right  * distance;
        }
    }

    private float Ease(float t, EasingType easing)
    {
        switch (easing)
        {
            case EasingType.Linear:    return t;
            case EasingType.EaseIn:    return t * t * t;
            case EasingType.EaseOut:   return 1f - Mathf.Pow(1f - t, 3f);
            case EasingType.EaseInOut: return t < 0.5f ? 4*t*t*t : 1f - Mathf.Pow(-2*t+2, 3)/2f;
            case EasingType.BackOvershoot:
                float c1 = 1.70158f, c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            default: return t;
        }
    }

    private void StopCurrent()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
            _isAnimating = false;
        }
    }
}
