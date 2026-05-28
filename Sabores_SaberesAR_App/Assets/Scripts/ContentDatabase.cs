// ============================================================
//  ContentDatabase.cs
//  ScriptableObject que actua como base de datos de barcodes.
//  Crear en Unity: clic derecho en Project > Create > VuforiaApp > Content Database
// ============================================================

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ContentDatabase", menuName = "VuforiaApp/Content Database")]
public class ContentDatabase : ScriptableObject
{
    [Tooltip("Lista de todos los contenidos registrados")]
    public List<ContentData> entries = new List<ContentData>();

    /// <summary>
    /// Busca y retorna el ContentData correspondiente al barcode dado.
    /// Retorna null si no existe entrada para ese codigo.
    /// </summary>
    public ContentData GetByBarcode(string barcodeValue)
    {
        if (string.IsNullOrEmpty(barcodeValue)) return null;

        return entries.Find(e =>
            string.Equals(e.barcodeValue, barcodeValue,
                          System.StringComparison.OrdinalIgnoreCase));
    }
}
