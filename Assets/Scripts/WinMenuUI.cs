using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinMenuUI : MonoBehaviour
{
    public static WinMenuUI Instance;

    public GameObject panelContainer;           // WinMenu (ou Panel principal)
    public TextMeshProUGUI winnerText;

    private void Awake()
    {
        // Singleton simples
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Começa desativado
        gameObject.SetActive(false);
    }

    // Chame com PieceType.X / PieceType.O ou PieceType.None (empate)
    public void ShowWinner(PieceType winner)
    {
        gameObject.SetActive(true);  // ativa o menu
        panelContainer.SetActive(true);

        if (winner == PieceType.X)
            winnerText.text = "Jogador X venceu!";
        else if (winner == PieceType.O)
            winnerText.text = "Jogador O venceu!";
        else
            winnerText.text = "Empate!";

        // Pausa a simulação (útil para congelar animações do jogo)
        Time.timeScale = 0f;

        // Opcional: garantir que o cursor e interações UI estejam OK (no build pode querer puxar cursor)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestart()
    {
        // Antes de recarregar, desfaz a pausa
        Time.timeScale = 1f;
        // Recarrega cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitToMenu()
    {
        Time.timeScale = 1f;
        // Mude "MainMenu" para o nome da sua cena de menu principal
        SceneManager.LoadScene("Main");
    }

    // Método utilitário se quiser fechar sem recarregar (não usado no exemplo)
    public void Hide()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
