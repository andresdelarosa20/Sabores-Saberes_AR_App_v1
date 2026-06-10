using UnityEngine;

[System.Serializable]
public class ContentData
{
    public string barcodeValue;       // Debe coincidir exactamente con el QR escaneado
    public string title;
    public string titlecard1;
    public string titlecard2;
    public string titlecard3;
    [TextArea(2, 5)] public string descriptionimage1;
    [TextArea(2, 5)] public string descriptionimage2;
    [TextArea(2, 5)] public string descriptionimage3;
    public Sprite image;
    public Sprite imagecard1;
    public Sprite imagecard2;
    public Sprite imagecard3;
    public AudioClip audioClip;
}