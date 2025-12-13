using UnityEngine;
public class PieceSelector : MonoBehaviour
{
    public static PieceSelector Instance;
    [HideInInspector] public SelectablePiece selectedX;
    [HideInInspector] public SelectablePiece selectedO;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void SelectX(SelectablePiece piece)
    {
        ClearSelection();
        selectedX = piece;
        piece.Select();
    }
    public void SelectO(SelectablePiece piece)
    {
        ClearSelection();
        selectedO = piece;
        piece.Select();
    }
    public void ClearSelection()
    {
        if (selectedX != null) selectedX.Deselect();
        if (selectedO != null) selectedO.Deselect();
        selectedX = null;
        selectedO = null;
    }
}