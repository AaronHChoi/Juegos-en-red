using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance; // Singleton instance
    public GameObject playerPrefab;
    public GameObject playerPrefab2;
    [Space]
    public Transform[] spawnPoints; // Array of spawn points for players

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Automatically sync scene for all players
        PhotonNetwork.AutomaticallySyncScene = true;

        // Validate prefabs and spawn points
        if (playerPrefab == null || playerPrefab2 == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerPrefabs or spawn points are not assigned in GameManager.");
            return;
        }

        // Spawn the local player
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Debug.Log("Spawning player...");

        // Determine the spawn point and prefab for this player
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];
        string prefabName = (playerIndex == 0) ? playerPrefab.name : playerPrefab2.name;

        // Instantiate the player prefab across the network
        GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", prefabName), spawnPoint.position, Quaternion.identity);

        Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} spawned at {spawnPoint.position}");

        // Check if the player object is instantiated correctly
        if (player == null)
        {
            Debug.LogError("Failed to instantiate player prefab.");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} has joined the room.");

        // Handle any additional logic when a player joins, such as syncing game state
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} has left the room.");

        // Load the main menu scene for the remaining player
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
}
