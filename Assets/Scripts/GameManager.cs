using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance; // Singleton instance
    public GameObject playerPrefab;
    [Space]
    public Transform spawnPoint;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Ensure Photon Network is ready
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon Network is not connected. Returning to main menu...");
            // Add logic to return to the main menu or handle disconnection
            return;
        }

        // Validate player prefab and spawn point
        if (playerPrefab == null || spawnPoint == null)
        {
            Debug.LogError("PlayerPrefab or SpawnPoint is not assigned in GameManager.");
            return;
        }

        // Spawn the local player
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Debug.Log("Spawning player...");

        GameObject _player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

        // Check if the player has the necessary setup components
        PlayerSetup playerSetup = _player.GetComponent<PlayerSetup>();
        if (playerSetup != null)
        {
            playerSetup.IsLocalPlayer(); // Setup local player
        }
        else
        {
            Debug.LogError("Player prefab is missing the PlayerSetup script!");
        }
    }
}
