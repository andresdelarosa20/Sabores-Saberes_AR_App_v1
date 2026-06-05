using System.Collections;
using UnityEngine;

public class CardLoopAnimation : MonoBehaviour
{
    [Header("Tarjetas en loop")]
    public RectTransform[] cards;

    [Header("Desplazamiento")]
    public float moveX = 60f;        // cuánto se mueve a la derecha
    public float moveY = -40f;       // cuánto baja
    public float rotationAmount = 8f; // grados de rotación al final

    [Header("Timing")]
    public float cycleDuration = 1.8f;  // duración de un ciclo completo
    public float staggerDelay = 0.4f;   // desfase entre tarjetas

    private Vector2[] _originalPositions;
    private Quaternion[] _originalRotations;

    private void Awake()
    {
        _originalPositions = new Vector2[cards.Length];
        _originalRotations = new Quaternion[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            _originalPositions[i] = cards[i].anchoredPosition;
            _originalRotations[i] = cards[i].localRotation;
        }
    }

    private void OnEnable() { StartLoops(); }
    private void OnDisable()
    {
        StopAllCoroutines();
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].anchoredPosition = _originalPositions[i];
            cards[i].localRotation = _originalRotations[i];
        }
    }

    public void StartLoops()
    {
        StopAllCoroutines();
        for (int i = 0; i < cards.Length; i++)
            StartCoroutine(LoopCard(i));
    }

    private IEnumerator LoopCard(int index)
    {
        // Desfase inicial para que no estén sincronizadas
        float offset = staggerDelay * index;
        float time = offset;

        while (true)
        {
            time += Time.deltaTime;

            // t va de 0 a 1 de forma continua usando sin
            // sin va de -1 a 1, lo normalizamos a 0-1
            float rawT = (Mathf.Sin((time / cycleDuration) * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f;

            // Aplicar movimiento suave sin snap
            float x = _originalPositions[index].x + moveX * rawT;
            float y = _originalPositions[index].y + moveY * rawT;
            float rot = rotationAmount * rawT;

            cards[index].anchoredPosition = new Vector2(x, y);
            cards[index].localRotation = Quaternion.Euler(0f, 0f, -rot);

            yield return null;
        }
    }
}