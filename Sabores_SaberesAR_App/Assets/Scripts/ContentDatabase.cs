using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ContentDatabase", menuName = "App/Content Database")]
public class ContentDatabase : ScriptableObject
{
    public List<ContentData> entries;

    public ContentData GetByBarcode(string code)
    {
        string clean = code.Trim().ToUpper();
        return entries.Find(e => e.barcodeValue.Trim().ToUpper() == clean);
    }
}
