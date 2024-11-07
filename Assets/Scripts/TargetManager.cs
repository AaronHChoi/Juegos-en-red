using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public List<GameObject> targets;
    private Dictionary<GameObject, Vector3> targetPositions;
    public float respawnTimeVariance = 1f;
    public float respawnTimeVariance2 = 3f;
    void Start()
    {
        targetPositions = new Dictionary<GameObject, Vector3>();

        foreach (GameObject target in targets)
        {
            targetPositions[target] = target.transform.position;
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
        float respawnTime = Random.Range(respawnTimeVariance, respawnTimeVariance2);
        yield return new WaitForSeconds(respawnTime);

        TargetStats targetStats = target.GetComponent<TargetStats>();
        if (targetStats != null)
        {
            targetStats.EnableTarget();
            target.transform.position = targetPositions[target];
        }
        else
        {
            Debug.LogError("TargetStats component is missing on " + target.name);
        }
    }
}
