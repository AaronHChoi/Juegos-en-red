using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public int requiredPlayers = 2; // Number of players required to start the game

    public void Play()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No room available, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)requiredPlayers });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
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
