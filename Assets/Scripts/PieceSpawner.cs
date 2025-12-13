using UnityEngine;
public class PieceSpawner : MonoBehaviour
{
    public GameObject pieceXPrefab;
    public GameObject pieceOPrefab;
    public Transform topParent;
    public Transform bottomParent;
    public int piecesPerPlayer = 3;
    public void SpawnInitialPieces()
    {
        // Limpa antes de spawn
        foreach (Transform t in topParent) Destroy(t.gameObject);
        foreach (Transform t in bottomParent) Destroy(t.gameObject);
        for (int i = 0; i < piecesPerPlayer; i++)
        {
            GameObject x = Instantiate(pieceXPrefab, topParent);
            x.name = "PieceX_" + i;
            GameObject o = Instantiate(pieceOPrefab, bottomParent);
            o.name = "PieceO_" + i;
        }
        PieceManager.Instance.RefreshPieces();
    }
}