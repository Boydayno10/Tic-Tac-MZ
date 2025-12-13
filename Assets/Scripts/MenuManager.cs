using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject modePanel;
    private void Start()
    {
        mainPanel.SetActive(true);
        modePanel.SetActive(false);
    }
    public void OpenModePanel()
    {
        mainPanel.SetActive(false);
        modePanel.SetActive(true);
    }
    public void BackToMainMenu()
    {
        modePanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    public void PlaySingle()
    {
        GameMode.mode = "single";
        SceneManager.LoadScene("Game");
    }
    public void PlayVS()
    {
        GameMode.mode = "vs";
        SceneManager.LoadScene("GameVs");
    }
    public void PlayOnline()
    {
        GameMode.mode = "online";
        SceneManager.LoadScene("LobbyOnline");
    }
    public void OpenOptions()
    {
        Debug.Log("Opções futuras...");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}