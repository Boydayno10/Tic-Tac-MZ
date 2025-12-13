using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public bool userExists = false; // SIMULA se o usuário existe ou não
    public string userName = "";
    public string userBio = "";
    public int userProvince = 0;
    public int userAvatar = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Carrega usuário salvo (fake) no PlayerPrefs
        userExists = PlayerPrefs.GetInt("user_exists", 0) == 1;
        if (userExists)
        {
            userName = PlayerPrefs.GetString("user_name", "");
            userBio = PlayerPrefs.GetString("user_bio", "");
            userProvince = PlayerPrefs.GetInt("user_province", 0);
            userAvatar = PlayerPrefs.GetInt("user_avatar", -1);
        }
    }

    public void Login()
    {
        Debug.Log("Login Google SIMULADO com sucesso");

        // Atualiza o estado local com base no que está salvo
        userExists = PlayerPrefs.GetInt("user_exists", 0) == 1;

        if (userExists)
        {
            userName = PlayerPrefs.GetString("user_name", "");
            userBio = PlayerPrefs.GetString("user_bio", "");
            userProvince = PlayerPrefs.GetInt("user_province", 0);
            userAvatar = PlayerPrefs.GetInt("user_avatar", -1);
            Debug.Log("Usuário já existe → Login OK");
        }
        else
        {
            Debug.Log("Usuário NÃO existe → Criar perfil");
        }
    }

    // Cria uma conta fake e salva no PlayerPrefs para testar fluxo
    public void CreateFakeAccount(string name, string bio, int province, int avatar)
    {
        userName = name;
        userBio = bio;
        userProvince = province;
        userAvatar = avatar;
        userExists = true;

        PlayerPrefs.SetInt("user_exists", 1);
        PlayerPrefs.SetString("user_name", userName);
        PlayerPrefs.SetString("user_bio", userBio);
        PlayerPrefs.SetInt("user_province", userProvince);
        PlayerPrefs.SetInt("user_avatar", userAvatar);
        PlayerPrefs.Save();

        Debug.Log("Conta fake criada: " + userName);
    }

    public void ClearFakeAccount()
    {
        userExists = false;
        userName = ""; userBio = ""; userProvince = 0; userAvatar = -1;
        PlayerPrefs.DeleteKey("user_exists");
        PlayerPrefs.DeleteKey("user_name");
        PlayerPrefs.DeleteKey("user_bio");
        PlayerPrefs.DeleteKey("user_province");
        PlayerPrefs.DeleteKey("user_avatar");
        PlayerPrefs.Save();
        Debug.Log("Conta fake removida");
    }
}
