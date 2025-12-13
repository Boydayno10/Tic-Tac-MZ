using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject modePanel;
    public GameObject authPanel; // painel de autenticação
    public GameObject googleButton; // botão do Google

    private void Start()
    {
        mainPanel.SetActive(true);
        modePanel.SetActive(false);
        authPanel.SetActive(false); // inicia desativado
        if (googleButton != null) googleButton.SetActive(true); // Google Button visível inicialmente
    }

    // Abre painel de modo de jogo
    public void OpenModePanel()
    {
        mainPanel.SetActive(false);
        modePanel.SetActive(true);
        if (googleButton != null) googleButton.SetActive(false); // Desativa Google Button
    }

    public void BackToMainMenu()
    {
        modePanel.SetActive(false);
        mainPanel.SetActive(true);
        if (googleButton != null) googleButton.SetActive(true); // Ativa Google Button
    }

    // Botões de jogar
    public void PlaySingle()
    {
        GameMode.mode = "single";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void PlayVS()
    {
        GameMode.mode = "vs";
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameVs");
    }

    public void PlayOnline()
    {
        GameMode.mode = "online";
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyOnline");
    }

    // Abrir painel de opções
    public void OpenOptions()
    {
        Debug.Log("Opções futuras...");
    }

    // Sair do jogo
    public void ExitGame()
    {
        Application.Quit();
    }

    // ---------------------------
    // NOVO: Funções para AuthPanel
    // ---------------------------

    // Chamada pelo botão Google
    public void OpenAuthPanel()
    {
        mainPanel.SetActive(false); // esconde o menu principal
        authPanel.SetActive(true);  // mostra painel de autenticação
        if (googleButton != null) googleButton.SetActive(false); // Desativa Google Button
    }

    // Para fechar o painel de autenticação (ex: botão cancelar)
    public void CloseAuthPanel()
    {
        authPanel.SetActive(false);
        mainPanel.SetActive(true); // volta para o menu principal
        if (googleButton != null) googleButton.SetActive(true); // Ativa Google Button
    }
}
