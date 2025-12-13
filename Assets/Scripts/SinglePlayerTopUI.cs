using UnityEngine;

public class SinglePlayerTopUI : MonoBehaviour
{
    public GameObject topPieces;
    public GameObject playerInfoPanel;

    void Start()
    {
        if (GameMode.mode == "single")
        {
            HideTopPiecesOnlyVisual();

            if (playerInfoPanel != null)
                playerInfoPanel.SetActive(true);
        }
        else
        {
            if (playerInfoPanel != null)
                playerInfoPanel.SetActive(false);
        }
    }

    void HideTopPiecesOnlyVisual()
    {
        if (topPieces == null) return;

        CanvasGroup cg = topPieces.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = topPieces.AddComponent<CanvasGroup>();

        cg.alpha = 0f;           // invisível
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }
}
