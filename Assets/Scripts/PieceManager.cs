using UnityEngine;
public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;
    public SelectablePiece[] topPieces;
    public SelectablePiece[] bottomPieces;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    // Atualiza arrays e IDs após spawn das peças
    public void RefreshPieces()
    {
        Transform top = GameObject.Find("TopPieces").transform;
        Transform bottom = GameObject.Find("BottomPieces").transform;
        topPieces = top.GetComponentsInChildren<SelectablePiece>();
        bottomPieces = bottom.GetComponentsInChildren<SelectablePiece>();
        int id = 0;
        foreach (var p in topPieces) p.pieceId = id++;
        foreach (var p in bottomPieces) p.pieceId = id++;
    }
    public SelectablePiece GetPieceById(int id)
    {
        foreach (var p in topPieces)
            if (p.pieceId == id) return p;
        foreach (var p in bottomPieces)
            if (p.pieceId == id) return p;
        return null;
    }
}