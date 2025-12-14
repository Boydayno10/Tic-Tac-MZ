using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    public int myIndex;
    private SelectablePiece currentPiece = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null || gm.gameOver || IsOccupied()) return;

        SelectablePiece selected = PieceSelector.Instance.selectedX ?? PieceSelector.Instance.selectedO;
        if (selected == null) return;

        if (!gm.IsPieceTurn(selected.type)) return;

        // Online: verificação mais rigorosa
        if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
        {
            if (!GameManagerOnline.Instance.IsMyTurn() || selected.type != GameManagerOnline.Instance.myPiece)
                return;
        }

        bool isPlacement = selected.currentCell == null;
        bool isMovementPhase = gm.IsMovementPhase(selected.type);
        
        if (!isPlacement && !isMovementPhase) return;

        int pieceId = selected.pieceId;

        // Remove da célula antiga se for movimento
        if (!isPlacement && selected.currentCell != null)
            selected.currentCell.ClearCell();

        // Aplica na célula
        PlacePiece(selected);
        selected.Deselect();
        PieceSelector.Instance.ClearSelection();

        // Checa vitória
        gm.CheckWin(selected);

        // Sincroniza online
        if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
        {
            GameManagerOnline.Instance.SendMove(pieceId, myIndex, isPlacement, selected.type);
        }
        else
        {
            gm.SwitchTurn();
        }
    }

    // Resto do código permanece igual...
    public void PlacePiece(SelectablePiece piece)
    {
        if (currentPiece != null)
            currentPiece.currentCell = null;
            
        currentPiece = piece;
        piece.currentCell = this;
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
        piece.transform.localRotation = Quaternion.identity;
        piece.transform.localScale = Vector3.one;
        
        CanvasGroup cg = piece.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
    }

    public void ClearCell()
    {
        if (currentPiece != null)
            currentPiece.currentCell = null;
        currentPiece = null;
    }

    public bool IsOccupied() => currentPiece != null;
    public SelectablePiece GetCurrentPiece() => currentPiece;
}