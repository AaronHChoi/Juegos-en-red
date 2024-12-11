using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public int requiredPlayers = 2; // Number of players required to start the game
    public TMP_Text statusText;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public void Play()
    {
        PhotonNetwork.JoinRandomRoom();
        statusText.text = "Joining a random room...";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No room available, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)requiredPlayers });
        statusText.text = "Creating a new room...";
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Player {PhotonNetwork.NickName} joined the room.");

        // Master client syncs player names
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncPlayerNames", RpcTarget.AllBuffered);
        }
        statusText.text = "Waiting for other players...";
        CheckPlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} has joined the room.");
        CheckPlayers();
    }

    private void CheckPlayers()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == requiredPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }
}
