using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ContentDatabase", menuName = "App/Content Database")]
public class ContentDatabase : ScriptableObject
{
    public List<ContentData> entries;

    public ContentData GetByBarcode(string code)
    {
        if (entries == null) return null;
        return entries.Find(e => e.barcodeValue == code);
    }
}
