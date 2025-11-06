using UnityEngine;

public class AsteroidSpawn : MonoBehaviour
{
    public System.Collections.Generic.List<Transform> spawnPoints = new();
    public GameObject asteroidObj;

    private void Awake()
    {
        StartCoroutine(SpawnAsteroids());
    }

    System.Collections.IEnumerator SpawnAsteroids()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
            var val = Random.Range(.5f, 1.5f);
            yield return new WaitForSeconds(val);
            var spawnPoint = Random.Range(0, spawnPoints.Count);
            Instantiate(asteroidObj,
                spawnPoints[spawnPoint].transform.position,
                Quaternion.identity);
        }
    }
}