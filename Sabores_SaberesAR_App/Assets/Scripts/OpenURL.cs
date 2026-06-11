using UnityEngine;
using UnityEngine.UI;

public class OpenURL : MonoBehaviour
{
    [SerializeField] private Button botonInstagram;
    [SerializeField] private Button botonFacebook;

    private string instagramURL = "https://www.instagram.com/saboressaberes/";
    private string facebookURL = "https://www.facebook.com/p/SaboresSaberes-100083363139700/";

    void Start()
    {
        botonInstagram.onClick.AddListener(() => Application.OpenURL(instagramURL));
        botonFacebook.onClick.AddListener(() => Application.OpenURL(facebookURL));
    }

}