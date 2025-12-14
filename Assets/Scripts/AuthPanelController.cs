using UnityEngine;
using UnityEngine.UI;

public class AuthPanelController : MonoBehaviour
{
    public GameObject profilePanel;
    public Toggle termsToggle;
    public Button loginButton;

    void Start()
    {
        loginButton.interactable = false; // desativado inicialmente
        termsToggle.onValueChanged.AddListener(OnToggleChanged);

        // Tenta auto-atribuir profilePanel se não estiver setado no inspector
        if (profilePanel == null)
        {
            var pc = FindObjectOfType<ProfilePanelController>();
            if (pc != null)
            {
                profilePanel = pc.gameObject;
                Debug.Log("AuthPanelController: profilePanel auto-assigned from ProfilePanelController.");
            }
            else
            {
                // Tenta buscar via MenuManager se presente
                var menu = FindObjectOfType<MenuManager>();
                if (menu != null && menu.profilePanel != null)
                {
                    profilePanel = menu.profilePanel;
                    Debug.Log("AuthPanelController: profilePanel auto-assigned from MenuManager.profilePanel.");
                }
                else
                {
                    Debug.LogWarning("AuthPanelController: profilePanel not assigned and could not be auto-found. Assign it in the Inspector.");
                }
            }
        }
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

        // Se usuário NÃO existir → abrir ProfilePanel
        if (!AuthManager.Instance.userExists)
        {
            if (profilePanel != null)
            {
                Debug.Log("AuthPanelController: profilePanel assigned, attempting to open...");
                var pc = profilePanel.GetComponent<ProfilePanelController>();
                if (pc != null)
                {
                    Debug.Log("AuthPanelController: calling ProfilePanelController.Open()");
                    pc.Open();
                }
                else
                {
                    Debug.Log("AuthPanelController: ProfilePanelController component missing, setting active directly.");
                    profilePanel.SetActive(true);  // mostra painel de perfil
                }
            }
            else
            {
                Debug.LogError("AuthPanelController: profilePanel reference is null!");
            }
        }
        else
        {
            // Usuário já existe → entrar direto no menu principal
            MenuManager menu = FindObjectOfType<MenuManager>();
            if (menu != null)
            {
                menu.BackToMainMenu(); // ou método específico para abrir menu pós-login
            }
        }

        gameObject.SetActive(false); // esconde painel de autenticação
    }
}
