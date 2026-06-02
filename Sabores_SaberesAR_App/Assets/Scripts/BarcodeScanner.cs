using UnityEngine;
using Vuforia;

public class BarcodeScanner : MonoBehaviour
{
    public TMPro.TextMeshProUGUI barcodeAsText;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit))
            {

                barcodeAsText.text = hit.collider.GetComponent<BarcodeBehaviour>().InstanceData.Text;
            }
        }
    }
}