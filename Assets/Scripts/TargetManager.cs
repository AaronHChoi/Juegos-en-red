using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviourPunCallbacks
{
    public List<GameObject> targets;
    private Dictionary<GameObject, Vector3> targetPositions;

    public float respawnTimeVariance = 1f; // Regular target respawn variance
    public float respawnTimeVariance2 = 3f;
    public float shotgunRespawnTime = 15f; // Cooldown for Shotgun Powerup
    public float bulletPowerupRespawnTime = 15f; // Cooldown for Bullet Powerup

    void Start()
    {
        targetPositions = new Dictionary<GameObject, Vector3>();

        foreach (GameObject target in targets)
        {
            targetPositions[target] = target.transform.position;
            target.GetComponent<TargetStats>().EnableTarget();
        }
    }

    public void RespawnTarget(GameObject target)
    {
        photonView.RPC("HandleRespawn", RpcTarget.All, target.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void HandleRespawn(int targetViewID)
    {
        GameObject target = PhotonView.Find(targetViewID).gameObject;
        StartCoroutine(RespawnCoroutine(target));
    }

    private IEnumerator RespawnCoroutine(GameObject target)
    {
        // Determine respawn time based on the tag
        float respawnTime;
        if (target.CompareTag("ShotgunPowerup"))
        {
            respawnTime = shotgunRespawnTime;
        }
        else if (target.CompareTag("BulletPowerup"))
        {
            respawnTime = bulletPowerupRespawnTime;
        }
        else
        {
            respawnTime = Random.Range(respawnTimeVariance, respawnTimeVariance2);
        }

        // Wait for the determined respawn time
        yield return new WaitForSeconds(respawnTime);

        // Enable the target and reset its position
        target.GetComponent<TargetStats>().EnableTarget();
        target.transform.position = targetPositions[target];
    }
}
