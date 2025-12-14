using UnityEngine;
public class AIPlayer : MonoBehaviour
{
    public PieceType aiType = PieceType.X;
    private GameManager gm;
    private Cell[] cells;
    private float delay = 0.7f;
    private bool isMakingMove = false;
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        cells = gm.cells;
    }
    private void Update()
    {
        if (GameMode.mode != "single") return;
        if (gm.gameOver || gm.currentTurn != aiType || isMakingMove) return;
        isMakingMove = true;
        Invoke(nameof(MakeMove), delay);
    }
    private void MakeMove()
    {
		// Use minimax to pick the best move for both placement and movement phases
		var best = GetBestMove();

		if (best.from == -1)
		{
			// placement
			Cell target = cells[best.to];
			if (target != null)
				ExecuteMove(target);
		}
		else
		{
			// movement: move piece at "from" to "to"
			Cell fromCell = cells[best.from];
			Cell toCell = cells[best.to];
			if (fromCell != null && fromCell.IsOccupied() && toCell != null && !toCell.IsOccupied())
			{
				SelectablePiece piece = fromCell.GetCurrentPiece();
				if (piece != null)
					MovePieceTo(piece, toCell);
			}
		}

		isMakingMove = false;
    }

	// Represents a candidate move: placement (from == -1) or movement (from -> to)
	private struct MoveChoice { public int from; public int to; public MoveChoice(int f, int t) { from = f; to = t; } }

	private MoveChoice GetBestMove()
	{
		PieceType[] board = new PieceType[cells.Length];
		for (int i = 0; i < cells.Length; i++)
			board[i] = cells[i].IsOccupied() ? cells[i].GetCurrentPiece().type : PieceType.None;

		bool isPlacement = CountPiecesOnBoard(aiType) < 3;

		int depthLimit = 8; // safe cap
		var result = Minimax(board, depthLimit, int.MinValue, int.MaxValue, true, isPlacement);
		return result.bestMove;
	}

	private (int score, MoveChoice bestMove) Minimax(PieceType[] board, int depth, int alpha, int beta, bool maximizingPlayer, bool placementPhase)
	{
		PieceType winner = CheckWinner(board);
		if (winner == aiType) return (10 + depth, new MoveChoice(-1, -1));
		PieceType opponent = (aiType == PieceType.X) ? PieceType.O : PieceType.X;
		if (winner == opponent) return (-10 - depth, new MoveChoice(-1, -1));

		bool hasEmpty = false;
		for (int i = 0; i < board.Length; i++) if (board[i] == PieceType.None) { hasEmpty = true; break; }
		if (!hasEmpty) return (0, new MoveChoice(-1, -1));

		if (depth == 0) return (0, new MoveChoice(-1, -1));

		if (maximizingPlayer)
		{
			int maxEval = int.MinValue;
			MoveChoice best = new MoveChoice(-1, -1);

			if (placementPhase)
			{
				for (int j = 0; j < board.Length; j++)
				{
					if (board[j] != PieceType.None) continue;
					board[j] = aiType;
					var eval = Minimax(board, depth - 1, alpha, beta, false, false);
					board[j] = PieceType.None;
					if (eval.score > maxEval) { maxEval = eval.score; best = new MoveChoice(-1, j); }
					alpha = Mathf.Max(alpha, maxEval);
					if (beta <= alpha) break;
				}
			}
			else
			{
				// movement: move each own piece to any empty cell
				for (int i = 0; i < board.Length; i++)
				{
					if (board[i] != aiType) continue;
					for (int j = 0; j < board.Length; j++)
					{
						if (board[j] != PieceType.None) continue;
						board[i] = PieceType.None;
						board[j] = aiType;
						var eval = Minimax(board, depth - 1, alpha, beta, false, false);
						board[i] = aiType;
						board[j] = PieceType.None;
						if (eval.score > maxEval) { maxEval = eval.score; best = new MoveChoice(i, j); }
						alpha = Mathf.Max(alpha, maxEval);
						if (beta <= alpha) break;
					}
				}
			}

			return (maxEval, best);
		}
		else
		{
			int minEval = int.MaxValue;
			MoveChoice best = new MoveChoice(-1, -1);

			// opponent moves
			bool oppPlacement = CountPiecesOnBoard(opponent) < 3;

			if (oppPlacement)
			{
				for (int j = 0; j < board.Length; j++)
				{
					if (board[j] != PieceType.None) continue;
					board[j] = opponent;
					var eval = Minimax(board, depth - 1, alpha, beta, true, false);
					board[j] = PieceType.None;
					if (eval.score < minEval) { minEval = eval.score; best = new MoveChoice(-1, j); }
					beta = Mathf.Min(beta, minEval);
					if (beta <= alpha) break;
				}
			}
			else
			{
				for (int i = 0; i < board.Length; i++)
				{
					if (board[i] != opponent) continue;
					for (int j = 0; j < board.Length; j++)
					{
						if (board[j] != PieceType.None) continue;
						board[i] = PieceType.None;
						board[j] = opponent;
						var eval = Minimax(board, depth - 1, alpha, beta, true, false);
						board[i] = opponent;
						board[j] = PieceType.None;
						if (eval.score < minEval) { minEval = eval.score; best = new MoveChoice(i, j); }
						beta = Mathf.Min(beta, minEval);
						if (beta <= alpha) break;
					}
				}
			}

			return (minEval, best);
		}
	}

	private PieceType CheckWinner(PieceType[] board)
	{
		int[][] wins = new int[][] {
			new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
			new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
			new[]{0,4,8}, new[]{2,4,6}
		};
		foreach (var combo in wins)
		{
			PieceType first = board[combo[0]];
			if (first == PieceType.None) continue;
			if (board[combo[1]] == first && board[combo[2]] == first) return first;
		}
		return PieceType.None;
	}
    private void ExecuteMove(Cell target)
    {
        int myCount = CountPiecesOnBoard(aiType);
        if (myCount < 3)
        {
            // Fase de colocação
            SelectablePiece piece = FindAvailablePiece();
            if (piece != null) PlacePiece(piece, target);
        }
        else
        {
            // Fase de movimento – move qualquer peça para o alvo
            SelectablePiece piece = GetMyPieces()[0]; // pega qualquer uma (todas podem se mover)
            if (piece.currentCell != null)
                MovePieceTo(piece, target);
        }
    }
    // ===================================================================
    // FUNÇÕES INTELIGENTES (rápidas e perfeitas)
    // ===================================================================
    private Cell FindTwoInLine(PieceType type) // retorna a casa que completa 3
    {
        int[][] lines = {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };
        foreach (var line in lines)
        {
            int count = 0;
            int emptyIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                Cell c = cells[line[i]];
                if (c.IsOccupied() && c.GetCurrentPiece().type == type) count++;
                else if (!c.IsOccupied()) emptyIndex = line[i];
            }
            if (count == 2 && emptyIndex != -1)
                return cells[emptyIndex];
        }
        return null;
    }
    private Cell FindForkOpportunity() // cria duas ameaças simultâneas
    {
        foreach (Cell cell in cells)
        {
            if (cell.IsOccupied()) continue;
            int threatsCreated = 0;
            int[][] lines = GetLinesContaining(cell);
            foreach (var line in lines)
            {
                int myCount = 0;
                int emptyCount = 0;
                foreach (int idx in line)
                {
                    Cell c = cells[idx];
                    if (c == cell) emptyCount++;
                    else if (c.IsOccupied() && c.GetCurrentPiece().type == aiType) myCount++;
                    else if (c.IsOccupied()) { myCount = -99; break; }
                    else emptyCount++;
                }
                if (myCount == 1 && emptyCount == 2) threatsCreated++;
            }
            if (threatsCreated >= 2) return cell;
        }
        return null;
    }
    private Cell FindOpponentFork() // bloqueia fork do adversário
    {
        PieceType opp = aiType == PieceType.X ? PieceType.O : PieceType.X;
        foreach (Cell cell in cells)
        {
            if (cell.IsOccupied()) continue;
            int threats = 0;
            int[][] lines = GetLinesContaining(cell);
            foreach (var line in lines)
            {
                int oppCount = 0;
                int emptyCount = 0;
                foreach (int idx in line)
                {
                    Cell c = cells[idx];
                    if (c == cell) emptyCount++;
                    else if (c.IsOccupied() && c.GetCurrentPiece().type == opp) oppCount++;
                    else if (c.IsOccupied()) { oppCount = -99; break; }
                    else emptyCount++;
                }
                if (oppCount == 1 && emptyCount == 2) threats++;
            }
            if (threats >= 2) return cell;
        }
        return null;
    }
    private int[][] GetLinesContaining(Cell cell)
    {
        int idx = System.Array.IndexOf(cells, cell);
        var list = new System.Collections.Generic.List<int[]>();
        int[][] all = {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };
        foreach (var line in all)
            if (System.Array.IndexOf(line, idx) >= 0)
                list.Add(line);
        return list.ToArray();
    }
    private Cell GetBestStrategicCell()
    {
        // Prioridade: centro → cantos → lados
        int[] order = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };
        foreach (int i in order)
        {
            if (!cells[i].IsOccupied())
                return cells[i];
        }
        return null;
    }
    // ===================================================================
    // FUNÇÕES AUXILIARES (iguais ou simplificadas)
    // ===================================================================
    private SelectablePiece[] GetMyPieces()
    {
        return System.Array.FindAll(FindObjectsOfType<SelectablePiece>(), p => p.type == aiType);
    }
    private int CountPiecesOnBoard(PieceType type)
    {
        int count = 0;
        foreach (Cell c in cells)
            if (c.IsOccupied() && c.GetCurrentPiece().type == type)
                count++;
        return count;
    }
    private SelectablePiece FindAvailablePiece()
    {
        foreach (SelectablePiece p in GetMyPieces())
            if (p.currentCell == null)
                return p;
        return null;
    }
    private void PlacePiece(SelectablePiece piece, Cell target)
    {
        piece.transform.position = target.transform.position;
        piece.currentCell = target;
        target.PlacePiece(piece); // Use o método público
        piece.Deselect();
        gm.CheckWin(piece);
        gm.SwitchTurn();
    }
    private void MovePieceTo(SelectablePiece piece, Cell target)
    {
        if (piece.currentCell != null)
            piece.currentCell.ClearCell();
        piece.transform.position = target.transform.position;
        piece.currentCell = target;
        target.PlacePiece(piece); // Use o método público
        piece.Deselect();
        gm.CheckWin(piece);
        gm.SwitchTurn();
    }
}