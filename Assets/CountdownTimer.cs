using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class CountdownTimer : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text timerText;  // Assign this in the inspector
    private float countdownTime = 30f;
    private bool isCountingDown = false;

    void Start()
    {
        isCountingDown = true;     
    }

    void Update()
    {
        if (isCountingDown && countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        int displayTime = Mathf.CeilToInt(countdownTime);
        timerText.text = displayTime.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send countdown time to other players
            stream.SendNext(countdownTime);
        }
        else
        {
            // Receive countdown time from the master client
            countdownTime = (float)stream.ReceiveNext();
        }
    }
}
