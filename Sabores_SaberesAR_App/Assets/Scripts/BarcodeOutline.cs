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

        // Convertir de local a world space
        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = transform.TransformPoint(vertices[i]);
        }

        lineRenderer.SetPositions(worldVertices);
    }

    void Update()
    {
        if (barcodeBehaviour == null) return;

        if (barcodeBehaviour.InstanceData == null ||
            string.IsNullOrEmpty(barcodeBehaviour.InstanceData.Text))
        {
            lineRenderer.enabled = false;
            currentVertices = null;
            return;
        }

        // Redibujar cada frame si hay vértices guardados
        if (currentVertices != null && lineRenderer.enabled)
        {
            Vector3[] worldVertices = new Vector3[currentVertices.Length];
            for (int i = 0; i < currentVertices.Length; i++)
            {
                worldVertices[i] = transform.TransformPoint(currentVertices[i]);
            }
            lineRenderer.SetPositions(worldVertices);
        }
    }

    void OnDestroy()
    {
        if (barcodeBehaviour != null)
            barcodeBehaviour.OnBarcodeOutlineChanged -= OnBarcodeOutlineChanged;
    }
}