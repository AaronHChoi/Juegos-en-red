using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetStats : MonoBehaviour
{
    public int Points = 10;
    public int RespawnTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit()
    {
        Debug.Log("hit");
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        Debug.Log("respawning");
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(RespawnTime);

        gameObject.SetActive(true);
    }
}
