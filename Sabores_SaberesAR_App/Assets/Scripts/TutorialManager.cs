using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    [Header("═══ Pantalla del Slider (tutorial1) ═══")]
    public GameObject sliderScreen;
    public Slider tutorialSlider;

    [Header("═══ Tarjetas de Tutorial ═══")]
    public GameObject tutorial2;
    public GameObject tutorial3;
    public GameObject tutorial4;

    [Header("═══ Slider ═══")]
    [Range(0f, 1f)] public float snapThreshold = 0.75f; // Si pasa este punto, completa solo
    public float snapSpeed = 8f;                         // Velocidad del snap

    [Header("═══ Animación ═══")]
    public float showDuration = 0.35f;
    public float hideDuration = 0.25f;
    public AnimationType animationType = AnimationType.ScaleAndFade;

    public enum AnimationType { Fade, ScaleAndFade, SlideFromBottom, SlideFromTop }

    private bool _sliderDone = false;
    private bool _isSnapping = false;

    // -------------------------------------------------------
    // INICIALIZACIÓN
    // -------------------------------------------------------
    void Start()
    {
        sliderScreen.SetActive(true);
        tutorial2.SetActive(false);
        tutorial3.SetActive(false);
        tutorial4.SetActive(false);

        tutorialSlider.value = 0f;

        // Detectar cuando el usuario suelta el slider
        EventTrigger trigger = tutorialSlider.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = tutorialSlider.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((_) => OnSliderReleased());
        trigger.triggers.Add(entry);
    }

    // -------------------------------------------------------
    // SLIDER — cuando el usuario suelta
    // -------------------------------------------------------
    void OnSliderReleased()
    {
        if (_sliderDone || _isSnapping) return;

        if (tutorialSlider.value >= snapThreshold)
            StartCoroutine(SnapSlider(1f));  // Completa solo hasta el final
        else
            StartCoroutine(SnapSlider(0f));  // Regresa al inicio
    }

    private IEnumerator SnapSlider(float targetValue)
    {
        _isSnapping = true;

        while (Mathf.Abs(tutorialSlider.value - targetValue) > 0.005f)
        {
            tutorialSlider.value = Mathf.Lerp(tutorialSlider.value, targetValue, Time.deltaTime * snapSpeed * 3f);
            yield return null;
        }

        tutorialSlider.value = targetValue;
        _isSnapping = false;

        if (targetValue >= 1f && !_sliderDone)
        {
            _sliderDone = true;
            StartCoroutine(Transition(sliderScreen, tutorial2));
        }
    }

    // -------------------------------------------------------
    // BOTONES
    // -------------------------------------------------------
    public void MostrarTutorial3()
    {
        StartCoroutine(Transition(tutorial2, tutorial3));
    }

    public void MostrarTutorial4()
    {
        StartCoroutine(Transition(tutorial3, tutorial4));
    }

    public void FinalizarTutorial()
    {
        StartCoroutine(HideOnly(tutorial4));
        Debug.Log("Tutorial finalizado");
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NombreDeTuEscena");
    }

    // -------------------------------------------------------
    // TRANSICIÓN
    // -------------------------------------------------------
    private IEnumerator Transition(GameObject actual, GameObject siguiente)
    {
        yield return StartCoroutine(AnimateOut(actual));
        actual.SetActive(false);

        siguiente.SetActive(true);
        yield return StartCoroutine(AnimateIn(siguiente));
    }

    private IEnumerator HideOnly(GameObject target)
    {
        yield return StartCoroutine(AnimateOut(target));
        target.SetActive(false);
    }

    // -------------------------------------------------------
    // ANIMACIÓN DE ENTRADA
    // -------------------------------------------------------
    private IEnumerator AnimateIn(GameObject target)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(target);
        RectTransform rt = target.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / showDuration));
            cg.alpha = t;
            ApplyShowAnim(rt, t, originalPos);
            yield return null;
        }

        cg.alpha = 1f;
        rt.anchoredPosition = originalPos;
        rt.localScale = Vector3.one;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    // -------------------------------------------------------
    // ANIMACIÓN DE SALIDA
    // -------------------------------------------------------
    private IEnumerator AnimateOut(GameObject target)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(target);
        RectTransform rt = target.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;

        cg.blocksRaycasts = false;
        cg.interactable = false;

        float elapsed = 0f;
        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseIn(Mathf.Clamp01(elapsed / hideDuration));
            cg.alpha = 1f - t;
            ApplyHideAnim(rt, t, originalPos);
            yield return null;
        }

        cg.alpha = 0f;
        rt.anchoredPosition = originalPos;
        rt.localScale = Vector3.one;
    }

    // -------------------------------------------------------
    // TIPOS DE ANIMACIÓN
    // -------------------------------------------------------
    private void ApplyShowAnim(RectTransform rt, float t, Vector2 originalPos)
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos;
                break;
            case AnimationType.ScaleAndFade:
                rt.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t);
                rt.anchoredPosition = originalPos;
                break;
            case AnimationType.SlideFromBottom:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos + Vector2.up * Mathf.Lerp(-300f, 0f, t);
                break;
            case AnimationType.SlideFromTop:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos + Vector2.up * Mathf.Lerp(300f, 0f, t);
                break;
        }
    }

    private void ApplyHideAnim(RectTransform rt, float t, Vector2 originalPos)
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos;
                break;
            case AnimationType.ScaleAndFade:
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
                rt.anchoredPosition = originalPos;
                break;
            case AnimationType.SlideFromBottom:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos + Vector2.up * Mathf.Lerp(0f, -300f, t);
                break;
            case AnimationType.SlideFromTop:
                rt.localScale = Vector3.one;
                rt.anchoredPosition = originalPos + Vector2.up * Mathf.Lerp(0f, 300f, t);
                break;
        }
    }

    // -------------------------------------------------------
    // UTILIDADES
    // -------------------------------------------------------
    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseIn(float t) => t * t * t;
}
