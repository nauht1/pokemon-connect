using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SyncPlayerName(int actorNumber, string name)
    {
        Debug.Log("SyncPlayerName");
        photonView.RPC("RPC_SyncPlayerName", RpcTarget.AllBuffered, actorNumber, name);
    }

    public void UpdateScore(int playerActorNumber, int newScore)
    {
        photonView.RPC("RPC_UpdateScore", RpcTarget.All, playerActorNumber, newScore);
    }

    [PunRPC]
    public void RPC_SyncPlayerName(int actorNumber, string name)
    {
        Debug.Log("RPC_SyncPlayerName");
        GameManager.Instance.OnPlayerNameSynced(actorNumber, name);
    }

    [PunRPC]
    public void RPC_UpdateScore(int playerActorNumber, int newScore)
    {
        GameManager.Instance.OnScoreUpdated(playerActorNumber, newScore);
    }
}