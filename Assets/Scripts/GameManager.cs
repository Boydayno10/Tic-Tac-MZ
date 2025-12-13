using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Cell[] cells;
    [HideInInspector] public PieceType currentTurn;
    public bool gameOver = false;
    public PieceSpawner spawner;
    public float blinkSpeed = 4f;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        System.Array.Sort(cells, (a, b) => a.myIndex.CompareTo(b.myIndex));

        if (spawner == null)
            spawner = GetComponent<PieceSpawner>();

        spawner.SpawnInitialPieces();
        PieceManager.Instance.RefreshPieces();

        // Modo ONLINE
        if (GameMode.mode == "online")
        {
            if (GameManagerOnline.Instance != null)
            {
                currentTurn = GameManagerOnline.Instance.IsMyTurn() ?
                    GameManagerOnline.Instance.myPiece :
                    (GameManagerOnline.Instance.myPiece == PieceType.X ? PieceType.O : PieceType.X);
            }
            else currentTurn = PieceType.X;
        }
        else
        {
            // Modo SINGLEPLAYER
            currentTurn = (Random.value < 0.5f) ? PieceType.O : PieceType.X;
        }

        UpdateSelectablePieces();
    }

    public void SwitchTurn()
    {
        if (gameOver) return;

        if (GameMode.mode == "online")
        {
            GameManagerOnline.Instance.SyncGameState();
            return;
        }

        currentTurn = (currentTurn == PieceType.O) ? PieceType.X : PieceType.O;
        UpdateSelectablePieces();
    }

    public bool IsPieceTurn(PieceType type)
    {
        if (gameOver) return false;

        if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
            return type == GameManagerOnline.Instance.myPiece &&
                   GameManagerOnline.Instance.IsMyTurn();

        return type == currentTurn;
    }

    public bool IsMovementPhase(PieceType type) => CountPiecesOnBoard(type) >= 3;

    public int CountPiecesOnBoard(PieceType type)
    {
        int count = 0;
        foreach (Cell c in cells)
            if (c.IsOccupied() && c.GetCurrentPiece().type == type) count++;
        return count;
    }

    public void UpdateSelectablePieces()
    {
        bool isPlacementPhase = CountPiecesOnBoard(currentTurn) < 3;

        foreach (SelectablePiece piece in FindObjectsOfType<SelectablePiece>())
        {
            bool enable = false;

            if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
            {
                bool isMyPiece = piece.type == GameManagerOnline.Instance.myPiece;
                bool myTurn = GameManagerOnline.Instance.IsMyTurn();

                enable = !gameOver && isMyPiece && myTurn &&
                         (isPlacementPhase ? piece.currentCell == null : piece.currentCell != null);
            }
            else
            {
                bool myTurn = piece.type == currentTurn;
                enable = !gameOver && myTurn &&
                         (isPlacementPhase ? piece.currentCell == null : piece.currentCell != null);
            }

            piece.enabled = enable;

            CanvasGroup cg = piece.GetComponent<CanvasGroup>();
            if (cg == null) cg = piece.gameObject.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = enable;
            cg.interactable = enable;
            cg.alpha = enable ? 1f : 0.3f;

            if (!piece.enabled) piece.Deselect();
        }

        // Invisibilizar IA no singleplayer
        foreach (SelectablePiece piece in FindObjectsOfType<SelectablePiece>())
        {
            CanvasGroup cg = piece.GetComponent<CanvasGroup>();
            if (cg == null) cg = piece.gameObject.AddComponent<CanvasGroup>();

            if (GameMode.mode == "single")
            {
                PieceType playerType = currentTurn;
                PieceType aiType = (playerType == PieceType.X) ? PieceType.O : PieceType.X;

                if (piece.type == aiType && piece.currentCell == null)
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                }
            }
        }
    }

    // --------------------------
    //    ♟️   CHECAR VITÓRIA
    // --------------------------
    public void CheckWin(SelectablePiece lastPiece)
    {
        if (gameOver) return;

        int[][] wins = new int[][] {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };

        foreach (var combo in wins)
        {
            if (IsWinningCombo(combo, lastPiece.type))
            {
                gameOver = true;
                StopAllBlinkingAndFixAlpha(combo);

                Debug.Log("Vitória → " + lastPiece.type);

                // 🔥 RPC OU LOCAL (SEU CÓDIGO)
                if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
                {
                    GameManagerOnline.Instance.photonView.RPC(
                        "RPC_ShowWinner",
                        Photon.Pun.RpcTarget.All,
                        (int)lastPiece.type
                    );
                }
                else
                {
                    WinMenuUI.Instance.ShowWinner(lastPiece.type);
                }

                UpdateSelectablePieces();
                return;
            }
        }

        // EMPATE
        if (IsDraw())
        {
            gameOver = true;
            StopAllBlinkingAndFixAlpha(null);

            if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
            {
                GameManagerOnline.Instance.photonView.RPC(
                    "RPC_ShowWinner",
                    Photon.Pun.RpcTarget.All,
                    (int)PieceType.None
                );
            }
            else
            {
                WinMenuUI.Instance.ShowWinner(PieceType.None);
            }

            UpdateSelectablePieces();
        }
    }

    private bool IsDraw()
    {
        foreach (Cell c in cells)
            if (!c.IsOccupied()) return false;
        return true;
    }

    private bool IsWinningCombo(int[] combo, PieceType type)
    {
        foreach (int index in combo)
        {
            if (!cells[index].IsOccupied()) return false;
            if (cells[index].GetCurrentPiece().type != type) return false;
        }
        return true;
    }

    private void StopAllBlinkingAndFixAlpha(int[] combo)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        foreach (SelectablePiece p in FindObjectsOfType<SelectablePiece>())
        {
            if (p.border != null)
                p.border.color = new Color(p.border.color.r, p.border.color.g, p.border.color.b, 1f);
        }
    }

    public void ApplyMove(int pieceId, int cellIndex, bool isPlacement, PieceType type, bool isRemote = false)
    {
        if (gameOver) return;
        if (cellIndex < 0 || cellIndex >= cells.Length) return;

        Cell target = cells[cellIndex];

        if (target.IsOccupied())
            target.ClearCell();

        SelectablePiece piece = PieceManager.Instance.GetPieceById(pieceId);
        if (piece == null)
        {
            Debug.LogWarning("Peça não encontrada!");
            return;
        }

        if (!isPlacement && piece.currentCell != null)
            piece.currentCell.ClearCell();

        target.PlacePiece(piece);
        piece.Deselect();

        CheckWin(piece);

        if (GameMode.mode == "single")
            SwitchTurn();
    }
}
