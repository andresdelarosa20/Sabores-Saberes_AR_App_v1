using UnityEngine;
using Vuforia;

[RequireComponent(typeof(BarcodeBehaviour))]
public class BarcodeOutline : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float lineWidth = 0.01f;

    private BarcodeBehaviour barcodeBehaviour;
    private LineRenderer lineRenderer;
    private Vector3[] currentVertices = null;

    void Start()
    {
        barcodeBehaviour = GetComponent<BarcodeBehaviour>();

        if (barcodeBehaviour == null)
        {
            Debug.LogError("[BarcodeOutline] No se encontró BarcodeBehaviour");
            return;
        }

        // Obtener el existente o crear uno nuevo
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        lineRenderer.enabled = false;

        barcodeBehaviour.OnBarcodeOutlineChanged += OnBarcodeOutlineChanged;
    }

    void OnBarcodeOutlineChanged(Vector3[] vertices)
    {
        if (vertices == null || vertices.Length < 4)
        {
            lineRenderer.enabled = false;
            currentVertices = null;
            return;
        }

        currentVertices = vertices;
        lineRenderer.enabled = true;
        lineRenderer.SetPositions(vertices);
    }

    void Update()
    {
        // Si no hay vértices activos, ocultar
        if (barcodeBehaviour.InstanceData == null ||
            string.IsNullOrEmpty(barcodeBehaviour.InstanceData.Text))
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                currentVertices = null;
            }
        }
    }

    void OnDestroy()
    {
        if (barcodeBehaviour != null)
            barcodeBehaviour.OnBarcodeOutlineChanged -= OnBarcodeOutlineChanged;
    }
}