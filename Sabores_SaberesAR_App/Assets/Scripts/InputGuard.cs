using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// InputGuard — Deshabilita los Buttons de la escena brevemente al cargar,
/// sin tocar el EventSystem. Más seguro y compatible con DontDestroyOnLoad.
///
/// SETUP: agregar al InfoScreenManager junto con InfoScreen.cs
/// </summary>
public class InputGuard : MonoBehaviour
{
    [Tooltip("Segundos que se deshabilitan los botones al entrar a la escena")]
    [Range(0.1f, 1f)]
    public float blockDuration = 0.25f;

    private void Start()
    {
        StartCoroutine(BlockButtons());
    }

    private IEnumerator BlockButtons()
    {
        // Buscar todos los botones en la escena y deshabilitarlos
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
            btn.interactable = false;

        float elapsed = 0f;
        while (elapsed < blockDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Reactivar todos
        foreach (var btn in buttons)
            btn.interactable = true;

        Destroy(this);
    }
}
