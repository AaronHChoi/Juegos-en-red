using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("hit");

        if (gameObject.CompareTag("ShotgunPowerup"))
        {
            // Special effect logic for ShotgunPowerup
            ActivateShotgunPowerup();
        }

        DisableTarget();
        targetManager.RespawnTarget(gameObject);
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

    private void ActivateShotgunPowerup()
    {
        Debug.Log("Shotgun power-up activated!");
        FindObjectOfType<PlayerMovement>().ActivateShotgunReticle();
    }


}