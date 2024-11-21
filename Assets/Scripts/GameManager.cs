using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance; // Singleton instance
    public GameObject playerPrefab;
    public GameObject playerPrefab2;
    public Transform[] spawnPoints;
    [Space]
    public List<GameObject> targets; // List of all targets
    public float respawnTimeVariance = 1f; // Respawn variance
    public float respawnTimeVariance2 = 3f;
    public float shotgunRespawnTime = 15f; // Cooldown for Shotgun Powerup
    public float bulletPowerupRespawnTime = 15f; // Cooldown for Bullet Powerup
    public float debuffPowerupRespawnTime = 15f; // Cooldown for Debuff Powerup
    private Dictionary<GameObject, Vector3> targetPositions; // Original positions of targets
    [Space]
    public TMPro.TMP_Text scoreTextPlayer1; // Score display for Player 1
    public TMPro.TMP_Text scoreTextPlayer2; // Score display for Player 2
    public TMPro.TMP_Text timerText; // Timer display
    private int scorePlayer1 = 0; // Player 1 score
    private int scorePlayer2 = 0; // Player 2 score
    [Space]
    public TMPro.TMP_Text bulletCountPlayer1Text; // UI for Player 1's bullets
    public TMPro.TMP_Text bulletCountPlayer2Text; // UI for Player 2's bullets
    [Space]
    private float gameTimer = 30f; // Game timer
    private bool gameActive = true;
    [Space]
    public GameObject EndGamePanel;
    public TMPro.TMP_Text WinnerText;
    public TMPro.TMP_Text ScoresText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (playerPrefab == null || playerPrefab2 == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerPrefabs or spawn points are not assigned in GameManager.");
            return;
        }

        InitializeTargets();
        SpawnPlayer();
    }

    void Update()
    {
        if (!gameActive) return;

        UpdateTimer();

        if (gameTimer <= 0)
        {
            EndGame();
        }
    }

    private void UpdateTimer()
    {
        gameTimer -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(gameTimer).ToString();
    }

    public int GetPlayerIndex(Player player)
    {
        return PhotonNetwork.PlayerList.ToList().IndexOf(player);
    }

    public void AddScore(int playerIndex, int points)
    {
        photonView.RPC("UpdateScore", RpcTarget.All, playerIndex, points);
    }

    [PunRPC]
    private void UpdateScore(int playerIndex, int points)
    {
        if (playerIndex == 0)
        {
            scorePlayer1 += points;
            scoreTextPlayer1.text = $"Player 1: {scorePlayer1}";
        }
        else if (playerIndex == 1)
        {
            scorePlayer2 += points;
            scoreTextPlayer2.text = $"Player 2: {scorePlayer2}";
        }
    }

    public void UpdateBulletCount(int playerIndex, float bulletCount)
    {
        photonView.RPC("SyncBulletCount", RpcTarget.All, playerIndex, bulletCount);
    }

    [PunRPC]
    private void SyncBulletCount(int playerIndex, float bulletCount)
    {
        if (playerIndex == 0)
        {
            bulletCountPlayer1Text.text = $"Bullets: {bulletCount}";
        }
        else if (playerIndex == 1)
        {
            bulletCountPlayer2Text.text = $"Bullets: {bulletCount}";
        }
    }

    // Initialize targets and store original positions
    private void InitializeTargets()
    {
        targetPositions = new Dictionary<GameObject, Vector3>();

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                targetPositions[target] = target.transform.position;
                target.GetComponent<TargetStats>().EnableTarget();
                Debug.Log($"Target {target.name} initialized at {target.transform.position}");
            }
            else
            {
                Debug.LogError("Null target found in the targets list during initialization.");
            }
        }
    }


    // Spawn player logic
    private void SpawnPlayer()
    {
        int playerIndex = GetPlayerIndex(PhotonNetwork.LocalPlayer); // Use GetPlayerIndex
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];
        string prefabName = (playerIndex == 0) ? playerPrefab.name : playerPrefab2.name;

        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", prefabName), spawnPoint.position, Quaternion.identity);
    }

    // Respawn target logic
    public void RespawnTarget(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("RespawnTarget called with a null target.");
            return;
        }

        if (photonView == null)
        {
            Debug.LogError("PhotonView is missing in GameManager.");
            return;
        }

        photonView.RPC("HandleRespawn", RpcTarget.All, target.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void HandleRespawn(int targetViewID)
    {
        GameObject target = PhotonView.Find(targetViewID).gameObject;
        StartCoroutine(RespawnCoroutine(target));
    }

    private IEnumerator RespawnCoroutine(GameObject target)
    {
        float respawnTime =
            target.CompareTag("ShotgunPowerup") ? shotgunRespawnTime :
            target.CompareTag("BulletPowerup") ? bulletPowerupRespawnTime :
            target.CompareTag("DebuffPowerup") ? debuffPowerupRespawnTime :
            Random.Range(respawnTimeVariance, respawnTimeVariance2);

        yield return new WaitForSeconds(respawnTime);

        if (target != null && targetPositions.ContainsKey(target))
        {
            target.GetComponent<TargetStats>().EnableTarget();
            target.transform.position = targetPositions[target];
        }
        else
        {
            Debug.LogError("Target not found or not registered correctly.");
        }
    }


    // End game logic

    private void EndGame()
    {
        gameActive = false;
        Cursor.visible = true;

        string winner = scorePlayer1 > scorePlayer2 ? "Player 1 Wins!" : "Player 2 Wins!";
        photonView.RPC("DisplayEndGame", RpcTarget.All, winner, scorePlayer1, scorePlayer2);
    }

    [PunRPC]
    private void DisplayEndGame(string winner, int score1, int score2)
    {
        // Display the winner's name
        WinnerText.text = $"{winner}";

        // Display the final scores
        ScoresText.text = $"Player 1: {score1} | Player 2: {score2}";

        // Show the end-game panel
        EndGamePanel.SetActive(true);
    }

    public void ReturnToLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }
}
