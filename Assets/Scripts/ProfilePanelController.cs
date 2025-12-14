using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfilePanelController : MonoBehaviour
{
    public Button createButton;
    public TMP_InputField nameInput;
    public TMP_InputField bioInput;
    public TMP_Dropdown provinceDropdown;

    private int selectedAvatar = -1;

    public MenuManager menuManager; // <--- link para menu principal

    void Start()
    {
        // Começa ativo na hierarquia, mas invisível via CanvasGroup (alpha=0)
        // Isso permite que seja mostrado novamente ao fazer login quando não existir usuário
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        createButton.interactable = false;

        // encontra o MenuManager automaticamente se não estiver linkado
        if (menuManager == null)
            menuManager = FindObjectOfType<MenuManager>();
    }

    void Update()
    {
        ValidateForm();
    }

    public void SelectAvatar(int index)
    {
        selectedAvatar = index;
        ValidateForm();
    }

    // Abre o painel (pode ser chamado por AuthPanelController)
    public void Open()
    {
        Debug.Log("ProfilePanelController.Open() called. Activating profile panel...");

        // Reset básico e valores padrão para ativar o botão Create
        selectedAvatar = 0; // auto-seleciona o avatar 0

        // Prefill name: usa nome salvo se existir, senão usa "Player"
        if (nameInput != null)
        {
            string defaultName = "Player";
            if (AuthManager.Instance != null && AuthManager.Instance.userName != "")
                defaultName = AuthManager.Instance.userName;
            nameInput.text = defaultName;
        }

        if (bioInput != null) bioInput.text = "";
        if (provinceDropdown != null) provinceDropdown.value = 0;

        // Atualiza botão de criar com base na validação
        ValidateForm();

        // Ativa a hierarquia de pais para garantir visibilidade (se o pai estiver desativado)
        Transform t = transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }

        // Garante CanvasGroup visível e interativo
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        gameObject.SetActive(true);

        // Garantir que o Google Button fique invisível enquanto o painel de profile estiver ativo
        if (menuManager != null && menuManager.googleButton != null)
            menuManager.googleButton.SetActive(false);
    }

    // Cancela e volta para o painel de autenticação
    public void Cancel()
    {
        // Volta a deixar o panel invisível via CanvasGroup antes de desativar
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        // Mantém o GameObject ativo (mas invisível) para que possa ser mostrado
        // novamente apenas alterando o CanvasGroup alpha quando necessário.
        if (menuManager != null)
            menuManager.OpenAuthPanel();
    }

    void ValidateForm()
    {
        bool valid =
            selectedAvatar >= 0 &&
            !string.IsNullOrEmpty(nameInput.text) &&
            nameInput.text.Length >= 3;

        createButton.interactable = valid;
    }

    public void CreateProfile()
    {
        Debug.Log("Perfil criado!");
        Debug.Log("Nome: " + nameInput.text);
        Debug.Log("Província: " + provinceDropdown.options[provinceDropdown.value].text);
        Debug.Log("Bio: " + bioInput.text);
        Debug.Log("Avatar: " + selectedAvatar);

        // Salvar conta fake para testes (salva em PlayerPrefs)
        AuthManager.Instance.CreateFakeAccount(
            nameInput.text,
            bioInput.text,
            provinceDropdown.value,
            selectedAvatar
        );

        // Fecha ProfilePanel: deixa alpha = 0 e depois desativa
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        // Mantém o GameObject ativo (mas invisível) para consistência com o estado inicial
        if (menuManager != null)
        {
            menuManager.BackToMainMenu();
        }
    }
}
