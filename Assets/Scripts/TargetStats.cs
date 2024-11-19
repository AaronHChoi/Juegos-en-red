using UnityEngine;
using Photon.Pun;

public class TargetStats : MonoBehaviourPunCallbacks
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private TargetManager targetManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        targetManager = FindObjectOfType<TargetManager>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing on " + gameObject.name);
        }
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D component is missing on " + gameObject.name);
        }
        if (targetManager == null)
        {
            Debug.LogError("TargetManager is not found in the scene.");
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
            FindObjectOfType<PlayerMovement>().ActivateShotgunReticle();
        }
        else if (gameObject.CompareTag("BulletPowerup"))
        {
            FindObjectOfType<PlayerMovement>().ActivateTemporaryBulletPowerup();
        }

        DisableTarget();
        targetManager.RespawnTarget(gameObject); // Respawn the target
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
