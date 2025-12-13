using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TextMeshProUGUI statusText;

    void Start()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);

        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "za";
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "Conectando à região ZA...";
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Conectado! Entrando em sala...";
        PhotonNetwork.JoinRandomRoom(); // tenta entrar em uma sala existente
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Se não houver salas, cria uma
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        string roomName = "Room_" + Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomName, options);
        statusText.text = "Criando sala: " + roomName;
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Sala: " + PhotonNetwork.CurrentRoom.Name + " | Jogadores: " + PhotonNetwork.CurrentRoom.PlayerCount;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            PhotonNetwork.LoadLevel("GameOnline");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "Jogador entrou: " + newPlayer.NickName;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            PhotonNetwork.LoadLevel("GameOnline");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        statusText.text = otherPlayer.NickName + " saiu da sala";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = "Desconectado: " + cause;
    }
}
