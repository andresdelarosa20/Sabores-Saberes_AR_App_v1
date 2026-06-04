using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class SlideToConfirmTutorial : MonoBehaviour, IPointerUpHandler
{

    [Header("Bounce Animation")]
    public RectTransform image1;
    public RectTransform image2;

    public Vector2 start1;
    public Vector2 target1;

    public Vector2 start2;
    public Vector2 target2;

    public Vector2 startPosition;
    public Vector2 targetPosition;

    public float moveDuration = 0.6f;
    public float bounceAmount = 20f;
    public float bounceDuration = 0.15f;

    // ==========================
    // REFERENCIAS DEL INSPECTOR
    // ==========================

    [Header("Slider")]
    // Slider que el usuario arrastrará.
    public Slider slider;

    [Header("Fade Images")]
    // Imagen que desaparece progresivamente.
    public Image imageA;

    // Imagen que aparece progresivamente.
    public Image imageB;

    [Header("Animation")]
    // Velocidad con la que el slider se moverá automáticamente
    // hacia el inicio o hacia el final.
    public float snapSpeed = 5f;

    [Header("Settings")]

    // Punto de confirmación.
    // Si el usuario supera este porcentaje, el slider se completará.
    // Si no lo supera, regresará al inicio.
    [Range(0f, 1f)]
    public float confirmThreshold = 0.5f;

    // ==========================
    // VARIABLES INTERNAS
    // ==========================

    // Evita que se inicien varias animaciones al mismo tiempo.
    private bool isAnimating = false;

    // Evita ejecutar la acción más de una vez.
    private bool actionExecuted = false;

    // ==========================
    // START
    // ==========================

    private void Start()
    {
        // Configura el rango del slider.
        slider.minValue = 0;
        slider.maxValue = 1;

        // Inicia completamente a la izquierda.
        slider.value = 0;

        // Actualiza la transparencia de las imágenes.
        UpdateImages();

        // Cada vez que el slider cambie de valor,
        // se actualizarán las imágenes.
        slider.onValueChanged.AddListener(delegate
        {
            UpdateImages();
        });
    }

    // ==========================
    // CONTROL DE OPACIDAD
    // ==========================

    private void UpdateImages()
    {
        float t = slider.value;

        // Opacidad: 1 al inicio, 0 al final
        float alpha = 1 - t;

        if (imageA != null)
        {
            Color c = imageA.color;
            c.a = alpha;
            imageA.color = c;
        }

        if (imageB != null)
        {
            Color c = imageB.color;
            c.a = alpha;
            imageB.color = c;
        }
    }

    // ==========================
    // AL SOLTAR EL DEDO O MOUSE
    // ==========================

    public void OnPointerUp(PointerEventData eventData)
    {
        // Si ya existe una animación ejecutándose,
        // o la acción ya fue completada,
        // no hacemos nada.
        if (isAnimating || actionExecuted)
            return;

        // Si el slider pasó el umbral definido...
        if (slider.value >= confirmThreshold)
        {
            // Completa automáticamente hasta el final.
            StartCoroutine(SmoothMove(1f, true));
        }
        else
        {
            // Regresa automáticamente al inicio.
            StartCoroutine(SmoothMove(0f, false));
        }
    }

    // ==========================
    // ANIMACIÓN AUTOMÁTICA
    // ==========================

    private IEnumerator SmoothMove(float target, bool executeAction)
    {
        // Bloquea nuevas animaciones.
        isAnimating = true;

        // Mientras no haya llegado al destino...
        while (Mathf.Abs(slider.value - target) > 0.001f)
        {
            // Mueve suavemente el slider.
            slider.value = Mathf.MoveTowards(
                slider.value,
                target,
                snapSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Asegura el valor exacto.
        slider.value = target;

        // Libera el bloqueo.
        isAnimating = false;

        // Si se llegó al final y debe ejecutar la acción...
        if (executeAction)
        {
            actionExecuted = true;

            // Llama al método de confirmación.
            OnSliderConfirmed();
        }
    }

    private IEnumerator MoveWithBounce()
    {
        // Posiciones iniciales
        image1.anchoredPosition = start1;
        image2.anchoredPosition = start2;

        float time = 0;

        // Movimiento principal
        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            // Suavizado
            t = Mathf.SmoothStep(0f, 1f, t);
            image1.anchoredPosition =
                Vector2.Lerp(start1, target1, t);
            image2.anchoredPosition =
                Vector2.Lerp(start2, target2, t);

            yield return null;
        }

        // Asegurar posición final exacta
        image1.anchoredPosition = target1;
        image2.anchoredPosition = target2;

        // Bounce hacia arriba
        Vector2 bounceTarget1 =
            target1 + Vector2.up * bounceAmount;
        Vector2 bounceTarget2 =
            target2 + Vector2.up * bounceAmount;

        time = 0;

        while (time < bounceDuration)
        {
            time += Time.deltaTime;

            float t = time / bounceDuration;
            image1.anchoredPosition =
                Vector2.Lerp(target1, bounceTarget1, t);
            image2.anchoredPosition =
                Vector2.Lerp(target2, bounceTarget2, t);

            yield return null;
        }

        // Regreso del bounce
        time = 0;

        while (time < bounceDuration)
        {
            time += Time.deltaTime;

            float t = time / bounceDuration;
            image1.anchoredPosition =
                Vector2.Lerp(bounceTarget1, target1, t);
            image2.anchoredPosition =
                Vector2.Lerp(bounceTarget2, target2, t);

            yield return null;
        }

        // Posición final exacta
        image1.anchoredPosition = target1;
        image2.anchoredPosition = target2;
    }

    // ==========================
    // ACCIÓN FINAL
    // ==========================

    private void OnSliderConfirmed()
    {
        Debug.Log("SLIDER COMPLETADO");

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Tutorial")
        {
            Debug.Log("SLIDER COMPLETADO");

            StartCoroutine(MoveWithBounce());
        }
        else
        {

            SceneTransitionManager.Instance.TransitionToScene("Barcode");
        }
    }
}