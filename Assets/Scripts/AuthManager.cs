using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public bool userExists = false; // SIMULA se o usuário existe ou não

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
    }

    public void Login()
    {
        Debug.Log("Login Google SIMULADO com sucesso");

        if (userExists)
        {
            Debug.Log("Usuário já existe → Login OK");
        }
        else
        {
            Debug.Log("Usuário NÃO existe → Criar perfil");
        }
    }
}
