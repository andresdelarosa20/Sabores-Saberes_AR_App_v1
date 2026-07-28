using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class BarcodeScreen : MonoBehaviour
{
    [Header("Base de datos")]
    [SerializeField] private ContentDatabase database;
    [Header("UI (Opcional)")]
    [SerializeField] private GameObject tapPromptObject;

    private BarcodeBehaviour barcodeBehaviour;
    private string detectedCode = "";
    private bool barcodeDetected = false;

    void Start()
    {
        barcodeBehaviour = GetComponent<BarcodeBehaviour>();
        if (barcodeBehaviour == null)
        {
            Debug.LogError("[BarcodeScreen] No se encontró BarcodeBehaviour en este GameObject");
            return;
        }

        // Asegurar que tenga collider para el raycast
        if (GetComponent<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1f, 0.01f);
            Debug.Log("[BarcodeScreen] Collider agregado automáticamente a " + gameObject.name);
        }

        SetTapPromptVisible(false);
        Debug.Log("[BarcodeScreen] Iniciado correctamente");
    }

    void Update()
    {
        if (barcodeBehaviour == null) return;
        if (Camera.main == null) return;

        // Detectar si este QR específico está siendo trackeado
        if (barcodeBehaviour.InstanceData != null &&
            !string.IsNullOrEmpty(barcodeBehaviour.InstanceData.Text))
        {
            detectedCode = barcodeBehaviour.InstanceData.Text;
            barcodeDetected = true;
            SetTapPromptVisible(true);
        }
        else
        {
            detectedCode = "";
            barcodeDetected = false;
            SetTapPromptVisible(false);
            return;
        }

        // Detectar tap solo cuando este QR está activo
        bool tapped = false;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            // Verificar que el click esté sobre este QR con raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null &&
                    hit.collider.GetComponent<BarcodeBehaviour>() == barcodeBehaviour)
                {
                    tapped = true;
                }
            }
        }
#else
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null &&
                    hit.collider.GetComponent<BarcodeBehaviour>() == barcodeBehaviour)
                {
                    tapped = true;
                }
            }
        }
#endif

        if (tapped)
        {
            OnBarcodeTapped();
        }
    }

    void OnBarcodeTapped()
    {
        // Log forense - agrégalo temporalmente
        Debug.Log($"[BarcodeScreen] QR bytes: {string.Join(",", System.Text.Encoding.UTF8.GetBytes(detectedCode))}");
        foreach (var entry in database.entries)
            Debug.Log($"[BarcodeScreen] DB bytes: {string.Join(",", System.Text.Encoding.UTF8.GetBytes(entry.barcodeValue))}");

        Debug.Log("[BarcodeScreen] Buscando: " + detectedCode);

        if (database == null)
        {
            Debug.LogError("[BarcodeScreen] Database no asignada");
            return;
        }

        ContentData content = database.GetByBarcode(detectedCode);

        if (content != null)
        {
            Debug.Log("[BarcodeScreen] Encontrado: " + content.title);
            GameManager.SelectedContent = content;
            SceneManager.LoadScene("InfoScreen");
        }
        else
        {
            Debug.LogWarning("[BarcodeScreen] No existe en DB: " + detectedCode);
            foreach (var entry in database.entries)
                Debug.Log("[BarcodeScreen] DB contiene: " + entry.barcodeValue);
        }
    }

    void SetTapPromptVisible(bool visible)
    {
        if (tapPromptObject != null)
            tapPromptObject.SetActive(visible);
    }
}