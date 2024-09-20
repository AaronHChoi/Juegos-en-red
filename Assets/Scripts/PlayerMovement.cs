using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Vector3 mousePosition;
    public float movSpeed = 1f;
    Vector2 position = new Vector2(0f, 0f);

    private bool isCollidingWithTarget = false;
    private GameObject targetObject;

    public float bulletCount;
    public float bulletMax = 6;
    public float reloadTime = 0.5f;
    private bool isReloading = false;


    private void Start()
    {
        Cursor.visible = false;
        bulletCount = bulletMax;
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
                bulletCount--;
                Debug.Log("Hit. Current Bullets: " + bulletCount);
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
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
            Debug.Log("No bullets left!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            isCollidingWithTarget = true;
            targetObject = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            isCollidingWithTarget = false;
            targetObject = null;
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
}