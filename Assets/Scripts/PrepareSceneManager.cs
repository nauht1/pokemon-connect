using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq.Expressions;
using Unity.VisualScripting;

public class PrepareSceneManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput;
    public TMP_InputField avaiRoomInput;
    public TextMeshProUGUI statusNetworkLabel;

    public Button createBtn;
    public Button joinBtn;

    private void Start()
    {
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
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
            return;
        }

        PhotonNetwork.NickName = playerName;
        GameManager.Instance.SetPlayerName(playerName);
        GameManager.Instance.SetRoomName(roomName);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        Debug.Log("Created Rooom");
        //PhotonNetwork.LoadLevel(2);
    }

    public void OnJoinBtnClicked()
    {
        string playerName = playerNameInput.text;
        string roomName = avaiRoomInput.text;

        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(roomName))
        {
            statusNetworkLabel.text = "Please enter both Player Name and Room Name to join!";
            return;
        }

        PhotonNetwork.NickName = playerName;

        GameManager.Instance.SetPlayerName(playerName);
        GameManager.Instance.SetRoomName(roomName);

        PhotonNetwork.JoinRoom(roomName);
        statusNetworkLabel.text = $"Joining room '{roomName}'...";
        Debug.Log($"Joining room '{roomName}'...");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
    }
}
