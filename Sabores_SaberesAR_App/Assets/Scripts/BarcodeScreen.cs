using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

/// <summary>
/// ESCENA: BarcodeScreen
/// 
/// Cómo montarlo en Unity:
/// 1. Crea una escena llamada "BarcodeScreen".
/// 2. Añade Vuforia AR Camera.
/// 3. Añade un Barcode Target (Vuforia > Barcode).
/// 4. Al GameObject del Barcode Target, añade este script.
/// 5. Arrastra el ContentDatabase.asset al campo "Database" en el Inspector.
/// 6. (Opcional) Arrastra un GameObject de UI (ej: un Text/Panel "Toca para ver")
///    al campo "tapPromptObject" para mostrarlo cuando se detecte un código.
/// </summary>
public class BarcodeScreen : MonoBehaviour
{
    [Header("Base de datos")]
    [SerializeField] private ContentDatabase database;

    [Header("UI (opcional)")]
    [SerializeField] private GameObject tapPromptObject; // Ej: "Toca para ver más"

    private string _detectedCode = "";
    private bool   _isTracked    = false;

    // ------------------------------------------------------------------ //

    void Start()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
            observer.OnTargetStatusChanged += OnTargetStatusChanged;

        SetTapPromptVisible(false);
    }

    // Se llama cada vez que Vuforia cambia el estado del target
    void OnTargetStatusChanged(ObserverBehaviour obs, TargetStatus status)
    {
        _isTracked = status.Status == Status.TRACKED ||
                     status.Status == Status.EXTENDED_TRACKED;

        if (_isTracked)
        {
            // TargetName contiene el valor del QR/barcode escaneado
            // y funciona en todas las versiones de Vuforia
            _detectedCode = obs.TargetName;
            Debug.Log($"[BarcodeScreen] QR detectado: '{_detectedCode}'");
        }
        else
        {
            _detectedCode = "";
        }

        SetTapPromptVisible(_isTracked);
    }

    void Update()
    {
        if (!_isTracked || string.IsNullOrEmpty(_detectedCode)) return;

        bool tapped = false;

#if UNITY_EDITOR
        tapped = Input.GetMouseButtonDown(0);
#else
        tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif

        if (tapped) OnBarcodeTapped();
    }

    void OnBarcodeTapped()
    {
        ContentData content = database.GetByBarcode(_detectedCode);

        if (content != null)
        {
            GameManager.SelectedContent = content;
            SceneManager.LoadScene("InfoScreen");
        }
        else
        {
            Debug.LogWarning($"[BarcodeScreen] No hay contenido para '{_detectedCode}'. " +
                             $"Verifica que el barcodeValue en ContentDatabase coincida exactamente.");
        }
    }

    void SetTapPromptVisible(bool visible)
    {
        if (tapPromptObject != null)
            tapPromptObject.SetActive(visible);
    }

    void OnDestroy()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }
}
