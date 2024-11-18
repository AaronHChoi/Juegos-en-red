using UnityEngine;
using Photon.Pun;
using TMPro;

public class ScoreDisplay : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text player1ScoreText; // Assign in the inspector (left side of the screen)
    public TMP_Text player2ScoreText; // Assign in the inspector (right side of the screen)
    private int player1Score;
    private int player2Score;

    void Start()
    {
        UpdateScoreText();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                player1Score = GetLocalPlayerScore();
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                player2Score = GetLocalPlayerScore();
            }

            UpdateScoreText();
        }
    }

    void UpdateScoreText()
    {
        player1ScoreText.text = "Player 1: " + player1Score;
        player2ScoreText.text = "Player 2: " + player2Score;
    }

    public int GetLocalPlayerScore()
    {
        // Replace this with your logic to retrieve the score
        return FindObjectOfType<PlayerMovement>().points;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                stream.SendNext(player1Score);
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                stream.SendNext(player2Score);
            }
        }
        else
        {
            player1Score = (int)stream.ReceiveNext();
            player2Score = (int)stream.ReceiveNext();
        }
    }
}
