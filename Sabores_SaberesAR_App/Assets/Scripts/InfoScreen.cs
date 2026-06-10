using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESCENA: InfoScreen
///
/// Jerarquía del Canvas:
///
/// [Canvas]
/// ├── TitleText                  (TextMeshProUGUI)
/// ├── DescriptionText            (TextMeshProUGUI)
/// ├── ContentImage               (Image)
/// ├── TitleCard1                 (TextMeshProUGUI)
/// ├── TitleCard2                 (TextMeshProUGUI)
/// ├── TitleCard3                 (TextMeshProUGUI)
/// ├── DescriptionImage1          (TextMeshProUGUI)
/// ├── DescriptionImage2          (TextMeshProUGUI)
/// ├── DescriptionImage3          (TextMeshProUGUI)
/// ├── ImageCard1                 (Image)
/// ├── ImageCard2                 (Image)
/// ├── ImageCard3                 (Image)
/// └── AudioPlayer
///     ├── TimeSlider             (Slider)
///     ├── CurrentTimeText        (TextMeshProUGUI)
///     ├── TotalTimeText          (TextMeshProUGUI)
///     ├── BtnRewind              (Button)
///     ├── BtnPlayPause           (Button)
///     │     └── Image            ← el Image del botón que cambia de sprite
///     └── BtnForward             (Button)
/// [BtnBack] (Button, fuera del player)
/// </summary>
public class InfoScreen : MonoBehaviour
{
    [Header("UI - Información principal")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image contentImage;

    [Header("UI - Cards (títulos)")]
    [SerializeField] private TextMeshProUGUI titleCard1Text;
    [SerializeField] private TextMeshProUGUI titleCard2Text;
    [SerializeField] private TextMeshProUGUI titleCard3Text;

    [Header("UI - Cards (descripciones)")]
    [SerializeField] private TextMeshProUGUI descriptionImage1Text;
    [SerializeField] private TextMeshProUGUI descriptionImage2Text;
    [SerializeField] private TextMeshProUGUI descriptionImage3Text;

    [Header("UI - Cards (imágenes)")]
    [SerializeField] private Image imageCard1;
    [SerializeField] private Image imageCard2;
    [SerializeField] private Image imageCard3;

    [Header("UI - Reproductor")]
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private Button btnPlayPause;
    [SerializeField] private Button btnRewind;
    [SerializeField] private Button btnForward;
    [SerializeField] private float seekSeconds = 10f;

    [Header("UI - Sprites Play/Pause")]
    [Tooltip("Sprite que se muestra cuando el audio está REPRODUCIÉNDOSE (ícono de pausa)")]
    [SerializeField] private Sprite spritePause;   // ícono ⏸ (se muestra cuando está playing)
    [Tooltip("Sprite que se muestra cuando el audio está PAUSADO (ícono de play)")]
    [SerializeField] private Sprite spritePlay;    // ícono ▶ (se muestra cuando está paused)

    [Header("UI - Navegación")]
    [SerializeField] private Button btnBack;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    // Image del botón play/pause para cambiar el sprite
    private Image _btnPlayPauseImage;
    private bool _isDraggingSlider = false;

    // ------------------------------------------------------------------ //

    void Start()
    {
        ContentData content = GameManager.SelectedContent;

        if (content == null)
        {
            Debug.LogError("[InfoScreen] GameManager.SelectedContent es null.");
            return;
        }

        // ── Información principal ──────────────────────────────────────
        SetText(titleText, content.title);
        SetText(descriptionText, content.descriptionimage1);
        SetText(descriptionText, content.descriptionimage2);
        SetText(descriptionText, content.descriptionimage3);
        SetImage(contentImage, content.image);

        // ── Cards ──────────────────────────────────────────────────────
        SetText(titleCard1Text, content.titlecard1);
        SetText(titleCard2Text, content.titlecard2);
        SetText(titleCard3Text, content.titlecard3);

        SetText(descriptionImage1Text, content.descriptionimage1);
        SetText(descriptionImage2Text, content.descriptionimage2);
        SetText(descriptionImage3Text, content.descriptionimage3);

        SetImage(imageCard1, content.imagecard1);
        SetImage(imageCard2, content.imagecard2);
        SetImage(imageCard3, content.imagecard3);

        // ── Reproductor ────────────────────────────────────────────────
        if (audioSource != null && content.audioClip != null)
        {
            audioSource.clip = content.audioClip;
            audioSource.Play();

            if (timeSlider != null)
            {
                timeSlider.minValue = 0f;
                timeSlider.maxValue = content.audioClip.length;
                timeSlider.value = 0f;
            }

            if (totalTimeText != null)
                totalTimeText.text = FormatTime(content.audioClip.length);
        }

        // ── Imagen del botón play/pause ────────────────────────────────
        if (btnPlayPause != null)
        {
            // Buscar el componente Image en el botón o en su hijo directo
            _btnPlayPauseImage = btnPlayPause.GetComponent<Image>();
            if (_btnPlayPauseImage == null)
                _btnPlayPauseImage = btnPlayPause.GetComponentInChildren<Image>();

            UpdatePlayPauseSprite(); // Sincronizar sprite con el estado inicial
        }

        // ── Listeners ─────────────────────────────────────────────────
        if (btnPlayPause != null) btnPlayPause.onClick.AddListener(TogglePlayPause);
        if (btnRewind != null) btnRewind.onClick.AddListener(Rewind);
        if (btnForward != null) btnForward.onClick.AddListener(FastForward);
        if (btnBack != null) btnBack.onClick.AddListener(GoBack);

        if (timeSlider != null)
        {
            timeSlider.onValueChanged.AddListener(OnSliderDrag);
            AddSliderPointerEvents();
        }
    }

    // ------------------------------------------------------------------ //

    void Update()
    {
        if (audioSource == null || audioSource.clip == null) return;
        if (_isDraggingSlider) return;

        if (timeSlider != null)
            timeSlider.value = audioSource.time;

        if (currentTimeText != null)
            currentTimeText.text = FormatTime(audioSource.time);
    }

    // ------------------------------------------------------------------ //
    //  Controles de audio

    public void TogglePlayPause()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.UnPause();

        UpdatePlayPauseSprite();
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
    //  Sprite del botón play/pause

    /// Actualiza el sprite según si el audio está reproduciéndose o pausado
    private void UpdatePlayPauseSprite()
    {
        if (_btnPlayPauseImage == null) return;

        // Si está reproduciendo → mostrar ícono de PAUSA
        // Si está pausado      → mostrar ícono de PLAY
        if (audioSource != null && audioSource.isPlaying)
        {
            if (spritePause != null) _btnPlayPauseImage.sprite = spritePause;
        }
        else
        {
            if (spritePlay != null) _btnPlayPauseImage.sprite = spritePlay;
        }
    }

    // ------------------------------------------------------------------ //
    //  Slider

    void OnSliderDrag(float value)
    {
        if (currentTimeText != null)
            currentTimeText.text = FormatTime(value);
    }

    void OnSliderPointerDown() => _isDraggingSlider = true;

    void OnSliderPointerUp()
    {
        _isDraggingSlider = false;
        if (audioSource != null && timeSlider != null)
            audioSource.time = timeSlider.value;
    }

    void AddSliderPointerEvents()
    {
        var trigger = timeSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = timeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var down = new UnityEngine.EventSystems.EventTrigger.Entry
        { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        down.callback.AddListener((_) => OnSliderPointerDown());
        trigger.triggers.Add(down);

        var up = new UnityEngine.EventSystems.EventTrigger.Entry
        { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
        up.callback.AddListener((_) => OnSliderPointerUp());
        trigger.triggers.Add(up);
    }

    // ------------------------------------------------------------------ //
    //  Navegación

    public void GoBack()
    {
        if (audioSource != null) audioSource.Stop();
        SceneManager.LoadScene("BarcodeScreen");
    }

    // ------------------------------------------------------------------ //
    //  Helpers

    /// Asigna texto solo si el campo UI y el valor existen
    private void SetText(TextMeshProUGUI field, string value)
    {
        if (field == null) return;
        field.text = value ?? "";

        // Ocultar el GameObject si no hay contenido
        field.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    /// Asigna un sprite a una Image; oculta el GameObject si no hay sprite
    private void SetImage(Image field, Sprite sprite)
    {
        if (field == null) return;
        field.sprite = sprite;
        field.gameObject.SetActive(sprite != null);
    }

    /// Convierte segundos a "M:SS"
    string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m}:{s:00}";
    }

    // ------------------------------------------------------------------ //

    void OnDestroy()
    {
        if (btnPlayPause != null) btnPlayPause.onClick.RemoveListener(TogglePlayPause);
        if (btnRewind != null) btnRewind.onClick.RemoveListener(Rewind);
        if (btnForward != null) btnForward.onClick.RemoveListener(FastForward);
        if (btnBack != null) btnBack.onClick.RemoveListener(GoBack);
        if (timeSlider != null) timeSlider.onValueChanged.RemoveListener(OnSliderDrag);
    }
}
