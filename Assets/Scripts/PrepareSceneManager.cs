using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq.Expressions;

public class PrepareSceneManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput;
    public TMP_InputField searchInput;
    public TextMeshProUGUI statusNetworkLabel;

    public Button createBtn;
    public Button joinBtn;

    private void Start()
    {
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        PhotonNetwork.JoinLobby();
        string status =PhotonNetwork.NetworkClientState.ToString();
        statusNetworkLabel.text = status;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
    }

    public void CreateRoom()
    {
        string playerName = playerNameInput.text;
        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(playerName) && string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Please tell us your PlayerName and your RoomName !");
        }

        GameManager.Instance.SetPlayerName(playerName);
        GameManager.Instance.SetRoomName(roomName);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
}
