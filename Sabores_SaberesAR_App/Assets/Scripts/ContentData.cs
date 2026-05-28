// ============================================================
//  ContentData.cs
//  Struct que representa la informacion de un barcode.
//  No requiere ninguna dependencia adicional.
// ============================================================

using UnityEngine;

[System.Serializable]
public class ContentData
{
    [Tooltip("Valor exacto del barcode, ej: BC-001")]
    public string barcodeValue;

    [Tooltip("Titulo que se mostrara en la pantalla de detalle")]
    public string title;

    [Tooltip("Descripcion o informacion del contenido")]
    [TextArea(3, 6)]
    public string description;

    [Tooltip("Imagen asociada al contenido")]
    public Sprite image;

    [Tooltip("Audio asociado al contenido")]
    public AudioClip audioClip;
}
