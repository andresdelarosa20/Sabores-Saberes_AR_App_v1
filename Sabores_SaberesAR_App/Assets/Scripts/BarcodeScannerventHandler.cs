using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class BarcodeScanner : MonoBehaviour
{
    [SerializeField] private ContentDatabase database;

    private string _currentBarcodeValue = "";
    private bool _isTracked = false;

    void Start()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
            observer.OnTargetStatusChanged += OnStatusChanged;
    }

    void OnStatusChanged(ObserverBehaviour obs, TargetStatus status)
    {
        _isTracked = status.Status == Status.TRACKED ||
                     status.Status == Status.EXTENDED_TRACKED;

        // TargetName funciona en todas las versiones de Vuforia
        // y en un barcode target contiene el valor del código escaneado
        _currentBarcodeValue = _isTracked ? obs.TargetName : "";

        SetTapPromptVisible(_isTracked);
        Debug.Log($"[BarcodeScanner] Detectado: '{_currentBarcodeValue}'");
    }

    void Update()
    {
        if (!_isTracked || string.IsNullOrEmpty(_currentBarcodeValue)) return;

        bool tapped = Input.touchCount > 0 &&
                      Input.GetTouch(0).phase == TouchPhase.Began;
#if UNITY_EDITOR
        tapped = Input.GetMouseButtonDown(0);
#endif
        if (tapped) OnBarcodeTapped();
    }

    void OnBarcodeTapped()
    {
        ContentData content = database.GetByBarcode(_currentBarcodeValue);

        if (content != null)
        {
            GameManager.SelectedContent = content;
            SceneManager.LoadScene("DetailScene");
        }
        else
        {
            Debug.LogWarning($"[BarcodeScanner] Sin contenido para: '{_currentBarcodeValue}'");
        }
    }

    void SetTapPromptVisible(bool visible)
    {
        // Cuando tengas el UI listo, reemplaza esto por:
        // tapPromptObject.SetActive(visible);
        Debug.Log($"[BarcodeScanner] Tap prompt: {visible}");
    }
}