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
        // 1. Tenta vencer imediatamente
        Cell win = FindTwoInLine(aiType); // 2 minhas + 1 vazio = vitória
        if (win != null)
        {
            ExecuteMove(win);
            isMakingMove = false;
            return;
        }
        // 2. Bloqueia vitória imediata do adversário
        PieceType opponent = aiType == PieceType.X ? PieceType.O : PieceType.X;
        Cell block = FindTwoInLine(opponent);
        if (block != null)
        {
            ExecuteMove(block);
            isMakingMove = false;
            return;
        }
        // 3. Cria duas ameaças ao mesmo tempo (força o jogador a perder no próximo turno)
        Cell fork = FindForkOpportunity();
        if (fork != null)
        {
            ExecuteMove(fork);
            isMakingMove = false;
            return;
        }
        // 4. Bloqueia fork do adversário
        Cell blockFork = FindOpponentFork();
        if (blockFork != null)
        {
            ExecuteMove(blockFork);
            isMakingMove = false;
            return;
        }
        // 5. Estratégia posicional perfeita (centro > cantos > lados)
        Cell strategic = GetBestStrategicCell();
        if (strategic != null)
        {
            ExecuteMove(strategic);
        }
        isMakingMove = false;
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