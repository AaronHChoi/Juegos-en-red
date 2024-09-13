using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Vector3 mousePosition;
    public float movSpeed = 1f;
    Vector2 position = new Vector2(0f, 0f);

    private bool isCollidingWithTarget = false;

    private void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        MouseMovement();
        Shoot();
    }

    private void FixedUpdate()
    {
        //Movement
        transform.position = position;

    }

    private void MouseMovement()
    {
        //Movement
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        position = Vector2.Lerp(transform.position, mousePosition, movSpeed);
    }

    private void Shoot()
    {
        // Shooting
        if (Mouse.current.leftButton.wasPressedThisFrame && isCollidingWithTarget)
        {
            Debug.Log("Hit");
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && !isCollidingWithTarget)
        {
            Debug.Log("Miss");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Target"))
        {
            isCollidingWithTarget = true;
            Debug.Log("on");
        }
    }

    private void OnCollisionExit2D(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Target"))
        {
            isCollidingWithTarget = false;
            Debug.Log("off");
        }
    }



}
