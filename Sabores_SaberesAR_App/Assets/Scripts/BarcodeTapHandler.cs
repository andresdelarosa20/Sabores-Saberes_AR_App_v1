using UnityEngine;
using Vuforia;
using TMPro;

public class BarcodeTapHandler : MonoBehaviour
{
    public TextMeshProUGUI barcodeText;
    public AudioSource beepSound;

    BarcodeBehaviour barcodeBehaviour;
    bool beepPlayed = false;

    void Start()
    {
        barcodeBehaviour = GetComponent<BarcodeBehaviour>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click detectado en BarcodeTapHandler");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 2f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Raycast golpeó algo: " + hit.collider.name);

                // Verifica que el objeto tocado tenga BarcodeBehaviour
                BarcodeBehaviour hitBarcode = hit.collider.GetComponent<BarcodeBehaviour>();

                if (hitBarcode != null && hitBarcode.InstanceData != null)
                {
                    barcodeText.text = hitBarcode.InstanceData.Text;
                    if (!beepPlayed)
                    {
                        beepSound.Play();
                        beepPlayed = true;
                    }
                }
            }
        }
    }
}