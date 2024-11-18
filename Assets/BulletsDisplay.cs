using UnityEngine;
using Photon.Pun;
using TMPro;

public class BulletDisplay : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text player1BulletText; // Assign in the inspector (left side of the screen)
    public TMP_Text player2BulletText; // Assign in the inspector (right side of the screen)
    private float player1CurrentBullets;
    private float player2CurrentBullets;
    private float player1MaxBullets;
    private float player2MaxBullets;

    void Start()
    {
        UpdateBulletText();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                player1CurrentBullets = GetLocalPlayerCurrentBullets();
                player1MaxBullets = GetLocalPlayerMaxBullets();
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                player2CurrentBullets = GetLocalPlayerCurrentBullets();
                player2MaxBullets = GetLocalPlayerMaxBullets();
            }

            UpdateBulletText();
        }
    }

    void UpdateBulletText()
    {
        player1BulletText.text = "P1 Bullets: " + Mathf.CeilToInt(player1CurrentBullets) + " / " + Mathf.CeilToInt(player1MaxBullets);
        player2BulletText.text = "P2 Bullets: " + Mathf.CeilToInt(player2CurrentBullets) + " / " + Mathf.CeilToInt(player2MaxBullets);
    }

    public float GetLocalPlayerCurrentBullets()
    {
        return FindObjectOfType<PlayerMovement>().bulletCount;
    }

    public float GetLocalPlayerMaxBullets()
    {
        return FindObjectOfType<PlayerMovement>().bulletMax;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                stream.SendNext(player1CurrentBullets);
                stream.SendNext(player1MaxBullets);
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                stream.SendNext(player2CurrentBullets);
                stream.SendNext(player2MaxBullets);
            }
        }
        else
        {
            player1CurrentBullets = (float)stream.ReceiveNext();
            player1MaxBullets = (float)stream.ReceiveNext();
            player2CurrentBullets = (float)stream.ReceiveNext();
            player2MaxBullets = (float)stream.ReceiveNext();
            UpdateBulletText();
        }
    }
}
