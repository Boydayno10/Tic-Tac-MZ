using UnityEngine;
using UnityEngine.UI;

public class AuthPanelController : MonoBehaviour
{
    public Toggle termsToggle;
    public Button loginButton;

    void Start()
    {
        loginButton.interactable = false; // desativado inicialmente
        termsToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        loginButton.interactable = isOn; // habilita botão quando marcado
    }

public void OnLoginButtonClicked()
{
    if (!termsToggle.isOn)
    {
        Debug.Log("Aceite os termos para continuar");
        return;
    }

    Debug.Log("Iniciando login...");
    AuthManager.Instance.Login();
}

}
