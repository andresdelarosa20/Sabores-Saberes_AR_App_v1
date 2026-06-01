using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ESCENA: InfoScreen
///
/// Jerarquía del Canvas:
///
/// [Canvas]
/// ├── TitleText              (TextMeshProUGUI)
/// ├── DescriptionText        (TextMeshProUGUI)
/// ├── ContentImage           (Image)
/// └── AudioPlayer
///     ├── TimeSlider         (Slider)  ← el usuario arrastra para ir a cualquier segundo
///     ├── CurrentTimeText    (TextMeshProUGUI)  ← "0:32"
///     ├── TotalTimeText      (TextMeshProUGUI)  ← "3:45"
///     ├── BtnRewind          (Button)  ← retrocede seekSeconds
///     ├── BtnPlayPause       (Button)  ← play / pausa
///     └── BtnForward         (Button)  ← adelanta seekSeconds
/// [BtnBack] (Button, fuera del player)
///
/// GameObject "InfoScreenManager":
///   - Este script (InfoScreen.cs)
///   - AudioSource  ← Unity necesita este componente para REPRODUCIR el clip
///                     que viene de la base de datos. Sin él no hay sonido.
/// </summary>
public class InfoScreen : MonoBehaviour
{
    [Header("UI - Información")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image           contentImage;

    [Header("UI - Reproductor")]
    [SerializeField] private Slider          timeSlider;        // El usuario arrastra aquí
    [SerializeField] private TextMeshProUGUI currentTimeText;   // "0:32"
    [SerializeField] private TextMeshProUGUI totalTimeText;     // "3:45"
    [SerializeField] private Button          btnPlayPause;
    [SerializeField] private Button          btnRewind;
    [SerializeField] private Button          btnForward;
    [SerializeField] private float           seekSeconds = 10f; // Cuánto salta cada botón

    [Header("UI - Navegación")]
    [SerializeField] private Button          btnBack;

    // AudioSource: componente de Unity que reproduce el clip.
    // El CLIP viene de la base de datos; el AudioSource es el "motor" que lo toca.
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private bool _isDraggingSlider = false; // Para no mover el slider mientras el usuario lo arrastra

    // ------------------------------------------------------------------ //

    void Start()
    {
        ContentData content = GameManager.SelectedContent;

        if (content == null)
        {
            Debug.LogError("[InfoScreen] GameManager.SelectedContent es null. " +
                           "¿Llegaste aquí sin escanear un QR?");
            return;
        }

        // --- Poblar la UI con los datos de la base de datos ---
        if (titleText       != null) titleText.text       = content.title;
        if (descriptionText != null) descriptionText.text = content.description;
        if (contentImage    != null && content.image != null)
            contentImage.sprite = content.image;

        // --- Cargar el clip de la DB en el AudioSource y reproducir ---
        if (audioSource != null && content.audioClip != null)
        {
            audioSource.clip = content.audioClip;
            audioSource.Play();

            // Configurar el slider con la duración total del clip
            if (timeSlider != null)
            {
                timeSlider.minValue = 0f;
                timeSlider.maxValue = content.audioClip.length;
                timeSlider.value    = 0f;
            }

            if (totalTimeText != null)
                totalTimeText.text = FormatTime(content.audioClip.length);
        }

        // --- Listeners de botones ---
        if (btnPlayPause != null) btnPlayPause.onClick.AddListener(TogglePlayPause);
        if (btnRewind    != null) btnRewind.onClick.AddListener(Rewind);
        if (btnForward   != null) btnForward.onClick.AddListener(FastForward);
        if (btnBack      != null) btnBack.onClick.AddListener(GoBack);

        // --- Listeners del slider ---
        if (timeSlider != null)
        {
            // Cuando el usuario EMPIEZA a arrastrar: pausar la actualización automática
            timeSlider.onValueChanged.AddListener(OnSliderDrag);

            // Usamos EventTrigger para detectar cuando suelta el slider
            AddSliderPointerEvents();
        }
    }

    // ------------------------------------------------------------------ //
    // Update: mantiene el slider y el tiempo sincronizados con el audio

    void Update()
    {
        if (audioSource == null || audioSource.clip == null) return;
        if (_isDraggingSlider) return; // No actualizar mientras el usuario arrastra

        // Mover el slider al tiempo actual del audio
        if (timeSlider != null)
            timeSlider.value = audioSource.time;

        // Actualizar el texto de tiempo actual
        if (currentTimeText != null)
            currentTimeText.text = FormatTime(audioSource.time);
    }

    // ------------------------------------------------------------------ //
    // Controles de audio

    public void TogglePlayPause()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.UnPause();
    }

    public void Rewind()
    {
        if (audioSource == null) return;
        audioSource.time = Mathf.Max(0f, audioSource.time - seekSeconds);
    }

    public void FastForward()
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.time = Mathf.Min(audioSource.clip.length, audioSource.time + seekSeconds);
    }

    // ------------------------------------------------------------------ //
    // Slider: el usuario arrastra para ir a cualquier segundo

    void OnSliderDrag(float value)
    {
        // Solo actualiza el texto mientras arrastra, no mueve el audio todavía
        if (currentTimeText != null)
            currentTimeText.text = FormatTime(value);
    }

    void OnSliderPointerDown()
    {
        _isDraggingSlider = true;
    }

    void OnSliderPointerUp()
    {
        _isDraggingSlider = false;

        // Cuando suelta, saltar al segundo seleccionado
        if (audioSource != null && timeSlider != null)
            audioSource.time = timeSlider.value;
    }

    /// Agrega los eventos de puntero al slider usando EventTrigger
    void AddSliderPointerEvents()
    {
        var trigger = timeSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = timeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // PointerDown
        var down = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
        };
        down.callback.AddListener((_) => OnSliderPointerDown());
        trigger.triggers.Add(down);

        // PointerUp
        var up = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
        };
        up.callback.AddListener((_) => OnSliderPointerUp());
        trigger.triggers.Add(up);
    }

    // ------------------------------------------------------------------ //
    // Navegación

    public void GoBack()
    {
        if (audioSource != null) audioSource.Stop();
        SceneManager.LoadScene("BarcodeScreen");
    }

    // ------------------------------------------------------------------ //
    // Utilidades

    /// Convierte segundos a formato "M:SS"  ej: 93.5 → "1:33"
    string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m}:{s:00}";
    }

    void OnDestroy()
    {
        if (btnPlayPause != null) btnPlayPause.onClick.RemoveListener(TogglePlayPause);
        if (btnRewind    != null) btnRewind.onClick.RemoveListener(Rewind);
        if (btnForward   != null) btnForward.onClick.RemoveListener(FastForward);
        if (btnBack      != null) btnBack.onClick.RemoveListener(GoBack);
        if (timeSlider   != null) timeSlider.onValueChanged.RemoveListener(OnSliderDrag);
    }
}
