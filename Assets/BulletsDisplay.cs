using UnityEngine;
using Photon.Pun;
using TMPro;

public class BulletDisplay : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text bulletText; // Assign this in the inspector
    public PlayerMovement playerMovement; // Reference to the PlayerMovement script
    private float currentBullets;
    private float maxBullets;

    void Start()
    {
        UpdateBulletText();
    }

    void Update()
    {
        if (currentBullets != playerMovement.bulletCount)
        {
            currentBullets = playerMovement.bulletCount;
            maxBullets = playerMovement.bulletMax;
            UpdateBulletText();
        }
    }

    void UpdateBulletText()
    {
        bulletText.text = "Bullets: " + Mathf.CeilToInt(currentBullets) + " / " + maxBullets;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the current bullet count to other players
            stream.SendNext(currentBullets);
        }
        else
        {
            // Receive the bullet count from the Master Client
            currentBullets = (float)stream.ReceiveNext();
            UpdateBulletText();
        }
    }
}
