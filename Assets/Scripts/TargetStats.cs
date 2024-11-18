using System.Collections;
using UnityEngine;

public class TargetStats : MonoBehaviour
{
    public int Points = 10;
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
        Debug.Log("Hit detected on target with tag: " + gameObject.tag);

        if (gameObject.CompareTag("ShotgunPowerup"))
        {
            ActivateShotgunPowerup();
        }
        else if (gameObject.CompareTag("BulletPowerup"))
        {
            ActivateBulletPowerup();
        }

        DisableTarget();
        targetManager.RespawnTarget(gameObject);
    }

    private void ActivateShotgunPowerup()
    {
        Debug.Log("Shotgun power-up activated!");
        FindObjectOfType<PlayerMovement>().ActivateShotgunReticle();
    }

    private void ActivateBulletPowerup()
    {
        Debug.Log("Bullet power-up activated!");
        FindObjectOfType<PlayerMovement>().ActivateTemporaryBulletPowerup();
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
