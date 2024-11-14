using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public List<GameObject> targets;
    private Dictionary<GameObject, Vector3> targetPositions;
    public float respawnTimeVariance = 1f; // For regular targets
    public float respawnTimeVariance2 = 3f; // For regular targets
    public float shotgunRespawnTime = 15f; // Fixed respawn time for Shotgun targets

    void Start()
    {
        targetPositions = new Dictionary<GameObject, Vector3>();

        // Initialize the target positions and ensure all targets are added to the dictionary
        foreach (GameObject target in targets)
        {
            if (!targetPositions.ContainsKey(target))
            {
                targetPositions[target] = target.transform.position;
            }

            // Enable all targets initially (including shotgun)
            TargetStats targetStats = target.GetComponent<TargetStats>();
            if (targetStats != null)
            {
                targetStats.EnableTarget();
            }
            else
            {
                Debug.LogError("TargetStats component is missing on " + target.name);
            }
        }
    }

    public void RespawnTarget(GameObject target)
    {
        StartCoroutine(RespawnCoroutine(target));
    }

    private IEnumerator RespawnCoroutine(GameObject target)
    {
        // Determine the respawn time
        float respawnTime;
        if (target.CompareTag("ShotgunPowerup"))
        {
            respawnTime = shotgunRespawnTime; // Fixed 15-second respawn time for shotgun targets
        }
        else
        {
            respawnTime = Random.Range(respawnTimeVariance, respawnTimeVariance2); // Random respawn time for regular targets
        }

        // Wait for the respawn time
        yield return new WaitForSeconds(respawnTime);

        // Check if the target is in the dictionary before trying to access its position
        if (targetPositions.ContainsKey(target))
        {
            TargetStats targetStats = target.GetComponent<TargetStats>();
            if (targetStats != null)
            {
                targetStats.EnableTarget(); // Make sure the target is enabled
                target.transform.position = targetPositions[target]; // Respawn the target at the saved position
            }
            else
            {
                Debug.LogError("TargetStats component is missing on " + target.name);
            }
        }
        else
        {
            Debug.LogWarning("Target is not in targetPositions dictionary.");
        }
    }
}
