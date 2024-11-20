using UnityEngine;
using Photon.Pun;

public class TargetStats : MonoBehaviourPunCallbacks
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing on " + gameObject.name);
        }
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D component is missing on " + gameObject.name);
        }
    }

    public void Hit()
    {
        photonView.RPC("HandleHit", RpcTarget.All); // Sync hit across all players
    }

    [PunRPC]
    private void HandleHit()
    {
        Debug.Log("Hit detected on target with tag: " + gameObject.tag);

        if (gameObject.CompareTag("ShotgunPowerup"))
        {
            FindObjectOfType<PlayerMovement>()?.ActivateShotgunReticle();
        }
        else if (gameObject.CompareTag("BulletPowerup"))
        {
            FindObjectOfType<PlayerMovement>()?.ActivateTemporaryBulletPowerup();
        }

        DisableTarget();

        // Safely call RespawnTarget
        if (GameManager.instance != null)
        {
            GameManager.instance.RespawnTarget(gameObject);
        }
        else
        {
            Debug.LogError("GameManager instance is null in HandleHit.");
        }
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
