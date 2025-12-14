using UnityEngine;
using Photon.Pun;

public class GameManagerOnline : MonoBehaviourPunCallbacks
{
    public static GameManagerOnline Instance;

    public PieceType myPiece = PieceType.X;
    private bool myTurn = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            myPiece = PieceType.X;
            myTurn = true;
        }
        else
        {
            myPiece = PieceType.O;
            myTurn = false;
        }

        SetupPieceControl();
    }

    public bool IsMyTurn() => myTurn;

    private void SetupPieceControl()
    {
        GameObject top = GameObject.Find("TopPieces");
        GameObject bottom = GameObject.Find("BottomPieces");

        if (top == null || bottom == null) return;

        bool master = PhotonNetwork.IsMasterClient;

        EnableGroup(top, master);
        EnableGroup(bottom, !master);
    }

    private void EnableGroup(GameObject obj, bool enable)
    {
        foreach (SelectablePiece p in obj.GetComponentsInChildren<SelectablePiece>())
        {
            CanvasGroup cg = p.GetComponent<CanvasGroup>();
            if (cg == null) cg = p.gameObject.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = enable;
            cg.interactable = enable;
            cg.alpha = enable ? 1f : 0.4f;
        }
    }

    // ---------------- RPCS -----------------

    [PunRPC]
    public void RPC_ShowWinner(int winnerInt)
    {
        PieceType winner = (PieceType)winnerInt;

        if (WinMenuUI.Instance != null)
            WinMenuUI.Instance.ShowWinner(winner);
    }

    public void SendMove(int pieceId, int cellIndex, bool isPlacement, PieceType type)
    {
        photonView.RPC(
            "RPC_ApplyMove",
            RpcTarget.All,
            pieceId, cellIndex, isPlacement, (int)type
        );
    }

    [PunRPC]
    private void RPC_ApplyMove(int pieceId, int cellIndex, bool isPlacement, int typeInt)
    {
        PieceType type = (PieceType)typeInt;

        FindObjectOfType<GameManager>().ApplyMove(pieceId, cellIndex, isPlacement, type, true);

        myTurn = (type != myPiece);

        FindObjectOfType<GameManager>().UpdateSelectablePieces();
    }

    public void SyncGameState()
    {
        myTurn = !myTurn;
        FindObjectOfType<GameManager>().UpdateSelectablePieces();
    }
}
