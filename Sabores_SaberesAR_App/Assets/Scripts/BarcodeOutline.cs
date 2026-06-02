using UnityEngine;
using Vuforia;

public class BarcodeOutline : MonoBehaviour
{
    [Header("Referencia al Barcode")]
    [SerializeField] private ObserverBehaviour barcodeTarget;

    [Header("Outline")]
    [SerializeField] private GameObject outlineObject;

    private void Start()
    {
        if (barcodeTarget != null)
        {
            barcodeTarget.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

    private void OnTargetStatusChanged(
        ObserverBehaviour observer,
        TargetStatus status)
    {
        bool tracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED;

        if (outlineObject != null)
            outlineObject.SetActive(tracked);
    }

    private void Update()
    {
        if (barcodeTarget == null || outlineObject == null)
            return;

        outlineObject.transform.position =
            barcodeTarget.transform.position;

        outlineObject.transform.rotation =
            barcodeTarget.transform.rotation;

        outlineObject.transform.localScale =
            barcodeTarget.transform.localScale * 1.05f;
    }

    private void OnDestroy()
    {
        if (barcodeTarget != null)
        {
            barcodeTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
}