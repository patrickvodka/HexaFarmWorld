using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BorderCheckTester : MonoBehaviour
{
    public float checkDelay = 1.0f; // Délai entre chaque vérification de bordures
    public SO_AllTiles allTilePrefabs; // Liste de tous les prefabs dans des SO_AllTiles.ceil de tuiles disponibles
    public List<GameObject> AllTilesGO = new List<GameObject>();
    private float height = 30; // Hauteur de l'hexagone
    private float width = 34.5f;

    public int radiusMap; // Rayon de la carte hexagonale

    // Dictionnaire pour stocker les tuiles sur la grille hexagonale
    public Dictionary<Vector3, GameObject> HexGridDictionary = new Dictionary<Vector3, GameObject>();
    // Liste pour savoir si toutes les nodes ont été collapsées
    [FormerlySerializedAs("HexGridCollapsedYet")] public List<Vector3> HexGridNotCollapsedYet = new List<Vector3>();
    // Liste des tuiles qui doivent être vérifiées mais ne doivent pas être marquées comme collapsed
    public List<Vector3> TilesWhoNeedToGetCheck = new List<Vector3>();
    private List<Vector3> CheckedButNotCollapsedTiles = new List<Vector3>();
    // Liste des directions hexagonales (voisins)
    private List<Vector3> directions = new List<Vector3>()
    {
        new Vector3(0, -1, 1), new Vector3(1, -1, 0), new Vector3(1, 0, -1),
        new Vector3(0, 1, -1), new Vector3(-1, 1, 0), new Vector3(-1, 0, 1)
    };

    void Start()
    {
        // Charger les prefabs de tuiles
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

        // Générer les tuiles de la carte au démarrage
        GenerateRandomTileMap();

        // Commencer le processus de vérification des bordures
        StartCoroutine(CheckBordersProcess());
    }

    private void GenerateRandomTileMap()
    {
        // Nettoyer le dictionnaire avant de commencer la génération
        HexGridDictionary.Clear();
        
        for (int q = -Mathf.CeilToInt(radiusMap); q <= Mathf.CeilToInt(radiusMap); q++)
        {
            for (int r = -Mathf.CeilToInt(radiusMap); r <= Mathf.CeilToInt(radiusMap); r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= radiusMap)
                {
                    Vector3 hexCoord = new Vector3(q, r, s);
                    Vector3 worldPosition = HexToWorldPosition(hexCoord);

                    if (!HexGridDictionary.ContainsKey(hexCoord))
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

                        // Ajouter la tuile au dictionnaire de la grille hexagonale
                        HexGridDictionary.Add(hexCoord, currentTile);

                        // Ajouter la tuile à la liste des tuiles non collapsed
                        HexGridNotCollapsedYet.Add(hexCoord);
                    }
                }
            }
        }

        // Assurez-vous que la tuile (0, 0, 0) est correctement marquée comme collapsed
        if (HexGridDictionary.ContainsKey(new Vector3(0, 0, 0)))
        {
            BaseTile CenterTile = HexGridDictionary[new Vector3(0, 0, 0)].GetComponent<BaseTile>();
            if (CenterTile != null)
            {
                CenterTile.ceilClass.isCollapsed = true;
                HexGridNotCollapsedYet.Remove(new Vector3(0, 0, 0));
            }
            else
            {
                Debug.LogError("Problème de génération important");
            }
        }
    }
    

    private IEnumerator CheckBordersProcess()
    {
        // Initial collapse check for the starting tile
        if (HexGridNotCollapsedYet.Count > 0)
        {
            GameObject initialTile = HexGridDictionary[new Vector3(0, 0, 0)];
            Debug.Log($"Lancement du check des bordures pour {initialTile.GetComponent<BaseTile>().ceilClass.hexCoord}");
            yield return StartCoroutine(CheckBordersWithDelay(initialTile.transform, true));
        }

        // Process subsequent border checks
        while (HexGridNotCollapsedYet.Count > 0 || TilesWhoNeedToGetCheck.Count > 0)
        {
            if (TilesWhoNeedToGetCheck.Count > 0)
            {
                // Traiter les tuiles dans TilesWhoNeedToGetCheck sans collapse
                while (TilesWhoNeedToGetCheck.Count > 0)
                {
                    Vector3 tileCoord = TilesWhoNeedToGetCheck[0];
                    TilesWhoNeedToGetCheck.RemoveAt(0);
                    if (HexGridDictionary.TryGetValue(tileCoord, out GameObject tileObject))
                    {
                        Debug.Log($"Vérification des bordures pour {tileCoord}");
                        yield return StartCoroutine(CheckBordersWithDelay(tileObject.transform, false));
                    }
                }
            }
            else
            {
                // Chercher la prochaine tuile pour un collapse check
                GameObject bestTile = FindTileWithMostCollapsedNeighbors();
                if (bestTile != null)
                {
                    Debug.Log($"Lancement du check des bordures pour {bestTile.GetComponent<BaseTile>().ceilClass.hexCoord}");
                    yield return StartCoroutine(CheckBordersWithDelay(bestTile.transform, true));
                }
                else
                {
                    Debug.LogWarning("Aucune tuile trouvée pour le check des bordures.");
                    yield break;
                }
            }
        }

        Debug.Log("Le processus de vérification des bordures est terminé.");
    }

    private GameObject FindTileWithMostCollapsedNeighbors()
    {
        GameObject bestTile = null;
        int maxCollapsedNeighbors = -1;

        foreach (Vector3 hexCoord in HexGridNotCollapsedYet)
        {
            if (HexGridDictionary.TryGetValue(hexCoord, out GameObject tileObject))
            {
                BaseTile baseTile = tileObject.GetComponent<BaseTile>();

                if (baseTile != null)
                {
                    int collapsedNeighbors = 0;

                    // Compter les voisins collapsed
                    for (int i = 0; i < directions.Count; i++)
                    {
                        Vector3 neighborCoord = hexCoord + directions[i];
                        if (HexGridDictionary.ContainsKey(neighborCoord))
                        {
                            BaseTile neighborTile = HexGridDictionary[neighborCoord].GetComponent<BaseTile>();
                            if (neighborTile.ceilClass.isCollapsed)
                            {
                                collapsedNeighbors++;
                            }
                        }
                    }

                    // Trouver la tuile avec le maximum de voisins collapsed
                    if (collapsedNeighbors > maxCollapsedNeighbors)
                    {
                        maxCollapsedNeighbors = collapsedNeighbors;
                        bestTile = tileObject;
                    }
                }
            }
        }

        Debug.Log($"Tuile avec le plus de voisins collapsed est {bestTile?.GetComponent<BaseTile>().ceilClass.hexCoord} avec {maxCollapsedNeighbors} voisins collapsed.");
        return bestTile;
    }

   private IEnumerator CheckBordersWithDelay(Transform targetTileTransform, bool collapseTile)
{
    BaseTile baseTile = targetTileTransform.GetComponent<BaseTile>();
    if (baseTile != null)
    {
        Vector3 baseHexCoord = baseTile.ceilClass.hexCoord;
        int[] borderArray = new int[6];
        for (int i = 0; i < 6; i++)
        {
            borderArray[i] = 0; // Initialiser les bordures à 0
        }

        bool hasCollapsedNeighbor = false;

        // Vérification des bordures des voisins
        for (int i = 0; i < directions.Count; i++)
        {
            Vector3 neighborCoord = baseHexCoord + directions[i];
            if (HexGridDictionary.ContainsKey(neighborCoord))
            {
                BaseTile neighborTile = HexGridDictionary[neighborCoord].GetComponent<BaseTile>();
                if (neighborTile != null)
                {
                    if (neighborTile.ceilClass.isCollapsed)
                    {
                        hasCollapsedNeighbor = true;
                        float neighborRotationY = neighborTile.transform.rotation.eulerAngles.y;
                        int NbrOfRotation = ((int)neighborRotationY / 60) % 6;
                        int[] adjustedBorders = new int[6];
                        neighborTile.cellType.borders.CopyTo(adjustedBorders, 0);

                        if (NbrOfRotation != 0)
                        {
                            adjustedBorders = RotateArrayRight(adjustedBorders, NbrOfRotation);
                        }

                        int neighborBorderType = adjustedBorders[(i + 3) % 6]; // important changer le - ici car pas pris en compte
                        borderArray[i] = neighborBorderType;
                    }
                    else
                    {
                        int[] adjustedBorders = new int[6];
                        neighborTile.cellType.borders.CopyTo(adjustedBorders, 0);
                        int neighborBorderType = adjustedBorders[(i + 3) % 6]; // important changer le - ici car pas pris en compte
                        borderArray[i] = neighborBorderType;
                        // Ajouter les tuiles non collapsées à TilesWhoNeedToGetCheck
                        if (!TilesWhoNeedToGetCheck.Contains(neighborCoord) && !CheckedButNotCollapsedTiles.Contains(neighborCoord))
                        {
                            TilesWhoNeedToGetCheck.Add(neighborCoord);
                            //Debug.Log($"Ajout de la tuile non collapsed {neighborCoord} à TilesWhoNeedToGetCheck");
                        }
                    }
                }
            }
            else
            {
                borderArray[i] = 0;
            }

            yield return new WaitForSeconds(checkDelay);
        }

        Debug.Log($"borderArray pour {baseHexCoord}: [{string.Join(", ", borderArray)}]");

        if (collapseTile )/*&& hasCollapsedNeighbor)*/ // test2
        {
            int rotationToSpawn;
            GameObject newTilePrefab = FindMatchingTilePrefabWithRotation(borderArray, out rotationToSpawn);
            if (newTilePrefab != null)
            {
                Destroy(targetTileTransform.gameObject);
                Vector3 worldPosition = HexToWorldPosition(baseHexCoord);
                GameObject newTile = Instantiate(newTilePrefab, worldPosition, Quaternion.Euler(0, rotationToSpawn, 0), transform);
                BaseTile newBaseTile = newTile.GetComponent<BaseTile>();
                newBaseTile.Initialize(baseHexCoord, AllTilesGO);

                CeilClass ceilClass = new CeilClass(baseHexCoord);
                newBaseTile.ceilClass = ceilClass;
                newBaseTile.ceilClass.isCollapsed = true;

                HexGridDictionary[baseHexCoord] = newTile;
                if (HexGridNotCollapsedYet.Contains(baseHexCoord))
                {
                    HexGridNotCollapsedYet.Remove(baseHexCoord);
                }

                // Réinitialiser la liste des tuiles vérifiées sans collapse
                CheckedButNotCollapsedTiles.Clear();

                Debug.Log($"Tuile à {baseHexCoord} remplacée par {newTilePrefab.name} et marquée comme collapsed.");

                yield return new WaitForSeconds(checkDelay);

                foreach (var dir in directions)
                {
                    Vector3 neighborCoord = baseHexCoord + dir;
                    if (HexGridDictionary.ContainsKey(neighborCoord))
                    {
                        BaseTile neighborTile = HexGridDictionary[neighborCoord].GetComponent<BaseTile>();
                        if (neighborTile != null && !neighborTile.ceilClass.isCollapsed && !TilesWhoNeedToGetCheck.Contains(neighborCoord))
                        {
                            TilesWhoNeedToGetCheck.Add(neighborCoord);
                            Debug.Log($"Ajout de la tuile {neighborCoord} à TilesWhoNeedToGetCheck");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Aucune tuile correspondante trouvée pour {baseHexCoord} avec les bordures spécifiées : [{string.Join(", ", borderArray)}].");
                if (HexGridNotCollapsedYet.Contains(baseHexCoord))
                {
                    HexGridNotCollapsedYet.Remove(baseHexCoord);
                    baseTile.ceilClass.isCollapsed = true;
                }

                // Réinitialiser la liste des tuiles vérifiées sans collapse
                CheckedButNotCollapsedTiles.Clear();
            }
        }
        else if (!collapseTile)
        {
            // Ajouter les coordonnées de la tuile à CheckedButNotCollapsedTiles si elle n'est pas collapse
            if (!CheckedButNotCollapsedTiles.Contains(baseHexCoord))
            {
                CheckedButNotCollapsedTiles.Add(baseHexCoord);
                Debug.Log($"Tuile {baseHexCoord} ajoutée à CheckedButNotCollapsedTiles");
            }
        }
    }
}






    private GameObject FindMatchingTilePrefabWithRotation(int[] borderArray, out int customRotation)
    {
        for (int rotation = 0; rotation < 6; rotation++)
        {
            int[] rotatedBorderArray = new int[6];
            for (int i = 0; i < 6; i++)
            {
                rotatedBorderArray[i] = borderArray[(i + rotation) % 6];
            }

            Debug.Log($"Vérification avec rotation de {rotation * 60} degrés. BorderArray testé : [{string.Join(", ", rotatedBorderArray)}]");

            foreach (GameObject tilePrefab in AllTilesGO)
            {
                BaseTile tile = tilePrefab.GetComponent<BaseTile>();
                if (tile != null)
                {
                    int[] tileBorders = tile.cellType.borders;
                    bool match = true;

                    Debug.Log($"Comparaison pour {tilePrefab.name}. Bordures : [{string.Join(", ", tileBorders)}] avec [{string.Join(", ", rotatedBorderArray)}]");

                    for (int i = 0; i < 6; i++)
                    {
                        if (rotatedBorderArray[i] != tileBorders[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        Debug.Log($"Trouvé un prefab de tuile correspondant avec rotation {rotation * 60}° pour les bordures {string.Join(", ", borderArray)} : {tilePrefab.name}");
                        customRotation = rotation * 60;
                        return tilePrefab;
                    }
                    else
                    {
                        Debug.Log($"Non match pour {tilePrefab.name}. Bordures comparées : [{string.Join(", ", tileBorders)}] vs [{string.Join(", ", rotatedBorderArray)}]");
                    }
                }
            }
        }
        customRotation = 0;
        return null;
    }

    public Vector3 HexToWorldPosition(Vector3 hexCoord)
    {
        float x = width * (hexCoord.x + hexCoord.z / 2.0f);
        float z = height * hexCoord.z;
        return new Vector3(x, 0, z);
    }

    private int[] RotateArrayRight(int[] array, int positions)
    {
        int length = array.Length;
        int[] rotatedArray = new int[length];
        for (int i = 0; i < length; i++)
        {
            rotatedArray[(i + positions) % length] = array[i];
        }
        return rotatedArray;
    }

    private int[] RotateArrayLeft(int[] array, int positions)
    {
        int length = array.Length;
        int[] rotatedArray = new int[length];
        for (int i = 0; i < length; i++)
        {
            rotatedArray[i] = array[(i + positions) % length];
        }
        return rotatedArray;
    }
}
