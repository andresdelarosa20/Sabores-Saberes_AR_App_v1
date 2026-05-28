// ============================================================
//  DetailViewController.cs
//  Adjuntar a: un GameObject vacio llamado "DetailViewController"
//              en la escena DetailScene.
//
//  Dependencias:
//    - TextMeshPro (incluido en Unity)
//    - Un AudioSource en la escena (asignado en el Inspector)
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DetailViewController : MonoBehaviour
{
    // --------------------------------------------------------
    //  Referencias UI - asignar en Inspector
    // --------------------------------------------------------

    [Header("Informacion")]
    [Tooltip("TMP Text para el titulo del contenido")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Tooltip("TMP Text para la descripcion del contenido")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Tooltip("Image de UI para mostrar la imagen del contenido")]
    [SerializeField] private Image contentImage;

    [Header("Reproductor de Audio")]
    [Tooltip("AudioSource de la escena")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Cuantos segundos retrocede/avanza cada boton")]
    [SerializeField] private float seekSeconds = 10f;

    [Header("Controles de Audio - UI")]
    [Tooltip("Boton de play/pausa")]
    [SerializeField] private Button playPauseButton;

    [Tooltip("Icono dentro del boton play/pausa (opcional)")]
    [SerializeField] private Image playPauseIcon;

    [Tooltip("Sprite para el estado Play")]
    [SerializeField] private Sprite playSprite;

    [Tooltip("Sprite para el estado Pausa")]
    [SerializeField] private Sprite pauseSprite;

    [Tooltip("Slider de progreso del audio (opcional)")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("TMP Text que muestra el tiempo actual / duracion")]
    [SerializeField] private TextMeshProUGUI timeLabel;

    [Header("Navegacion")]
    [Tooltip("Nombre de la escena del scanner")]
    [SerializeField] private string scannerSceneName = "ScannerScene";

    // --------------------------------------------------------
    //  Estado interno
    // --------------------------------------------------------

    private bool _isDraggingSlider = false;

    // --------------------------------------------------------
    //  Ciclo de vida
    // --------------------------------------------------------

    void Start()
    {
        ContentData content = GameManager.SelectedContent;

        if (content == null)
        {
            Debug.LogError("[DetailViewController] GameManager.SelectedContent es null. " +
                           "Asegurate de llegar aqui desde BarcodeScanner.");
            return;
        }

        PopulateUI(content);
        SetupAudio(content.audioClip);
    }

    void Update()
    {
        if (audioSource == null || audioSource.clip == null) return;
        if (!_isDraggingSlider)
            UpdateProgressUI();
    }

    // --------------------------------------------------------
    //  Inicializacion de UI y Audio
    // --------------------------------------------------------

    private void PopulateUI(ContentData content)
    {
        if (titleText != null)       titleText.text       = content.title;
        if (descriptionText != null) descriptionText.text = content.description;

        if (contentImage != null)
        {
            contentImage.sprite  = content.image;
            contentImage.enabled = content.image != null;
        }
    }

    private void SetupAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.Play();

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value    = 0f;
        }

        UpdatePlayPauseIcon();
    }

    // --------------------------------------------------------
    //  Controles de Audio (llamar desde los botones del Canvas)
    // --------------------------------------------------------

    /// <summary>Alterna entre reproducir y pausar el audio.</summary>
    public void TogglePlayPause()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.UnPause();

        UpdatePlayPauseIcon();
    }

    /// <summary>Retrocede seekSeconds segundos.</summary>
    public void Rewind()
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.time = Mathf.Max(0f, audioSource.time - seekSeconds);
    }

    /// <summary>Avanza seekSeconds segundos.</summary>
    public void FastForward()
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.time = Mathf.Min(audioSource.clip.length - 0.1f,
                                     audioSource.time + seekSeconds);
    }

    // --------------------------------------------------------
    //  Slider de progreso (llamar desde los eventos del Slider)
    // --------------------------------------------------------

    /// <summary>Llamar desde el evento OnValueChanged del Slider.</summary>
    public void OnSliderChanged(float value)
    {
        if (audioSource == null || audioSource.clip == null) return;
        if (_isDraggingSlider)
            audioSource.time = value * audioSource.clip.length;
    }

    /// <summary>Llamar desde el evento OnPointerDown del Slider.</summary>
    public void OnSliderBeginDrag()  => _isDraggingSlider = true;

    /// <summary>Llamar desde el evento OnPointerUp del Slider.</summary>
    public void OnSliderEndDrag()    => _isDraggingSlider = false;

    // --------------------------------------------------------
    //  Navegacion
    // --------------------------------------------------------

    /// <summary>Vuelve a la escena del scanner y detiene el audio.</summary>
    public void GoBack()
    {
        if (audioSource != null) audioSource.Stop();
        SceneManager.LoadScene(scannerSceneName);
    }

    // --------------------------------------------------------
    //  Helpers internos
    // --------------------------------------------------------

    private void UpdateProgressUI()
    {
        if (audioSource.clip == null) return;

        float progress = audioSource.time / audioSource.clip.length;

        if (progressSlider != null)
            progressSlider.value = progress;

        if (timeLabel != null)
            timeLabel.text = $"{FormatTime(audioSource.time)} / {FormatTime(audioSource.clip.length)}";
    }

    private void UpdatePlayPauseIcon()
    {
        if (playPauseIcon == null) return;
        playPauseIcon.sprite = audioSource.isPlaying ? pauseSprite : playSprite;
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
