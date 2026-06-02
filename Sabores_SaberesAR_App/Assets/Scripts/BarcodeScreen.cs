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
            Debug.LogError("[BarcodeScreen] No se encontró BarcodeBehaviour");
            return;
        }

        SetTapPromptVisible(false);

        Debug.Log("[BarcodeScreen] Iniciado correctamente");
    }

    void Update()
    {
        if (barcodeBehaviour == null)
            return;

        if (barcodeBehaviour.InstanceData != null)
        {
            detectedCode = barcodeBehaviour.InstanceData.Text;
            barcodeDetected = !string.IsNullOrEmpty(detectedCode);

            if (barcodeDetected)
            {
                Debug.Log("[BarcodeScreen] Código detectado: " + detectedCode);
                SetTapPromptVisible(true);
            }
        }
        else
        {
            barcodeDetected = false;
            detectedCode = "";
            SetTapPromptVisible(false);
        }

        if (!barcodeDetected)
            return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
#else
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
            OnBarcodeTapped();
        }
    }

    void OnBarcodeTapped()
    {
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
            Debug.LogWarning("[BarcodeScreen] No existe el código en la DB");

            foreach (var entry in database.entries)
            {
                Debug.Log("[BarcodeScreen] DB contiene: " + entry.barcodeValue);
            }
        }
    }

    void SetTapPromptVisible(bool visible)
    {
        if (tapPromptObject != null)
            tapPromptObject.SetActive(visible);
    }
}