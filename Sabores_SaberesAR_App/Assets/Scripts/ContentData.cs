using UnityEngine;

[System.Serializable]
public class ContentData
{
    public string barcodeValue;       // Debe coincidir exactamente con el QR escaneado
    public string title;
    [TextArea(2, 5)] public string description;
    public Sprite image;
    public AudioClip audioClip;
}