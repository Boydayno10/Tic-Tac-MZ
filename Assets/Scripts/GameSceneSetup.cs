using UnityEngine;
using System.Collections;

public class GameSceneSetup : MonoBehaviour
{
    public GameObject topPieces;
    public GameObject bottomPieces;

    private void Start()
    {
        StartCoroutine(SetupDepoisDoSpawn());
    }

    private IEnumerator SetupDepoisDoSpawn()
    {
        yield return new WaitForEndOfFrame();
        
        // IDs únicos
        var pieces = FindObjectsOfType<SelectablePiece>();
        for (int i = 0; i < pieces.Length; i++)
            pieces[i].pieceId = i;

        // Configuração específica por modo
        if (GameMode.mode == "online")
        {
            SetupOnlineMode();
        }
        else
        {
            MostrarTodos();
        }

        // Atualiza GameManager
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.UpdateSelectablePieces();
    }

    void SetupOnlineMode()
    {
        if (GameManagerOnline.Instance != null)
        {
            // GameManagerOnline já configura as peças automaticamente
            return;
        }
        MostrarTodos(); // Fallback
    }

    void MostrarTodos()
    {
        Mostrar(topPieces);
        Mostrar(bottomPieces);
    }

    void Mostrar(GameObject grupo)
    {
        foreach (Transform child in grupo.transform)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
    }
}