// ============================================================
//  GameManager.cs
//  Clase estatica que actua como puente entre escenas.
//  No necesita estar en la escena, es solo un contenedor de estado.
// ============================================================

public static class GameManager
{
    /// <summary>
    /// Almacena el ContentData seleccionado al tocar un barcode.
    /// La escena de detalle lee este valor en su Start().
    /// </summary>
    public static ContentData SelectedContent { get; set; }
}
