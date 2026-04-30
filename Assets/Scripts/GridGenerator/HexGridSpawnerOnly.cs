using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexGridSpawnerOnly : MonoBehaviour
{
    public SO_AllTiles allTilePrefabs;
    public int radiusMap = 10;

    public float width = 34.5f;
    public float height = 30f;

    // Optionnel : garder une ref si tu veux profiler / debug
    public Dictionary<Vector3, GameObject> grid = new Dictionary<Vector3, GameObject>();

    private static readonly Vector3[] directions =
    {
        new Vector3(0, -1, 1), new Vector3(1, -1, 0), new Vector3(1, 0, -1),
        new Vector3(0, 1, -1), new Vector3(-1, 1, 0), new Vector3(-1, 0, 1)
    };

    private List<GameObject> allTiles = new List<GameObject>();

    void Start()
    {
        CacheTiles();
        GenerateGrid();
    }

    void CacheTiles()
    {
        allTiles.Clear();

        foreach (var type in allTilePrefabs.ceil)
        {
            foreach (var prefab in type.ceil)
            {
                if (prefab != null)
                    allTiles.Add(prefab);
            }
        }

        if (allTiles.Count == 0)
        {
            Debug.LogError("No tiles found");
        }
    }

    void GenerateGrid()
    {
        grid.Clear();

        int radius = Mathf.CeilToInt(radiusMap);

        for (int q = -radius; q <= radius; q++)
        {
            for (int r = -radius; r <= radius; r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) > radius) continue;

                Vector3 hex = new Vector3(q, r, s);
                Vector3 pos = HexToWorld(hex);

                GameObject prefab = allTiles[Random.Range(0, allTiles.Count)];

                Quaternion rot = Quaternion.Euler(0, Random.Range(0, 6) * 60f, 0);

                GameObject tile = Instantiate(prefab, pos, rot, transform);

                grid.Add(hex, tile);
            }
        }

        Debug.Log($"Spawn terminé : {grid.Count} tiles");
    }

    Vector3 HexToWorld(Vector3 hex)
    {
        float x = (hex.x + hex.z * 0.5f) * width;
        float z = hex.z * height;
        return new Vector3(x, 0, z);
    }
}