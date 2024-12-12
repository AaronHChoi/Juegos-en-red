using UnityEngine;
using Photon.Pun;

public class TargetStats : MonoBehaviourPunCallbacks
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private SoundManager soundManager;

    private bool isHit = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        soundManager = FindObjectOfType<SoundManager>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing on " + gameObject.name);
        }
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D component is missing on " + gameObject.name);
        }
    }

    public void Hit(int playerViewID)
    {
        if (isHit) return;
        isHit = true;

        photonView.RPC("HandleHit", RpcTarget.All, playerViewID);
    }

    [PunRPC]
    private void HandleHit(int playerViewID)
    {
        Debug.Log("Hit detected on target with tag: " + gameObject.tag);

        PlayerMovement player = PhotonView.Find(playerViewID)?.GetComponent<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError($"PlayerMovement not found for PhotonView ID: {playerViewID}");
            return;
        }

        if (gameObject.CompareTag("ShotgunPowerup"))
        {
            player.ActivateShotgunReticle();
        }
        else if (gameObject.CompareTag("BulletPowerup"))
        {
            player.ActivateTemporaryBulletPowerup();
        }
        else if (gameObject.CompareTag("DebuffPowerup"))
        {
            PlayerMovement otherPlayer = FindOtherPlayer(playerViewID);
            if (otherPlayer != null)
            {
                otherPlayer.photonView.RPC("ApplyDebuff", RpcTarget.AllBuffered, 3);
                Debug.Log("Debuff Powerup hit! Other player loses 3 bullets.");
            }
        }
        else if (gameObject.CompareTag("GoldenTarget"))
        {
            player.PlayerAddScore(2);
            soundManager.PlaySound("Money");
            Debug.Log("Golden Target hit! 3 points awarded.");
        }


        DisableTarget();

        if (GameManager.instance != null)
        {
            GameManager.instance.RespawnTarget(gameObject);
        }
    }

    private PlayerMovement FindOtherPlayer(int shooterViewID)
    {
        PhotonView[] photonViews = Object.FindObjectsOfType<PhotonView>();

        foreach (PhotonView view in photonViews)
        {
            if (view.ViewID != shooterViewID && view.GetComponent<PlayerMovement>() != null)
            {
                return view.GetComponent<PlayerMovement>();
            }
        }
        return null;
    }





    private void DisableTarget()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    public void EnableTarget()
    {
        isHit = false; // Reset the hit flag
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
}
