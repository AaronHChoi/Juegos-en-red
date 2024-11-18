using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    private SoundManager soundManager;
    private SpriteRenderer reticleRenderer;

    public Sprite defaultCrosshair; // Default crosshair sprite
    public Sprite shotgunCrosshair; // Shotgun crosshair sprite
    public float shotgunCrosshairDuration = 4f; // Duration for shotgun crosshair
    public float shotgunColliderMultiplier = 4f; // Multiplier for shotgun collider size
    private bool isShotgunActive = false; // Tracks if shotgun power-up is active
    private CircleCollider2D playerCollider;
    private float originalColliderRadius;

    Vector3 mousePosition;
    public float movSpeed = 1f;
    Vector2 position = new Vector2(0f, 0f);

    private List<TargetStats> collidingTargets = new List<TargetStats>();

    public float bulletCount;
    public float bulletMax = 6;
    public float reloadTime = 0.5f;
    private bool isReloading = false;

    public int points;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private void Start()
    {
        Cursor.visible = false;
        bulletCount = bulletMax;

        reticleRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<CircleCollider2D>();
        soundManager = FindObjectOfType<SoundManager>();
        originalColliderRadius = playerCollider.radius;
    }

    void Update()
    {
        if (!photonView.IsMine) return; // Only local player controls its behavior

        MouseMovement();

        if (Input.GetKeyDown(KeyCode.Space) && !isReloading && bulletCount < bulletMax)
        {
            StartCoroutine(ReloadOneBullet());
        }

        Shoot();
    }

    private void MouseMovement()
    {
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        position = Vector2.Lerp(transform.position, mousePosition, movSpeed);
    }

    private void Shoot()
    {
        if (bulletCount > 0 && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Sync the shot sound across all players
            photonView.RPC("PlayShotSound", RpcTarget.All, isShotgunActive);

            if (collidingTargets.Count > 0)
            {
                foreach (var target in collidingTargets.ToArray())
                {
                    target.Hit(); // Apply the hit effect
                    points++;
                }
            }
            else
            {
                Debug.Log("Missed! No targets in range.");
            }

            bulletCount--;
            Debug.Log($"Shot fired. Remaining bullets: {bulletCount}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target") ||
            other.gameObject.CompareTag("ShotgunPowerup") ||
            other.gameObject.CompareTag("BulletPowerup"))
        {
            var target = other.GetComponent<TargetStats>();
            if (target != null && !collidingTargets.Contains(target))
            {
                collidingTargets.Add(target);
            }
        }
    }



    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target") ||
            other.gameObject.CompareTag("ShotgunPowerup") ||
            other.gameObject.CompareTag("BulletPowerup"))
        {
            var target = other.GetComponent<TargetStats>();
            if (target != null)
            {
                collidingTargets.Remove(target);
            }
        }
    }


    [PunRPC]
    private void PlayShotSound(bool shotgunActive)
    {
        soundManager.PlaySound(shotgunActive ? "ShotgunShotSound" : "ShotSound");
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = position;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.fixedDeltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * 10);
        }
    }

    private IEnumerator ReloadOneBullet()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        if (bulletCount < bulletMax)
        {
            bulletCount++;
        }

        isReloading = false;
    }

    public void ActivateShotgunReticle()
    {
        isShotgunActive = true;
        reticleRenderer.sprite = shotgunCrosshair;

        playerCollider.radius = shotgunColliderMultiplier;

        // Broadcast shotgun activation
        photonView.RPC("SyncShotgunActivation", RpcTarget.All, true);

        StartCoroutine(RevertToDefaultCrosshair());
    }

    [PunRPC]
    private void SyncShotgunActivation(bool active)
    {
        isShotgunActive = active;
        reticleRenderer.sprite = active ? shotgunCrosshair : defaultCrosshair;
        playerCollider.radius = active ? shotgunColliderMultiplier : originalColliderRadius;
    }

    private IEnumerator RevertToDefaultCrosshair()
    {
        yield return new WaitForSeconds(shotgunCrosshairDuration);

        photonView.RPC("SyncShotgunActivation", RpcTarget.All, false);
    }

    public void ActivateTemporaryBulletPowerup()
    {
        StartCoroutine(BulletPowerupCoroutine());
    }

    private IEnumerator BulletPowerupCoroutine()
    {
        float originalBulletMax = bulletMax;
        bulletMax = 10; // Increase to 10
        bulletCount = bulletMax; // Refill bullets

        photonView.RPC("SyncBulletPowerup", RpcTarget.All, bulletMax);

        Debug.Log($"Bullet power-up activated! Max bullets: {bulletMax}");

        yield return new WaitForSeconds(10f);

        bulletMax = originalBulletMax;
        photonView.RPC("SyncBulletPowerup", RpcTarget.All, bulletMax);

        Debug.Log($"Bullet power-up ended. Max bullets reverted to: {bulletMax}");

        if (bulletCount > bulletMax)
        {
            bulletCount = bulletMax;
        }
    }

    [PunRPC]
    private void SyncBulletPowerup(float newBulletMax)
    {
        bulletMax = newBulletMax;
        if (bulletCount > bulletMax)
        {
            bulletCount = bulletMax;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(bulletCount);
            stream.SendNext(isShotgunActive);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            bulletCount = (float)stream.ReceiveNext();
            isShotgunActive = (bool)stream.ReceiveNext();
        }
    }
}
