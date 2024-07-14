using UnityEngine;

public class SpawnAroundPoint : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnRadius = 10f;
    public int numberOfSpawns = 100;

    void Start()
    {
        SpawnPrefabs();
    }

    void SpawnPrefabs()
    {
        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning("Prefab to spawn or spawn point is not set.");
            return;
        }

        for (int i = 0; i < numberOfSpawns; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        }
    }
}