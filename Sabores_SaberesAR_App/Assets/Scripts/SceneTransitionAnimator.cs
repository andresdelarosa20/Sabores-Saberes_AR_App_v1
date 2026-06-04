using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SceneTransitionAnimator — Script reutilizable para animaciones de entrada/salida de escenas.
/// Compatible con: Image, Button, Text, RectTransform, CanvasGroup, cualquier UI element.
///
/// USO BÁSICO:
///   1. Agrega este componente a cualquier GameObject con un RectTransform.
///   2. Configura el tipo de animación, duración y dirección desde el Inspector.
///   3. Llama a PlayEnter() al entrar a la escena y PlayExit() al salir.
///   4. Opcionalmente, suscríbete a los eventos OnEnterComplete / OnExitComplete.
/// </summary>
public class SceneTransitionAnimator : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  ENUMS
    // ─────────────────────────────────────────────

    public enum AnimationType
    {
        Fade,               // Aparece/desaparece con opacidad
        SlideFromLeft,      // Entra desde la izquierda
        SlideFromRight,     // Entra desde la derecha
        SlideFromTop,       // Entra desde arriba
        SlideFromBottom,    // Entra desde abajo
        ScaleUp,            // Crece desde 0
        ScaleDown,          // Encoge hasta 0
        RotateFade,         // Gira mientras aparece/desaparece
        BounceIn,           // Rebota al entrar
        FlipHorizontal,     // Voltea en eje X
        FlipVertical,       // Voltea en eje Y
        Spiral,             // Gira y escala al mismo tiempo
    }

    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic,
        BackOvershoot,
    }

    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("═══ Configuración de Animación ═══")]
    [Tooltip("Tipo de animación al entrar/salir de escena")]
    public AnimationType animationType = AnimationType.Fade;

    [Tooltip("Función de suavizado (easing)")]
    public EasingType easingType = EasingType.EaseInOut;

    [Tooltip("Duración de la animación de entrada (segundos)")]
    [Range(0.1f, 3f)]
    public float enterDuration = 0.5f;

    [Tooltip("Duración de la animación de salida (segundos)")]
    [Range(0.1f, 3f)]
    public float exitDuration = 0.4f;

    [Tooltip("Retardo antes de iniciar la animación de entrada")]
    [Range(0f, 2f)]
    public float enterDelay = 0f;

    [Tooltip("Retardo antes de iniciar la animación de salida")]
    [Range(0f, 2f)]
    public float exitDelay = 0f;

    [Header("═══ Opciones de Slide ═══")]
    [Tooltip("Distancia de desplazamiento para animaciones Slide (px)")]
    public float slideDistance = 300f;

    [Header("═══ Opciones de Rotación ═══")]
    [Tooltip("Grados de rotación para RotateFade / Spiral")]
    public float rotationAmount = 180f;

    [Header("═══ Opciones de Escala ═══")]
    [Tooltip("Escala inicial para ScaleUp (0 = aparece desde nada)")]
    [Range(0f, 1f)]
    public float scaleStartValue = 0f;

    [Header("═══ Comportamiento ═══")]
    [Tooltip("¿Reproducir animación de entrada automáticamente al activarse?")]
    public bool playEnterOnEnable = true;

    [Tooltip("¿Desactivar el GameObject al completar la animación de salida?")]
    public bool deactivateOnExitComplete = true;

    [Tooltip("¿Ignorar la escala de tiempo (útil para menús de pausa)?")]
    public bool useUnscaledTime = false;

    // ─────────────────────────────────────────────
    //  EVENTOS
    // ─────────────────────────────────────────────

    /// <summary>Se dispara cuando la animación de ENTRADA termina.</summary>
    public event Action OnEnterComplete;

    /// <summary>Se dispara cuando la animación de SALIDA termina.</summary>
    public event Action OnExitComplete;

    // ─────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Vector2 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private Coroutine _currentCoroutine;
    private bool _initialized = false;

    // ─────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (playEnterOnEnable)
            PlayEnter();
    }

    // ─────────────────────────────────────────────
    //  INICIALIZACIÓN
    // ─────────────────────────────────────────────

    private void Initialize()
    {
        if (_initialized) return;

        _rect = GetComponent<RectTransform>();
        if (_rect == null)
        {
            Debug.LogWarning($"[SceneTransitionAnimator] '{name}' no tiene RectTransform. El script requiere un elemento UI.");
            return;
        }

        // Obtener o crear CanvasGroup para el fade
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Guardar estado original
        _originalPosition = _rect.anchoredPosition;
        _originalScale    = _rect.localScale;
        _originalRotation = _rect.localRotation;

        _initialized = true;
    }

    // ─────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────

    /// <summary>Reproduce la animación de ENTRADA (aparece en escena).</summary>
    public void PlayEnter(Action onComplete = null)
    {
        Initialize();
        StopCurrentAnimation();
        gameObject.SetActive(true);
        _currentCoroutine = StartCoroutine(AnimateEnter(onComplete));
    }

    /// <summary>Reproduce la animación de SALIDA (desaparece de escena).</summary>
    public void PlayExit(Action onComplete = null)
    {
        Initialize();
        StopCurrentAnimation();
        _currentCoroutine = StartCoroutine(AnimateExit(onComplete));
    }

    /// <summary>Salta directamente al estado "visible" sin animación.</summary>
    public void SetVisibleInstant()
    {
        Initialize();
        ApplyEnterState(1f);
    }

    /// <summary>Salta directamente al estado "invisible" sin animación.</summary>
    public void SetHiddenInstant()
    {
        Initialize();
        ApplyExitState(1f);
    }

    /// <summary>Detiene cualquier animación en curso.</summary>
    public void StopCurrentAnimation()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
    }

    // ─────────────────────────────────────────────
    //  COROUTINES PRINCIPALES
    // ─────────────────────────────────────────────

    private IEnumerator AnimateEnter(Action onComplete)
    {
        // Estado inicial (oculto)
        ApplyExitState(1f);

        // Esperar retardo
        if (enterDelay > 0f)
            yield return WaitForSeconds(enterDelay);

        // Animar de "oculto" → "visible"
        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / enterDuration);
            float easedT = ApplyEasing(t, easingType);
            ApplyEnterState(easedT);
            yield return null;
        }

        // Asegurar estado final exacto
        ApplyEnterState(1f);

        onComplete?.Invoke();
        OnEnterComplete?.Invoke();
        _currentCoroutine = null;
    }

    private IEnumerator AnimateExit(Action onComplete)
    {
        // Estado inicial (visible)
        ApplyEnterState(1f);

        // Esperar retardo
        if (exitDelay > 0f)
            yield return WaitForSeconds(exitDelay);

        // Animar de "visible" → "oculto" (t invertido)
        float elapsed = 0f;
        while (elapsed < exitDuration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / exitDuration);
            float easedT = ApplyEasing(t, easingType);
            // Para la salida, usamos 1 - easedT para invertir la animación
            ApplyEnterState(1f - easedT);
            yield return null;
        }

        // Asegurar estado final exacto
        ApplyExitState(1f);

        onComplete?.Invoke();
        OnExitComplete?.Invoke();

        if (deactivateOnExitComplete)
            gameObject.SetActive(false);

        _currentCoroutine = null;
    }

    // ─────────────────────────────────────────────
    //  APLICAR ESTADOS DE ANIMACIÓN
    //  t = 0 → oculto / t = 1 → visible
    // ─────────────────────────────────────────────

    private void ApplyEnterState(float t)
    {
        if (_rect == null) return;

        switch (animationType)
        {
            case AnimationType.Fade:
                _canvasGroup.alpha = t;
                _rect.anchoredPosition = _originalPosition;
                _rect.localScale = _originalScale;
                _rect.localRotation = _originalRotation;
                break;

            case AnimationType.SlideFromLeft:
                _canvasGroup.alpha = Mathf.Clamp01(t * 2f); // Fade rápido
                _rect.anchoredPosition = Vector2.Lerp(
                    _originalPosition + Vector2.left * slideDistance,
                    _originalPosition, t);
                _rect.localScale = _originalScale;
                break;

            case AnimationType.SlideFromRight:
                _canvasGroup.alpha = Mathf.Clamp01(t * 2f);
                _rect.anchoredPosition = Vector2.Lerp(
                    _originalPosition + Vector2.right * slideDistance,
                    _originalPosition, t);
                _rect.localScale = _originalScale;
                break;

            case AnimationType.SlideFromTop:
                _canvasGroup.alpha = Mathf.Clamp01(t * 2f);
                _rect.anchoredPosition = Vector2.Lerp(
                    _originalPosition + Vector2.up * slideDistance,
                    _originalPosition, t);
                _rect.localScale = _originalScale;
                break;

            case AnimationType.SlideFromBottom:
                _canvasGroup.alpha = Mathf.Clamp01(t * 2f);
                _rect.anchoredPosition = Vector2.Lerp(
                    _originalPosition + Vector2.down * slideDistance,
                    _originalPosition, t);
                _rect.localScale = _originalScale;
                break;

            case AnimationType.ScaleUp:
                _canvasGroup.alpha = t;
                float scaleUp = Mathf.Lerp(scaleStartValue, 1f, t);
                _rect.localScale = _originalScale * scaleUp;
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.ScaleDown:
                _canvasGroup.alpha = t;
                float scaleDown = Mathf.Lerp(2f, 1f, t); // Entra grande y encoge
                _rect.localScale = _originalScale * scaleDown;
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.RotateFade:
                _canvasGroup.alpha = t;
                float angle = Mathf.Lerp(rotationAmount, 0f, t);
                _rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                _rect.localScale = _originalScale;
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.BounceIn:
                _canvasGroup.alpha = Mathf.Clamp01(t * 3f);
                float bounceScale = BounceEase(t);
                _rect.localScale = _originalScale * bounceScale;
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.FlipHorizontal:
                _canvasGroup.alpha = t;
                float scaleX = Mathf.Lerp(-1f, 1f, t);
                _rect.localScale = new Vector3(
                    _originalScale.x * scaleX,
                    _originalScale.y,
                    _originalScale.z);
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.FlipVertical:
                _canvasGroup.alpha = t;
                float scaleY = Mathf.Lerp(-1f, 1f, t);
                _rect.localScale = new Vector3(
                    _originalScale.x,
                    _originalScale.y * scaleY,
                    _originalScale.z);
                _rect.anchoredPosition = _originalPosition;
                break;

            case AnimationType.Spiral:
                _canvasGroup.alpha = t;
                float spiralAngle = Mathf.Lerp(rotationAmount, 0f, t);
                float spiralScale = Mathf.Lerp(scaleStartValue, 1f, t);
                _rect.localRotation = Quaternion.Euler(0f, 0f, spiralAngle);
                _rect.localScale = _originalScale * spiralScale;
                _rect.anchoredPosition = _originalPosition;
                break;
        }
    }

    private void ApplyExitState(float t)
    {
        // El estado de "salida" es simplemente Enter con t=0
        ApplyEnterState(0f);
    }

    // ─────────────────────────────────────────────
    //  EASING FUNCTIONS
    // ─────────────────────────────────────────────

    private float ApplyEasing(float t, EasingType easing)
    {
        switch (easing)
        {
            case EasingType.Linear:
                return t;

            case EasingType.EaseIn:
                return t * t * t;

            case EasingType.EaseOut:
                return 1f - Mathf.Pow(1f - t, 3f);

            case EasingType.EaseInOut:
                return t < 0.5f
                    ? 4f * t * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            case EasingType.Bounce:
                return BounceOut(t);

            case EasingType.Elastic:
                return ElasticOut(t);

            case EasingType.BackOvershoot:
                return BackOut(t);

            default:
                return t;
        }
    }

    // Bounce easing (rebote al final)
    private float BounceOut(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    // Bounce para animación de ScaleUp
    private float BounceEase(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        float p = 0.3f;
        float s = p / 4f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }

    // Elastic easing
    private float ElasticOut(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        float p = 0.3f;
        float s = p / 4f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }

    // Back (overshoot) easing
    private float BackOut(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // ─────────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────────

    private float GetDeltaTime() =>
        useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private IEnumerator WaitForSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }
    }
}
