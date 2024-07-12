using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class GridScriptManual : MonoBehaviour
{
    public float height = 30; // Hauteur de l'hexagone
    public float width = 34.5f;  // Largeur de l'hexagone
    public float radiusMap = 5; // Rayon de la carte hexagonale
    public List<Vector3> globalPos = new List<Vector3>();
    public List<GameObject> prefab = new List<GameObject>();
    public Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();

    public Dictionary<Vector3, Vector3> worldToHexMap = new Dictionary<Vector3, Vector3>(); // Mapping position mondiale -> coordonnées hexagonales

    public SO_AllTiles allTilePrefabs; // Liste de tous les prefabs dans des SO_AllTiles.ceil de tuiles disponibles
    public List<GameObject> AllTilesGO = new List<GameObject>();
    public Dictionary<Vector3, GameObject> hexCoordCollapsedTile = new Dictionary<Vector3, GameObject>();

    private List<Vector3> directions = new List<Vector3>()
    {
        new Vector3(1, -1, 0), new Vector3(1, 0, -1), new Vector3(0, 1, -1),
        new Vector3(-1, 1, 0), new Vector3(-1, 0, 1), new Vector3(0, -1, 1)
    };

    void Start()
    {
        foreach (var So_type in allTilePrefabs.ceil)
        {
            foreach (var Go in So_type.ceil)
            {
                if (Go != null)
                {
                    AllTilesGO.Add(Go);
                }
            }
        }
        // Nettoyage des enfants avant le début
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        globalPos.Clear();
        prefab.Clear();
        hexGrid.Clear();
        StartCoroutine(GenerateTilesWithDelay());
    }

    private IEnumerator GenerateTilesWithDelay()
    {
        for (int q = -Mathf.CeilToInt(radiusMap); q <= Mathf.CeilToInt(radiusMap); q++)
        {
            for (int r = -Mathf.CeilToInt(radiusMap); r <= Mathf.CeilToInt(radiusMap); r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= radiusMap)
                {
                    Vector3 hexCoord = new Vector3(q, r, s);
                    Vector3 worldPosition = HexToWorldPosition(hexCoord);

                    if (!hexGrid.ContainsKey(hexCoord))
                    {
                        int randomTileTypeIndex = Random.Range(0, allTilePrefabs.ceil.Length);
                        SO_TileType tileType = allTilePrefabs.ceil[randomTileTypeIndex];
                        List<GameObject> possibleTilePrefabs = new List<GameObject>(tileType.ceil);

                        if (possibleTilePrefabs.Count == 0)
                        {
                            Debug.LogError("La liste des prefabs possibles est vide.");
                            continue;
                        }

                        GameObject randomTilePrefab = possibleTilePrefabs[Random.Range(0, possibleTilePrefabs.Count)];
                        Debug.Log($"Génération de la tuile {randomTilePrefab.name} à la position {hexCoord}.");
                        GameObject currentTile = Instantiate(randomTilePrefab, worldPosition, Quaternion.identity, transform);
                        BaseTile baseTile = currentTile.GetComponent<BaseTile>();
                        baseTile.Initialize(hexCoord, AllTilesGO);

                        CeilClass ceilClass = new CeilClass(hexCoord);
                        baseTile.ceilClass = ceilClass;

                        hexGrid.Add(hexCoord, currentTile);
                        globalPos.Add(hexCoord);
                        prefab.Add(currentTile);
                        worldToHexMap.Add(worldPosition, hexCoord);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        //StartCoroutine(CollapseTiles());
    }

    private IEnumerator CollapseTiles()
    {
        // Effondre la tuile initiale en (0, 0, 0)
        Vector3 initialHexCoord = new Vector3(0, 0, 0);
        if (hexGrid.ContainsKey(initialHexCoord))
        {
            CollapseTile(initialHexCoord);
        }
        
        while (true)
        {
            Vector3 targetHexCoord = FindTileWithMostCollapsedNeighbors();
            if (targetHexCoord == Vector3.zero)
            {
                break;
            }

            if (!hexCoordCollapsedTile.ContainsKey(targetHexCoord))
            {
                CollapseTile(targetHexCoord);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void CollapseTile(Vector3 hexCoord)
    {
        if (!hexGrid.ContainsKey(hexCoord)) return;

        BaseTile baseTile = hexGrid[hexCoord].GetComponent<BaseTile>();
        baseTile.ceilClass.isCollapsed = true;
        hexCoordCollapsedTile[hexCoord] = hexGrid[hexCoord];

        for (int i = 0; i < directions.Count; i++)
        {
            Vector3 neighborCoord = hexCoord + directions[i];
            if (hexGrid.ContainsKey(neighborCoord) && !hexGrid[neighborCoord].GetComponent<BaseTile>().ceilClass.isCollapsed)
            {
                CheckAndMatchBorders(hexCoord, neighborCoord, i);
            }
        }
    }

    private void CheckAndMatchBorders(Vector3 baseHexCoord, Vector3 neighborCoord, int borderIndex)
    {
        BaseTile baseTile = hexGrid[baseHexCoord].GetComponent<BaseTile>();
        BaseTile neighborTile = hexGrid.ContainsKey(neighborCoord) ? hexGrid[neighborCoord].GetComponent<BaseTile>() : null;

        int neighborBorderType = neighborTile != null ? neighborTile.cellType.borders[(borderIndex + 3) % 6] : 0;
        Debug.Log($"Vérification des bordures : baseTile ({baseHexCoord}) border[{borderIndex}] = {baseTile.cellType.borders[borderIndex]}, neighborTile ({neighborCoord}) border[{(borderIndex + 3) % 6}] = {neighborBorderType}");

        if (baseTile.cellType.borders[borderIndex] != neighborBorderType)
        {
            ReplaceTile(neighborCoord, baseTile.cellType.borders[(borderIndex + 3) % 6]);
        }
    }

    private Vector3 FindTileWithMostCollapsedNeighbors()
    {
        int maxCollapsedNeighbors = -1;
        Vector3 targetHexCoord = Vector3.zero;

        foreach (var hexCoord in globalPos)
        {
            if (hexCoordCollapsedTile.ContainsKey(hexCoord)) continue;

            int collapsedNeighborCount = 0;
            for (int i = 0; i < directions.Count; i++)
            {
                Vector3 neighborCoord = hexCoord + directions[i];
                if (hexCoordCollapsedTile.ContainsKey(neighborCoord))
                {
                    collapsedNeighborCount++;
                }
            }

            if (collapsedNeighborCount > maxCollapsedNeighbors)
            {
                maxCollapsedNeighbors = collapsedNeighborCount;
                targetHexCoord = hexCoord;
            }
        }

        return targetHexCoord;
    }

    public void ReplaceTile(Vector3 hexCoord, int requiredBorderType)
    {
        if (!hexGrid.ContainsKey(hexCoord)) return;

        Vector3 worldPosition = HexToWorldPosition(hexCoord);

        foreach (var tilePrefab in AllTilesGO)
        {
            BaseTile baseTile = tilePrefab.GetComponent<BaseTile>();
            for (int rotation = 0; rotation < 6; rotation++)
            {
                if (baseTile.cellType.borders[(0 + rotation) % 6] == requiredBorderType)
                {
                    Debug.Log($"Remplacement de la tuile {hexGrid[hexCoord].name} par {tilePrefab.name} avec une rotation de {rotation * 60} degrés.");
                    Destroy(hexGrid[hexCoord]);
                    GameObject newTile = Instantiate(tilePrefab, worldPosition, Quaternion.Euler(0, rotation * 60, 0), transform);
                    hexGrid[hexCoord] = newTile;
                    BaseTile newBaseTile = newTile.GetComponent<BaseTile>();
                    newBaseTile.Initialize(hexCoord, AllTilesGO);
                    newBaseTile.ceilClass.isCollapsed = true;
                    hexCoordCollapsedTile[hexCoord] = newTile;
                    return;
                }
            }
        }
    }

    public Vector3 HexToWorldPosition(Vector3 hexCoord)
    {
        float x = width * (hexCoord.x + hexCoord.z / 2.0f);
        float z = height * hexCoord.z;
        return new Vector3(x, 0, z);
    }
}
