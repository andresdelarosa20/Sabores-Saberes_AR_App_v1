using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelOverlayController : MonoBehaviour
{
    [Header("═══ Referencias ═══")]
    public RectTransform panelTarget;
    public Canvas rootCanvas;
    public CanvasGroup backdropPanel;

    [Header("═══ Animación ═══")]
    public AnimationType animationType = AnimationType.ScaleAndFade;
    [Range(0.1f, 1f)] public float showDuration = 0.35f;
    [Range(0.1f, 1f)] public float hideDuration = 0.25f;
    public EasingType showEasing = EasingType.BackOvershoot;
    public EasingType hideEasing = EasingType.EaseIn;

    [Header("═══ Opciones ═══")]
    public Vector2 targetPosition = Vector2.zero;
    [Range(0f, 1f)] public float backdropAlpha = 0.6f;

    public enum AnimationType { Fade, ScaleAndFade, SlideFromBottom, SlideFromTop }
    public enum EasingType { Linear, EaseIn, EaseOut, EaseInOut, BackOvershoot, Elastic }

    // ── Estado interno ──────────────────────────
    private CanvasGroup _panelCG;
    private Vector2 _originalPosition;
    private Transform _originalParent;
    private int _originalSiblingIndex;
    private bool _isVisible = false;
    private bool _isAnimating = false;
    private Coroutine _currentCoroutine;

    // Gestor global del backdrop (estático, compartido entre todas las salas)
    private static PanelOverlayController _activeController = null;

    // ─────────────────────────────────────────────
    private void Awake()
    {
        if (panelTarget == null) return;

        _panelCG = panelTarget.GetComponent<CanvasGroup>();
        if (_panelCG == null)
            _panelCG = panelTarget.gameObject.AddComponent<CanvasGroup>();

        _originalPosition = panelTarget.anchoredPosition;
        _originalParent = panelTarget.parent;
        _originalSiblingIndex = panelTarget.GetSiblingIndex();

        ApplyState(0f);
        panelTarget.gameObject.SetActive(false);

        // El backdrop se configura UNA sola vez, sin agregar listeners aquí
        if (backdropPanel != null)
        {
            backdropPanel.alpha = 0f;
            backdropPanel.blocksRaycasts = false;
            backdropPanel.interactable = false;

            // Agregar botón al backdrop solo si no existe todavía
            Button btn = backdropPanel.GetComponent<Button>();
            if (btn == null)
                btn = backdropPanel.gameObject.AddComponent<Button>();

            btn.transition = Selectable.Transition.None;

            // Un solo listener estático que cierra quien esté activo
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(CloseActive);
        }
    }

    // Cierra el controlador que esté activo en ese momento
    private static void CloseActive()
    {
        if (_activeController != null && _activeController._isVisible)
            _activeController.Hide();
    }

    // ─────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────

    public void Toggle()
    {
        if (_isAnimating) return;
        if (_isVisible) Hide();
        else Show();
    }

    public void Show()
    {
        if (_isAnimating) return;

        // Si hay otro panel abierto, cerrarlo primero
        if (_activeController != null && _activeController != this && _activeController._isVisible)
            _activeController.HideInstant();

        StopCurrent();

        if (rootCanvas != null)
            panelTarget.SetParent(rootCanvas.transform, false);

        panelTarget.anchoredPosition = targetPosition;
        panelTarget.localScale = Vector3.one;
        panelTarget.gameObject.SetActive(true);

        _activeController = this; // Registrarse como activo
        _currentCoroutine = StartCoroutine(AnimateShow());
    }

    public void Hide()
    {
        if (!_isVisible || _isAnimating) return;
        StopCurrent();
        _currentCoroutine = StartCoroutine(AnimateHide());
    }

    // Cierre inmediato sin animación (para cuando se abre otra sala)
    private void HideInstant()
    {
        StopCurrent();
        ApplyState(0f);
        panelTarget.SetParent(_originalParent, false);
        panelTarget.SetSiblingIndex(_originalSiblingIndex);
        panelTarget.anchoredPosition = _originalPosition;
        panelTarget.gameObject.SetActive(false);
        _isVisible = false;
        _isAnimating = false;
        if (_activeController == this)
            _activeController = null;
    }

    // ─────────────────────────────────────────────
    //  COROUTINES
    // ─────────────────────────────────────────────

    private IEnumerator AnimateShow()
    {
        _isAnimating = true;

        if (backdropPanel != null)
        {
            backdropPanel.blocksRaycasts = true;
            backdropPanel.interactable = false;
        }

        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float t = Ease(Mathf.Clamp01(elapsed / showDuration), showEasing);
            ApplyState(t);
            if (backdropPanel != null)
                backdropPanel.alpha = Mathf.Lerp(0f, backdropAlpha, elapsed / showDuration);
            yield return null;
        }

        ApplyState(1f);
        if (backdropPanel != null)
        {
            backdropPanel.alpha = backdropAlpha;
            backdropPanel.interactable = true;
        }

        _isVisible = true;
        _isAnimating = false;
    }

    private IEnumerator AnimateHide()
    {
        _isAnimating = true;

        if (backdropPanel != null)
            backdropPanel.interactable = false;

        float elapsed = 0f;
        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Ease(Mathf.Clamp01(elapsed / hideDuration), hideEasing);
            ApplyState(1f - t);
            if (backdropPanel != null)
                backdropPanel.alpha = Mathf.Lerp(backdropAlpha, 0f, elapsed / hideDuration);
            yield return null;
        }

        ApplyState(0f);

        if (backdropPanel != null)
        {
            backdropPanel.alpha = 0f;
            backdropPanel.blocksRaycasts = false;
            backdropPanel.interactable = false;
        }

        panelTarget.SetParent(_originalParent, false);
        panelTarget.SetSiblingIndex(_originalSiblingIndex);
        panelTarget.anchoredPosition = _originalPosition;
        panelTarget.gameObject.SetActive(false);

        _isVisible = false;
        _isAnimating = false;

        if (_activeController == this)
            _activeController = null;
    }

    // ─────────────────────────────────────────────
    //  ESTADO VISUAL
    // ─────────────────────────────────────────────

    private void ApplyState(float t)
    {
        if (_panelCG == null) return;

        _panelCG.alpha = t;
        _panelCG.blocksRaycasts = t >= 1f;
        _panelCG.interactable = t >= 1f;

        switch (animationType)
        {
            case AnimationType.Fade:
                panelTarget.localScale = Vector3.one;
                panelTarget.anchoredPosition = targetPosition;
                break;
            case AnimationType.ScaleAndFade:
                panelTarget.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t);
                panelTarget.anchoredPosition = targetPosition;
                break;
            case AnimationType.SlideFromBottom:
                panelTarget.localScale = Vector3.one;
                panelTarget.anchoredPosition = targetPosition + Vector2.up * Mathf.Lerp(-300f, 0f, t);
                break;
            case AnimationType.SlideFromTop:
                panelTarget.localScale = Vector3.one;
                panelTarget.anchoredPosition = targetPosition + Vector2.up * Mathf.Lerp(300f, 0f, t);
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  EASING
    // ─────────────────────────────────────────────

    private float Ease(float t, EasingType easing)
    {
        switch (easing)
        {
            case EasingType.Linear: return t;
            case EasingType.EaseIn: return t * t * t;
            case EasingType.EaseOut: return 1f - Mathf.Pow(1f - t, 3f);
            case EasingType.EaseInOut: return t < 0.5f ? 4 * t * t * t : 1f - Mathf.Pow(-2 * t + 2, 3) / 2f;
            case EasingType.BackOvershoot:
                float c1 = 1.70158f, c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            case EasingType.Elastic:
                if (t == 0f || t == 1f) return t;
                return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.075f) * (2f * Mathf.PI) / 0.3f) + 1f;
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
