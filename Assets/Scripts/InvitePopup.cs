using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvitePopup : MonoBehaviour
{
    public static InvitePopup Instance;

    public TextMeshProUGUI message;
    public Button acceptBtn, declineBtn;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(string from, System.Action accept, System.Action decline)
    {
        if(Instance == null)
        {
            Debug.LogError("InvitePopup Instance is null!");
            return;
        }

        Instance.gameObject.SetActive(true);
        Instance.message.text = $"{from} quer jogar contigo.";

        Instance.acceptBtn.onClick.RemoveAllListeners();
        Instance.declineBtn.onClick.RemoveAllListeners();

        Instance.acceptBtn.onClick.AddListener(() =>
        {
            Instance.gameObject.SetActive(false);
            accept.Invoke();
        });

        Instance.declineBtn.onClick.AddListener(() =>
        {
            Instance.gameObject.SetActive(false);
            decline.Invoke();
        });
    }
}
