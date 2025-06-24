using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public struct PlayerPanel
    {
        public TextMeshProUGUI playerNameText;
        public Image iconStatus;
    }

    public PlayerPanel[] playerPanels;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI statusText;

    public Sprite iconReady;
    public Sprite iconNotReady;
    public Sprite iconWaiting;

    public Button readyButton;
    public Button startButton;
    public Button quitButton;

    private Dictionary<int, bool> playerReadyStates = new Dictionary<int, bool>();
    private Dictionary<int, int> playerPanelIndices = new Dictionary<int, int>();
    private bool isReady = false;

    private void Start()
    {

        roomNameText.text = "Room: " + GameManager.Instance.roomName;

        InitializePlayerPanels();
        UpdatePlayerList();

        if (PhotonNetwork.IsMasterClient)
        {
            isReady = true;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", true }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            readyButton.gameObject.SetActive(false); // Ẩn Ready cho Host
            startButton.gameObject.SetActive(true); // Hiện Start cho Host
        }
        else
        {
            readyButton.gameObject.SetActive(true); // Hiện Ready cho Guess
            startButton.gameObject.SetActive(false); // Ẩn Start cho Guess
        }

        UpdatePlayerReadyStatus();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Khởi tạo panel
    void InitializePlayerPanels()
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            playerPanels[i].playerNameText.text = "Waiting for player ...";
            playerPanels[i].iconStatus.sprite = iconWaiting;
        }
    }

    void UpdatePlayerList()
    {
        // Update player tương ứng với panel
        int index = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (index < playerPanels.Length)
            {
                // Gán actor number của photon vào playerPanelIndices để biết player nào tương ứng với panel nào
                playerPanelIndices[player.ActorNumber] = index;
                playerPanels[index].playerNameText.text = player.NickName;

                bool isReady = player.CustomProperties.ContainsKey("IsReady") && (bool)player.CustomProperties["IsReady"];
                playerPanels[index].iconStatus.sprite = isReady ? iconReady : iconNotReady;
                playerReadyStates[player.ActorNumber] = isReady;
                index++;
            }
        }
    }

    void UpdatePlayerReadyStatus()
    {
        int readyCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (playerReadyStates.ContainsKey(player.ActorNumber) && playerReadyStates[player.ActorNumber])
            {
                readyCount++;
            }
        }

        statusText.text = $"Waiting for players ready ({readyCount} / {PhotonNetwork.PlayerList.Length})";

        bool hasEnoughPlayer = PhotonNetwork.PlayerList.Length > 1;
        bool allPlayerReady = readyCount == PhotonNetwork.PlayerList.Length;

        bool canStart = hasEnoughPlayer && allPlayerReady;

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = canStart; // Cho phép start btn nếu tất cả đều ready.
        }
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerList();
        UpdatePlayerReadyStatus();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} has entered the room");
        UpdatePlayerList();
        UpdatePlayerReadyStatus();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room");

        if (playerPanelIndices.ContainsKey(otherPlayer.ActorNumber))
        {
            int index = playerPanelIndices[otherPlayer.ActorNumber];

            playerPanels[index].playerNameText.text = "Waiting for player ...";
            playerPanels[index].iconStatus.sprite = iconWaiting;

            playerPanelIndices.Remove(otherPlayer.ActorNumber);
            playerReadyStates.Remove(otherPlayer.ActorNumber);
        }
        UpdatePlayerList();
        UpdatePlayerReadyStatus();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (changedProps.ContainsKey("IsReady"))
        {
            playerReadyStates[targetPlayer.ActorNumber] = (bool)changedProps["IsReady"];
            UpdatePlayerList();
            UpdatePlayerReadyStatus();
        }
    }

    public void OnReadyBtnClicked()
    {
        isReady = !isReady;
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Cancel" : "Ready";

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "IsReady", isReady }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void OnStartBtnClicked()
    {
        PhotonNetwork.LoadLevel(4);
    }

    public void OnQuitBtnClicked()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadSceneAsync(1);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("You left the room");
    }
}
