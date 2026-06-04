using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneTransitionManager — Coordinador central de transiciones entre escenas.
///
/// USO:
///   1. Crea un GameObject vacío en tu escena y agrégale este componente.
///   2. Arrastra todos tus SceneTransitionAnimator a la lista "animators".
///   3. Llama a TransitionToScene("NombreEscena") para hacer un cambio de escena
///      con animaciones de salida automáticas y entrada al cargar la nueva escena.
///
/// PATRÓN RECOMENDADO:
///   - Crea una escena "PersistentUI" con este manager marcado como DontDestroyOnLoad
///     para tener transiciones globales (fade negro, cortina, etc.)
///   - O úsalo por escena para animar los elementos locales.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  SINGLETON (opcional)
    // ─────────────────────────────────────────────

    public static SceneTransitionManager Instance { get; private set; }

    [Header("═══ Singleton ═══")]
    [Tooltip("Si está activado, este manager persiste entre escenas (DontDestroyOnLoad)")]
    public bool persistBetweenScenes = false;

    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("═══ Animadores Registrados ═══")]
    [Tooltip("Lista de animadores a controlar. Se ejecutan en el orden indicado.")]
    public List<AnimatorEntry> animators = new List<AnimatorEntry>();

    [Header("═══ Configuración de Escena ═══")]
    [Tooltip("¿Reproducir animación de entrada automáticamente al cargar la escena?")]
    public bool autoPlayEnterOnSceneLoad = true;

    [Tooltip("Tiempo de espera entre el fin de las salidas y el cambio de escena")]
    [Range(0f, 1f)]
    public float delayAfterExit = 0.1f;

    // ─────────────────────────────────────────────
    //  EVENTOS GLOBALES
    // ─────────────────────────────────────────────

    /// <summary>Se dispara cuando TODOS los animadores de entrada terminaron.</summary>
    public event Action OnAllEnterComplete;

    /// <summary>Se dispara cuando TODOS los animadores de salida terminaron.</summary>
    public event Action OnAllExitComplete;

    // ─────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────

    private bool _isTransitioning = false;

    // ─────────────────────────────────────────────
    //  DATA CLASS
    // ─────────────────────────────────────────────

    [Serializable]
    public class AnimatorEntry
    {
        [Tooltip("El componente animador")]
        public SceneTransitionAnimator animator;

        [Tooltip("Retardo escalonado adicional (stagger) para este elemento")]
        [Range(0f, 2f)]
        public float staggerDelay = 0f;

        [Tooltip("¿Este animador participa en las transiciones de salida?")]
        public bool includeInExit = true;

        [Tooltip("¿Este animador participa en las transiciones de entrada?")]
        public bool includeInEnter = true;
    }

    // ─────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (persistBetweenScenes)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (autoPlayEnterOnSceneLoad)
            PlayAllEnter();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────

    /// <summary>
    /// Ejecuta las animaciones de ENTRADA en todos los animadores registrados.
    /// </summary>
    public void PlayAllEnter(Action onComplete = null)
    {
        StartCoroutine(PlayEnterSequence(onComplete));
    }

    /// <summary>
    /// Ejecuta las animaciones de SALIDA en todos los animadores registrados.
    /// </summary>
    public void PlayAllExit(Action onComplete = null)
    {
        if (_isTransitioning) return;
        StartCoroutine(PlayExitSequence(onComplete));
    }

    /// <summary>
    /// Ejecuta la salida y luego carga la escena especificada.
    /// </summary>
    public void TransitionToScene(string sceneName, Action onExitComplete = null)
    {
        if (_isTransitioning)
        {
            Debug.LogWarning("[SceneTransitionManager] Ya hay una transición en curso.");
            return;
        }

        PlayAllExit(() =>
        {
            onExitComplete?.Invoke();
            StartCoroutine(LoadSceneAfterDelay(sceneName, delayAfterExit));
        });
    }

    /// <summary>
    /// Sobrecarga con índice de escena.
    /// </summary>
    public void TransitionToScene(int sceneIndex, Action onExitComplete = null)
    {
        TransitionToScene(SceneUtility.GetScenePathByBuildIndex(sceneIndex), onExitComplete);
    }

    /// <summary>
    /// Registra un animador en tiempo de ejecución.
    /// </summary>
    public void RegisterAnimator(SceneTransitionAnimator animator, float staggerDelay = 0f)
    {
        animators.Add(new AnimatorEntry
        {
            animator = animator,
            staggerDelay = staggerDelay,
            includeInEnter = true,
            includeInExit = true
        });
    }

    /// <summary>
    /// Elimina un animador del registro.
    /// </summary>
    public void UnregisterAnimator(SceneTransitionAnimator animator)
    {
        animators.RemoveAll(e => e.animator == animator);
    }

    // ─────────────────────────────────────────────
    //  COROUTINES
    // ─────────────────────────────────────────────

    private IEnumerator PlayEnterSequence(Action onComplete)
    {
        int pending = 0;

        foreach (var entry in animators)
        {
            if (entry.animator == null || !entry.includeInEnter) continue;

            pending++;
            float delay = entry.staggerDelay;

            // Captura para la lambda
            var capturedAnimator = entry.animator;

            if (delay > 0f)
                StartCoroutine(PlayEnterWithDelay(capturedAnimator, delay, () =>
                {
                    pending--;
                }));
            else
                capturedAnimator.PlayEnter(() => pending--);
        }

        // Esperar a que todos terminen
        yield return new WaitUntil(() => pending <= 0);

        onComplete?.Invoke();
        OnAllEnterComplete?.Invoke();
    }

    private IEnumerator PlayExitSequence(Action onComplete)
    {
        _isTransitioning = true;
        int pending = 0;

        foreach (var entry in animators)
        {
            if (entry.animator == null || !entry.includeInExit) continue;

            pending++;
            float delay = entry.staggerDelay;
            var capturedAnimator = entry.animator;

            if (delay > 0f)
                StartCoroutine(PlayExitWithDelay(capturedAnimator, delay, () =>
                {
                    pending--;
                }));
            else
                capturedAnimator.PlayExit(() => pending--);
        }

        yield return new WaitUntil(() => pending <= 0);

        _isTransitioning = false;
        onComplete?.Invoke();
        OnAllExitComplete?.Invoke();
    }

    private IEnumerator PlayEnterWithDelay(SceneTransitionAnimator animator, float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        animator.PlayEnter(onComplete);
    }

    private IEnumerator PlayExitWithDelay(SceneTransitionAnimator animator, float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        animator.PlayExit(onComplete);
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);
    }
}
