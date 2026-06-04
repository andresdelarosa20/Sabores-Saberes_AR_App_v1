using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Escena a cargar")]
    [SerializeField] private string sceneName;

    // Esta función aparecerá en el OnClick del botón
    public void LoadScene()
    {
        SceneTransitionManager.Instance.TransitionToScene(sceneName);
    }
}