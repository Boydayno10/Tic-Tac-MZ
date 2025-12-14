using UnityEngine;
using UnityEngine.UI;

public class ProfileAvatarController : MonoBehaviour
{
    public GameObject avatarGrid;
    public Image avatarPreviewImage;

    public Sprite[] avatarSprites; // 12 sprites
    private int selectedAvatar = -1;

    void Start()
    {
        avatarGrid.SetActive(false); // começa fechado
    }

    // CLICAR NO QUADRADO BRANCO
    public void OpenAvatarGrid()
    {
        avatarGrid.SetActive(true);
    }

    // CLICAR EM UM AVATAR
    public void SelectAvatar(int index)
    {
        selectedAvatar = index;
        avatarPreviewImage.sprite = avatarSprites[index];

        avatarGrid.SetActive(false); // fecha grid
        Debug.Log("Avatar escolhido: " + index);
    }

    public int GetSelectedAvatar()
    {
        return selectedAvatar;
    }
}
