using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWaveFonct : MonoBehaviour
{
    public float height = 60; // Hauteur de l'hexagone
    public float width = 69;  // Largeur de l'hexagone
    public float radiusMap = 5; // Rayon de la carte hexagonale
    public List<Vector3> globalPos = new List<Vector3>();
    public List<GameObject> prefab = new List<GameObject>();
    public Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();

    public Dictionary<Vector3, Vector3> worldToHexMap = new Dictionary<Vector3, Vector3>(); // Mapping position mondiale -> coordonnées hexagonales

    public SO_AllTiles allTilePrefabs; // Liste de tous les prefabs de tuiles disponibles
    public List<GameObject> AllTilesGO = new List<GameObject>();
    void Start()
    {
        foreach (var So_type in allTilePrefabs.ceil)
        {
            foreach (var Go in So_type.ceil)
            {
                if (Go !=null)
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
                        // Choisissez un prefab aléatoire parmi les options disponibles
                        int randomTileTypeIndex = Random.Range(0, allTilePrefabs.ceil.Length);
                        SO_TileType tileType = allTilePrefabs.ceil[randomTileTypeIndex];
                        List<GameObject> possibleTilePrefabs = new List<GameObject>(tileType.ceil);

                        // Assurez-vous que la liste des tiles possibles n'est pas vide
                        if (possibleTilePrefabs.Count == 0)
                        {
                            Debug.LogError("La liste des prefabs possibles est vide.");
                            continue;
                        }

                        GameObject randomTilePrefab = possibleTilePrefabs[Random.Range(0, possibleTilePrefabs.Count)];
                        GameObject currentTile = Instantiate(randomTilePrefab, worldPosition, Quaternion.identity, transform);
                        BaseTile baseTile = currentTile.GetComponent<BaseTile>();
                        baseTile.Initialize(hexCoord, AllTilesGO); // Initialisez la tuile pour WFC

                        // Créez un CeilClass pour stocker les informations de la tuile
                        CeilClass ceilClass = new CeilClass(hexCoord);
                        baseTile.ceilClass = ceilClass;

                        hexGrid.Add(hexCoord, currentTile);
                        globalPos.Add(hexCoord);
                        prefab.Add(currentTile);
                        worldToHexMap.Add(worldPosition, hexCoord); // Ajouter au mapping
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

       // StartCoroutine(WaveFunctionCollapse());
    }

    public Vector3 HexToWorldPosition(Vector3 hexCoord)
    {
        float x = width * (hexCoord.x + hexCoord.z / 2.0f);
        float z = height * hexCoord.z;
        return new Vector3(x, 0, z);
    }

    public void ReplaceTile(Vector3 hexCoord, GameObject newTilePrefab)
    {
        if (hexGrid.ContainsKey(hexCoord))
        {
            Destroy(hexGrid[hexCoord]);
            Vector3 worldPosition = HexToWorldPosition(hexCoord);
            GameObject newTile = Instantiate(newTilePrefab, worldPosition, Quaternion.identity, transform);
            hexGrid[hexCoord] = newTile;
        }
    }
/*
    private CeilClass GetTileWithFewestPossibilities()
    {
        CeilClass tileWithFewestPossibilities = null;
        int fewestPossibilities = int.MaxValue;

        foreach (var pos in globalPos)
        {
            BaseTile baseTile = hexGrid[pos].GetComponent<BaseTile>();
            CeilClass tile = baseTile.ceilClass;

            if (!tile.isCollapsed && tile.possibleTiles.Count < fewestPossibilities)
            {
                fewestPossibilities = tile.possibleTiles.Count;
                tileWithFewestPossibilities = tile;
            }
        }

        return tileWithFewestPossibilities;
    }

    private void CollapseTile(CeilClass tile)
    {
        if (tile.possibleTiles.Count == 0) return;

        // Sélectionnez un prefab aléatoire parmi les options possibles
        tile.selectedPrefab = tile.possibleTiles[Random.Range(0, tile.possibleTiles.Count)];
        tile.isCollapsed = true;

        Vector3 hexCoord = tile.hexCoord;
        Vector3 worldPosition = HexToWorldPosition(hexCoord);
        Destroy(hexGrid[hexCoord]);

        GameObject newTile = Instantiate(tile.selectedPrefab, worldPosition, Quaternion.identity, transform);
        hexGrid[hexCoord] = newTile;
    }

    private void UpdateNeighbors(CeilClass tile)
    {
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, -1, 0), new Vector3(1, 0, -1), new Vector3(0, 1, -1),
            new Vector3(-1, 1, 0), new Vector3(-1, 0, 1), new Vector3(0, -1, 1)
        };

        foreach (var direction in directions)
        {
            Vector3 neighborCoord = tile.hexCoord + direction;
            if (hexGrid.ContainsKey(neighborCoord))
            {
                BaseTile baseTile = hexGrid[neighborCoord].GetComponent<BaseTile>();
                CeilClass neighbor = baseTile.ceilClass;

                if (!neighbor.isCollapsed)
                {
                    List<GameObject> newPossibleTiles = new List<GameObject>();
                    foreach (var possibleTile in neighbor.possibleTiles)
                    {
                        if (IsValidNeighbor(tile.selectedPrefab, possibleTile, direction))
                        {
                            newPossibleTiles.Add(possibleTile);
                        }
                    }
                    neighbor.possibleTiles = newPossibleTiles;
                }
            }
        }
    }

    private bool IsValidNeighbor(GameObject tilePrefab, GameObject neighborPrefab, Vector3 direction)
    {
        if (tilePrefab == null || neighborPrefab == null)
        {
            Debug.LogError("Un des prefabs est nul.");
            return false;
        }

        BaseTile tileBase = tilePrefab.GetComponent<BaseTile>();
        BaseTile neighborTileBase = neighborPrefab.GetComponent<BaseTile>();

        if (tileBase == null || neighborTileBase == null)
        {
            Debug.LogError("Un des composants BaseTile est manquant.");
            return false;
        }

        int directionIndex = GetDirectionIndex(direction);
        int oppositeDirectionIndex = (directionIndex + 3) % 6;  // Index de la bordure opposée

        return tileBase.cellType.borders[directionIndex] == neighborTileBase.cellType.borders[oppositeDirectionIndex];
    }

    private int GetDirectionIndex(Vector3 direction)
    {
        if (direction == new Vector3(1, -1, 0)) return 0;
        if (direction == new Vector3(1, 0, -1)) return 1;
        if (direction == new Vector3(0, 1, -1)) return 2;
        if (direction == new Vector3(-1, 1, 0)) return 3;
        if (direction == new Vector3(-1, 0, 1)) return 4;
        if (direction == new Vector3(0, -1, 1)) return 5;

        Debug.LogError("Direction non valide");
        return -1;  // Erreur, direction non valide
    }

    private IEnumerator WaveFunctionCollapse()
    {
        while (true)
        {
            CeilClass tile = GetTileWithFewestPossibilities();
            if (tile == null) break;

            CollapseTile(tile);
            UpdateNeighbors(tile);
            yield return null;
        }
    }*/
}
