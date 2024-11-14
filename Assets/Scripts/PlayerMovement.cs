using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private SpriteRenderer reticleRenderer;

    public Sprite defaultCrosshair;                         // Default crosshair sprite 
    public Sprite shotgunCrosshair;                         // Shotgun crosshair sprite 
    public float shotgunCrosshairDuration = 4f;             // Duration to show shotgun crosshair



    public float shotgunColliderMultiplier = 4f; // Multiplier for shotgun collider size
    private CircleCollider2D playerCollider; // Reference to the CircleCollider2D
    private float originalColliderRadius; // Store the original radius to reset later



    Vector3 mousePosition;
    public float movSpeed = 1f;
    Vector2 position = new Vector2(0f, 0f);

    private bool isCollidingWithTarget = false;
    private TargetStats targetStats;

    public float bulletCount;
    public float bulletMax = 6;
    public float reloadTime = 0.5f;
    private bool isReloading = false;

    public int points;


    private void Start()
    {
        Cursor.visible = false;
        bulletCount = bulletMax;

        reticleRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<CircleCollider2D>(); // Get the CircleCollider2D component
        originalColliderRadius = playerCollider.radius; // Store the original radius
    }



    void Update()
    {
        MouseMovement();
        
        if (Input.GetKeyDown(KeyCode.Space) && !isReloading && bulletCount < bulletMax)
        {
            StartCoroutine(ReloadOneBullet());
        }

        Shoot();
    }

    private void FixedUpdate()
    {
        // Movement
        transform.position = position;
    }

    private void MouseMovement()
    {
        // Movement
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        position = Vector2.Lerp(transform.position, mousePosition, movSpeed);
    }

    private void Shoot()
    {
        // limit shooting
        if (bulletCount > 0)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && isCollidingWithTarget)
            {
                points++;
                bulletCount--;
                Debug.Log("Hit. Current Bullets: " + bulletCount);
                if (targetStats != null)
                {
                    targetStats.Hit();
                }
            }
            else if (Mouse.current.leftButton.wasPressedThisFrame && !isCollidingWithTarget)
            {
                bulletCount--;
                Debug.Log("Miss. Current Bullets: " + bulletCount);
            }
        }
        else
        {
            //Debug.Log("No bullets left!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target") || other.gameObject.CompareTag("ShotgunPowerup"))
        {
            isCollidingWithTarget = true;
            targetStats = other.GetComponent<TargetStats>();
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            isCollidingWithTarget = false;
            targetStats = null;
        }
    }

    private IEnumerator ReloadOneBullet()
    {
        isReloading = true;
        Debug.Log("Reloading one bullet...");

        yield return new WaitForSeconds(reloadTime);

        if (bulletCount < bulletMax)
        {
            bulletCount++;
        }

        isReloading = false;
    }

    public void ActivateShotgunReticle()
    {
        // Change the reticle to the shotgun crosshair
        reticleRenderer.sprite = shotgunCrosshair;

        // Scale up the collider radius for shotgun power-up
        playerCollider.radius = 3;

        // Start coroutine to revert back to default settings after the duration
        StartCoroutine(RevertToDefaultCrosshair());
    }



    private IEnumerator RevertToDefaultCrosshair()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(shotgunCrosshairDuration);

        // Revert to the default crosshair
        reticleRenderer.sprite = defaultCrosshair;

        // Reset collider radius back to the original
        playerCollider.radius = originalColliderRadius;
    }
}