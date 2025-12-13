using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ======================
// ENUM ATUALIZADO
// ======================
public enum PieceType
{
    None = -1,
    X = 0,
    O = 1
}

// ======================
// SCRIPT SELECTABLE PIECE COMPLETO
// ======================
[RequireComponent(typeof(CanvasGroup))]
public class SelectablePiece : MonoBehaviour, IPointerClickHandler
{
    public int pieceId = -1;
    public PieceType type;
    public Image border;

    [HideInInspector] 
    public Cell currentCell = null;

    private bool isSelected = false;
    private float blinkSpeed = 2f;
    private float timer = 0f;

    private void Update()
    {
        if (isSelected && border != null)
        {
            timer += Time.deltaTime * blinkSpeed;
            float alpha = Mathf.Abs(Mathf.Sin(timer));
            border.color = new Color(border.color.r, border.color.g, border.color.b, alpha);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();

        // Não pode clicar se o item estiver invisível ou bloqueado
        if (cg != null && (cg.alpha < 0.9f || !cg.interactable || !cg.blocksRaycasts))
            return;

        // Não pode clicar se o jogo acabou
        if (FindObjectOfType<GameManager>()?.gameOver == true)
            return;

        var gm = FindObjectOfType<GameManager>();

        // Movimento inválido na fase errada
        if (currentCell != null && gm != null && !gm.IsMovementPhase(type))
            return;

        // ======================
        // Regras do MODO ONLINE
        // ======================
        if (GameMode.mode == "online" && GameManagerOnline.Instance != null)
        {
            // Só pode clicar se for minha peça
            if (GameManagerOnline.Instance.myPiece != type)
                return;

            // Só pode clicar se for meu turno
            if (!GameManagerOnline.Instance.IsMyTurn())
                return;
        }

        // Seleção da peça
        if (type == PieceType.X)
            PieceSelector.Instance.SelectX(this);
        else
            PieceSelector.Instance.SelectO(this);
    }

    public void Select()
    {
        isSelected = true;
        timer = 0f;
    }

    public void Deselect()
    {
        isSelected = false;

        if (border != null)
            border.color = new Color(border.color.r, border.color.g, border.color.b, 0f);
    }
}
