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
            CanvasGroup cg = piece.GetComponent<CanvasGroup>();
            if (cg == null) cg = piece.gameObject.AddComponent<CanvasGroup>();

            bool enable = false;

            // --- Pieces that are ON the board (inside a Cell)
            if (piece.currentCell != null)
            {
                // Always fully visible on the board
                cg.alpha = 1f;

                if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
                {
                    bool isMyPiece = piece.type == GameManagerOnline.Instance.myPiece;
                    bool myTurn = GameManagerOnline.Instance.IsMyTurn();

                    // On-board pieces are only interactive in movement phase and only for the owner on their turn
                    enable = !gameOver && isMyPiece && myTurn && !isPlacementPhase;
                }
                else
                {
                    bool myTurnLocal = piece.type == currentTurn;
                    enable = !gameOver && myTurnLocal && !isPlacementPhase;
                }
            }
            // --- Pieces that are OFF the board (TopPieces / BottomPieces)
            else
            {
                if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
                {
                    bool isMyPiece = piece.type == GameManagerOnline.Instance.myPiece;
                    bool myTurn = GameManagerOnline.Instance.IsMyTurn();

                    // Owner's off-board pieces are full when it's their turn, faded otherwise
                    if (isMyPiece) cg.alpha = myTurn ? 1f : 0.4f;
                    else cg.alpha = myTurn ? 0.4f : 1f;

                    // Off-board pieces are interactive only during placement phase and only for the owner on their turn
                    enable = !gameOver && isMyPiece && myTurn && isPlacementPhase;
                }
                else
                {
                    // Singleplayer / local: compare to GameManager.currentTurn
                    cg.alpha = (piece.type == currentTurn) ? 1f : 0.4f;
                    bool myTurnLocal = piece.type == currentTurn;
                    enable = !gameOver && myTurnLocal && isPlacementPhase;
                }
            }

            piece.enabled = enable;
            cg.blocksRaycasts = enable;
            cg.interactable = enable;

            if (!piece.enabled) piece.Deselect();
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
                // Stop any previous blinking and ensure borders are at full alpha
                StopAllBlinkingAndFixAlpha(null);

                Debug.Log("Vitória → " + lastPiece.type);

                // Start blinking the winning combo 3 times, then show the WinMenu (or RPC it)
                if (blinkCoroutine != null)
                    StopCoroutine(blinkCoroutine);

                blinkCoroutine = StartCoroutine(BlinkWinningCombo(combo, lastPiece.type));

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

    private System.Collections.IEnumerator BlinkWinningCombo(int[] combo, PieceType winner)
    {
        SelectablePiece[] winPieces = new SelectablePiece[3];
        for (int i = 0; i < 3; i++)
        {
            int idx = combo[i];
            if (idx >= 0 && idx < cells.Length && cells[idx].IsOccupied())
                winPieces[i] = cells[idx].GetCurrentPiece();
            else
                winPieces[i] = null;
        }

        float fullCycle = Mathf.Max(0.01f, 1f / Mathf.Max(0.0001f, blinkSpeed));
        float half = fullCycle * 0.5f;

        int flashes = 3;
        for (int f = 0; f < flashes; f++)
        {
            // Fade out
            float t = 0f;
            while (t < half)
            {
                float a = Mathf.Lerp(1f, 0f, t / half);
                foreach (var p in winPieces)
                {
                    if (p != null && p.border != null)
                    {
                        var c = p.border.color;
                        p.border.color = new Color(c.r, c.g, c.b, a);
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }
            // ensure fully invisible
            foreach (var p in winPieces)
            {
                if (p != null && p.border != null)
                {
                    var c = p.border.color;
                    p.border.color = new Color(c.r, c.g, c.b, 0f);
                }
            }

            // Fade in
            t = 0f;
            while (t < half)
            {
                float a = Mathf.Lerp(0f, 1f, t / half);
                foreach (var p in winPieces)
                {
                    if (p != null && p.border != null)
                    {
                        var c = p.border.color;
                        p.border.color = new Color(c.r, c.g, c.b, a);
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }
            // ensure fully visible
            foreach (var p in winPieces)
            {
                if (p != null && p.border != null)
                {
                    var c = p.border.color;
                    p.border.color = new Color(c.r, c.g, c.b, 1f);
                }
            }
        }

        // After blinking, show the result (online: RPC; local: direct)
        if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
        {
            GameManagerOnline.Instance.photonView.RPC(
                "RPC_ShowWinner",
                Photon.Pun.RpcTarget.All,
                (int)winner
            );
        }
        else
        {
            WinMenuUI.Instance.ShowWinner(winner);
        }

        blinkCoroutine = null;
        yield break;
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
