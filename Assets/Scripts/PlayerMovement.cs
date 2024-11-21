using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    private SoundManager soundManager;
    private SpriteRenderer reticleRenderer;

    public Sprite defaultCrosshair;
    public Sprite shotgunCrosshair;
    public float shotgunCrosshairDuration = 4f;
    public float shotgunColliderMultiplier = 4f;
    private bool isShotgunActive = false;
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
        if (!photonView.IsMine) return;

        MouseMovement();

        if (Input.GetKeyDown(KeyCode.Space) && !isReloading && bulletCount < bulletMax)
        {
            StartCoroutine(ReloadOneBullet());
        }

        Shoot();
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = position;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.fixedDeltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * 10);
        }
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
            photonView.RPC("PlayShotSound", RpcTarget.All, isShotgunActive);

            int pointsEarned = 0;

            if (collidingTargets.Count > 0)
            {
                foreach (var target in collidingTargets.ToArray())
                {
                    target.Hit(photonView.ViewID);
                    pointsEarned++;
                }
                points += pointsEarned;

                if (GameManager.instance != null)
                {
                    int playerIndex = GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer);
                    GameManager.instance.AddScore(playerIndex, pointsEarned);
                }
            }
            else
            {
                Debug.Log("Missed! No targets in range.");
            }

            bulletCount--;
            if (GameManager.instance != null)
            {
                int playerIndex = GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer);
                GameManager.instance.UpdateBulletCount(playerIndex, bulletCount);
            }

            Debug.Log($"Shot fired. Remaining bullets: {bulletCount}");
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Target") ||
            other.gameObject.CompareTag("ShotgunPowerup") ||
            other.gameObject.CompareTag("BulletPowerup") ||
            other.gameObject.CompareTag("DebuffPowerup") ||
            other.gameObject.CompareTag("GoldenTarget"))
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
            other.gameObject.CompareTag("BulletPowerup") ||
            other.gameObject.CompareTag("DebuffPowerup") ||
            other.gameObject.CompareTag("GoldenTarget"))
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

    private IEnumerator ReloadOneBullet()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        if (bulletCount < bulletMax)
        {
            bulletCount++;

            // Update UI via GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateBulletCount(GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer), bulletCount);
            }
        }

        isReloading = false;
    }

    public void ActivateShotgunReticle()
    {
        if (!photonView.IsMine) return;

        Debug.Log("Shotgun power-up activated for player: " + PhotonNetwork.LocalPlayer.NickName);

        isShotgunActive = true;
        reticleRenderer.sprite = shotgunCrosshair;

        playerCollider.radius = originalColliderRadius * shotgunColliderMultiplier;
        photonView.RPC("SyncShotgunActivation", RpcTarget.OthersBuffered, true);

        StartCoroutine(RevertToDefaultCrosshair());
    }

    [PunRPC]
    private void SyncShotgunActivation(bool active)
    {
        isShotgunActive = active;
        reticleRenderer.sprite = active ? shotgunCrosshair : defaultCrosshair;
        playerCollider.radius = active ? originalColliderRadius * shotgunColliderMultiplier : originalColliderRadius;
    }

    private IEnumerator RevertToDefaultCrosshair()
    {
        yield return new WaitForSeconds(shotgunCrosshairDuration);

        if (photonView.IsMine)
        {
            isShotgunActive = false;
            reticleRenderer.sprite = defaultCrosshair;
            playerCollider.radius = originalColliderRadius;

            photonView.RPC("SyncShotgunActivation", RpcTarget.OthersBuffered, false);
        }
    }

    public void ActivateTemporaryBulletPowerup()
    {
        if (!photonView.IsMine) return;

        Debug.Log($"Activating bullet power-up for player: {PhotonNetwork.LocalPlayer.NickName}");
        StartCoroutine(BulletPowerupCoroutine());
    }

    private IEnumerator BulletPowerupCoroutine()
    {
        float originalBulletMax = bulletMax;
        bulletMax = 10;
        bulletCount = bulletMax;

        photonView.RPC("SyncBulletPowerup", RpcTarget.OthersBuffered, bulletMax);

        if (GameManager.instance != null)
        {
            int playerIndex = GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer);
            GameManager.instance.UpdateBulletCount(playerIndex, bulletCount);
        }

        yield return new WaitForSeconds(10f);

        bulletMax = originalBulletMax; 
        if (bulletCount > bulletMax)
        {
            bulletCount = bulletMax;
        }

        photonView.RPC("SyncBulletPowerup", RpcTarget.AllBuffered, bulletMax);
    }

    [PunRPC]
    public void SyncBulletPowerup(float newBulletMax)
    {
        bulletMax = newBulletMax;

        if (bulletCount > bulletMax)
        {
            bulletCount = bulletMax;
        }

        Debug.Log($"SyncBulletPowerup: Updated bulletMax to {bulletMax}, current bullets: {bulletCount}");
    }

    [PunRPC]
    public void ApplyDebuff(int bulletsToLose)
    {
        bulletCount = Mathf.Max(0, bulletCount - bulletsToLose);

        Debug.Log($"Debuff applied! Remaining bullets: {bulletCount}");

        if (GameManager.instance != null)
        {
            int playerIndex = GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer);
            GameManager.instance.UpdateBulletCount(playerIndex, bulletCount);
        }
    }

    public void PlayerAddScore(int pointsToAdd)
    {
        points += pointsToAdd;

        if (GameManager.instance != null)
        {
            int playerIndex = GameManager.instance.GetPlayerIndex(PhotonNetwork.LocalPlayer);
            GameManager.instance.AddScore(playerIndex, pointsToAdd);
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
