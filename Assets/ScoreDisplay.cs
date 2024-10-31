using UnityEngine;
using Photon.Pun;
using TMPro;

public class ScoreDisplay : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text scoreText;  // Assign this in the inspector
    public PlayerMovement playerMovement; // Reference to the PlayerMovement script
    private int score;

    void Start()
    {
        UpdateScoreText();
    }

    void Update()
    {
        if (score != playerMovement.points)
        {
            score = playerMovement.points;
            UpdateScoreText();
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the score to other players
            stream.SendNext(score);
        }
        else
        {
            // Receive the score from the Master Client
            score = (int)stream.ReceiveNext();
            UpdateScoreText();
        }
    }
}
