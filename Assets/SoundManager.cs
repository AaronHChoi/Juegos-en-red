using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SoundManager : MonoBehaviourPunCallbacks
{
    public List<AudioClip> soundsList;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found on this GameObject.");
        }
    }

    // This method will be called by RPC to play a sound
    [PunRPC]
    public void PlaySoundRPC(string soundName)
    {
        AudioClip clip = soundsList.Find(s => s.name == soundName);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    // Public method to be called from other scripts
    public void PlaySound(string soundName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Only the Master Client can trigger the RPC
            photonView.RPC("PlaySoundRPC", RpcTarget.All, soundName);
        }
        else
        {
            // If not the Master Client, simply call the local method
            PlaySoundRPC(soundName);
        }
    }
}
