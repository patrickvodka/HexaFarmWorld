using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
    public List<Vector3> HexGridCollapsedYet = new List<Vector3>();
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
                        HexGridCollapsedYet.Add(hexCoord);
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
                HexGridCollapsedYet.Remove(new Vector3(0, 0, 0));
            }
            else
            {
                Debug.LogError("Problème de génération important");
            }

            
        }
    }

    private IEnumerator CheckBordersProcess()
    {
        // Tant que la liste des tuiles non collapsed n'est pas vide
        while (HexGridCollapsedYet.Count > 0)
        {
            // Chercher la tuile avec le plus de voisins collapsed
            GameObject bestTile = FindTileWithMostCollapsedNeighbors();
            if (bestTile != null)
            {
                Debug.Log($"Lancement du check des bordures pour {bestTile.GetComponent<BaseTile>().ceilClass.hexCoord}");
                // Lancer la coroutine pour vérifier les bordures de cette tuile
                yield return StartCoroutine(CheckBordersWithDelay(bestTile.transform));
            }
            else
            {
                Debug.LogWarning("Aucune tuile trouvée pour le check des bordures.");
                yield break;
            }
        }

        Debug.Log("Le processus de vérification des bordures est terminé.");
    }

    private GameObject FindTileWithMostCollapsedNeighbors()
    {
        GameObject bestTile = null;
        int maxCollapsedNeighbors = -1;

        foreach (Vector3 hexCoord in HexGridCollapsedYet)
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

    private IEnumerator CheckBordersWithDelay(Transform targetTileTransform)
    {
        // Récupérer le composant BaseTile attaché à la transform cible
        BaseTile baseTile = targetTileTransform.GetComponent<BaseTile>();
        if (baseTile != null)
        {
            Vector3 baseHexCoord = baseTile.ceilClass.hexCoord; // Obtenir la coordonnée hexagonale de la tuile de base

            // Liste pour stocker les types de bordures des tuiles voisines collapsed
            int[] borderArray = new int[6];
            for (int i = 0; i < 6; i++)
            {
                borderArray[i] = 0;// Initialiser les bordures à 0 ou 1 random // test  = 0 ici par default bug sur les coté dois changer ilmportant
                Debug.LogWarning(borderArray[i]);
            }

            bool hasCollapsedNeighbor = false; // Indicateur pour savoir si on a des voisins collapsed

            // Vérification des bordures des voisins
            for (int i = 0; i < directions.Count; i++)
            {
                Vector3 neighborCoord = baseHexCoord + directions[i]; // Calculer la coordonnée du voisin

                // Si la grille contient la coordonnée du voisin
                if (HexGridDictionary.ContainsKey(neighborCoord))
                {
                    BaseTile neighborTile = HexGridDictionary[neighborCoord].GetComponent<BaseTile>();
                    
                    // si le voisin est collapsed
                    if (neighborTile.ceilClass.isCollapsed)
                    {
                        hasCollapsedNeighbor = true;

                        // Récupérer la rotation en Y du voisin et la normaliser entre 0 et 360 degrés
                        float neighborRotationY = neighborTile.transform.rotation.eulerAngles.y;
                        int NbrOfRotation = ((int)neighborRotationY / 60) % 6; // Nombre de rotations de 60 degrés

                        // Copier les bordures du voisin dans un nouvel array
                        int[] adjustedBorders = new int[6];
                        neighborTile.cellType.borders.CopyTo(adjustedBorders, 0);

                        // Ajuster les bordures en fonction de la rotation
                        if (NbrOfRotation != 0)
                        {
                            if (neighborRotationY < 0)
                            {
                                adjustedBorders = RotateArrayLeft(adjustedBorders, NbrOfRotation);
                            }
                            else
                            {
                                adjustedBorders = RotateArrayRight(adjustedBorders, NbrOfRotation);
                            }
                        }

                        // Stocker le type de bordure ajustée
                        int neighborBorderType = adjustedBorders[(i + 3) % 6];
                        borderArray[i] = neighborBorderType;

                        // Afficher les informations de débogage
                        Debug.Log($"Voisin collapsed à {neighborCoord} avec border[{(i + 3) % 6}] = {neighborBorderType}. Stockage en borderArray[{i}]");

                        // Afficher l'état actuel de borderArray
                        Debug.Log($"État actuel de borderArray après ajout : [{string.Join(", ", borderArray)}]");
                    }

                }
                else
                {
                    // Si le voisin est hors de la grille, considérer comme une bordure 0
                    Debug.Log($"Aucun voisin à {baseHexCoord + directions[i]} - Considéré comme border 0");
                }

                // Attendre le délai spécifié avant de continuer à la prochaine itération
                yield return new WaitForSeconds(checkDelay);
            }

            // Afficher les bordures stockées pour la tuile de base
            Debug.Log($"borderArray pour {baseHexCoord}: [{string.Join(", ", borderArray)}]");

            // Si la tuile de base a des voisins collapsed
            if (hasCollapsedNeighbor)
            {
                int RotationToSpawn;
                // Essayer de trouver une tuile qui correspond aux bordures connues avec toutes les rotations possibles
                GameObject newTilePrefab = FindMatchingTilePrefabWithRotation(borderArray, out RotationToSpawn);
                if (newTilePrefab != null)
                {
                    // Remplacer la tuile de base par la nouvelle tuile
                    Destroy(targetTileTransform.gameObject); // Détruire la tuile existante
                    Vector3 worldPosition = HexToWorldPosition(baseHexCoord); // Convertir la position hexagonale en position mondiale
                    GameObject newTile = Instantiate(newTilePrefab, worldPosition, Quaternion.Euler(0, RotationToSpawn, 0), transform);
                    BaseTile newBaseTile = newTile.GetComponent<BaseTile>();
                    newBaseTile.Initialize(baseHexCoord, AllTilesGO); // Initialiser la nouvelle tuile

                    // Créer un CeilClass pour stocker les informations de la tuile
                    CeilClass ceilClass = new CeilClass(baseHexCoord);
                    newBaseTile.ceilClass = ceilClass;

                    // Marquer la nouvelle tuile comme collapsed
                    newBaseTile.ceilClass.isCollapsed = true;

                    // Ajouter la nouvelle tuile au dictionnaire de la grille hexagonale
                    HexGridDictionary[baseHexCoord] = newTile; // Mettre à jour la grille hexagonale
                    
                    // Retirer la tuile de la liste des tuiles non collapsed
                    if (HexGridCollapsedYet.Contains(baseHexCoord))
                    {
                        
                        HexGridCollapsedYet.Remove(baseHexCoord);
                    }

                    Debug.Log($"Tuile à {baseHexCoord} remplacée par {newTilePrefab.name} et marquée comme collapsed.");

                    // Attendre le délai spécifié avant de continuer à la prochaine tuile
                    yield return new WaitForSeconds(checkDelay);
                }
                else
                {
                    // Si aucune tuile correspondante n'a été trouvée, afficher un avertissement
                    Debug.LogWarning($"Aucune tuile correspondante trouvée pour {baseHexCoord} avec les bordures spécifiées : [{string.Join(", ", borderArray)}].");
                }
            }
            else
            {
                // Si aucune tuile voisine n'est collapsed, afficher un message
                Debug.Log($"Aucune tuile collapsed voisine trouvée pour {baseHexCoord}.");
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
