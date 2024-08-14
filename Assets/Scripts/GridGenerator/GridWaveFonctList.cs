using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class GridWaveFonctList : MonoBehaviour
{
    public float checkDelay = 1.0f; // Delay between border checks
    public SO_AllTiles allTilePrefabs; // List of all tile prefabs in SO_AllTiles.ceil of available tiles
    public List<GameObject> AllTilesGO = new List<GameObject>();
    private float height = 30; // Height of the hexagon
    private float width = 34.5f;
    public int radiusMap; // Radius of the hexagonal map

    public Vector3 transCube;
    // Dictionary to store tiles on the hexagonal grid
    public Dictionary<Vector3, GameObject> HexGridDictionary = new Dictionary<Vector3, GameObject>();
    // List to track uncollapsed nodes
    public List<Vector3> HexGridNotCollapsedYet = new List<Vector3>();
    // List of tiles that need checking but shouldn't be marked as collapsed
    public List<Vector3> TilesWhoNeedToGetCheck = new List<Vector3>();
    private List<Vector3> CheckedButNotCollapsedTiles = new List<Vector3>();
    public Dictionary<BigInteger, List<(GameObject tilePrefab, int rotation)>> hashToTileMap = new Dictionary<BigInteger, List<(GameObject, int)>>(); // Dictionary for hashes

    // List of hexagonal directions (neighbors)
    private List<Vector3> directions = new List<Vector3>()
    {
        new Vector3(0, -1, 1), new Vector3(1, -1, 0), new Vector3(1, 0, -1),
        new Vector3(0, 1, -1), new Vector3(-1, 1, 0), new Vector3(-1, 0, 1)
    };

    void Start()
    {
        // Load tile prefabs
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
        PrecomputeTileHashes();

        // Generate map tiles at startup
        GenerateRandomTileMap();
        
        // Start border check process
        StartCoroutine(CheckBordersProcess());
    }

    private void FixedUpdate()
    {
        
        if (transCube != new Vector3(0,0,0))
        {
            Debug.Log("after" + HexGridDictionary[transCube].transform);
           StartCoroutine(CheckBordersWithDelay(HexGridDictionary[transCube].transform, true));
        }
    }

    private void GenerateRandomTileMap()
    {
        // Clear dictionary before generating
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
                            Debug.LogError("The list of possible prefabs is empty.");
                            continue;
                        }

                        GameObject randomTilePrefab = possibleTilePrefabs[Random.Range(0, possibleTilePrefabs.Count)];
                        GameObject currentTile = Instantiate(randomTilePrefab, worldPosition, Quaternion.identity, transform);
                        BaseTile baseTile = currentTile?.GetComponent<BaseTile>();
                        if (baseTile != null)
                        {
                            baseTile.Initialize(hexCoord);
                            baseTile.ReturnInput();
                        }
                        CeilClass ceilClass = new CeilClass(hexCoord);
                        baseTile.ceilClass = ceilClass;

                        // Add tile to the hexagonal grid dictionary
                        HexGridDictionary.Add(hexCoord, currentTile);

                        // Add tile to the list of uncollapsed tiles
                        HexGridNotCollapsedYet.Add(hexCoord);
                    }
                }
            }
        }

        // Ensure tile (0, 0, 0) is correctly marked as collapsed
        if (HexGridDictionary.ContainsKey(new Vector3(0, 0, 0)))
        {
            BaseTile centerTile = HexGridDictionary[new Vector3(0, 0, 0)]?.GetComponent<BaseTile>();
            if (centerTile != null)
            {
                centerTile.ceilClass.isCollapsed = true;
                HexGridNotCollapsedYet.Remove(new Vector3(0, 0, 0));
                
            }
            else
            {
                Debug.LogError("Major generation problem");
            }
        }
    }

    private IEnumerator CheckBordersProcess()
    {

        // Process subsequent border checks
        while (HexGridNotCollapsedYet.Count > 0 || TilesWhoNeedToGetCheck.Count > 0)
        {
            if (TilesWhoNeedToGetCheck.Count > 0)
            {
                // Process tiles in TilesWhoNeedToGetCheck without collapse
                while (TilesWhoNeedToGetCheck.Count > 0)
                {
                    Vector3 tileCoord = TilesWhoNeedToGetCheck[0];
                    TilesWhoNeedToGetCheck.RemoveAt(0);
                    if (HexGridDictionary.TryGetValue(tileCoord, out GameObject tileObject))
                    {
                        Debug.Log($"Checking borders for {tileCoord}");
                        yield return StartCoroutine(CheckBordersWithDelay(tileObject.transform, false));
                    }
                }
            }
            else
            {
               // CheckedButNotCollapsedTiles.Clear();// clear pour une nouvelle fois une fonction collapse
                // Find next tile for a collapse check
                GameObject bestTile = FindTileWithMostCollapsedNeighbors();
                if (bestTile != null)
                {
                    Debug.Log($"Starting border cllapsed check for {bestTile.GetComponent<BaseTile>().ceilClass.hexCoord}");
                    yield return StartCoroutine(CheckBordersWithDelay(bestTile.transform, true));
                }
                else
                {
                    Debug.LogWarning("No tile found for border check.");
                    yield break;
                }
            }
        }

        Debug.Log("Border check process completed.");
    }

    private GameObject FindTileWithMostCollapsedNeighbors()
    {
        GameObject bestTile = null;
        int maxCollapsedNeighbors = -1;

        foreach (Vector3 hexCoord in HexGridNotCollapsedYet)
        {
            if (HexGridDictionary.TryGetValue(hexCoord, out GameObject tileObject))
            {
                BaseTile baseTile = tileObject?.GetComponent<BaseTile>();

                if (baseTile != null)
                {
                    int collapsedNeighbors = 0;

                    // Count collapsed neighbors
                    for (int i = 0; i < directions.Count; i++)
                    {
                        Vector3 neighborCoord = hexCoord + directions[i];
                        if (HexGridDictionary.ContainsKey(neighborCoord))
                        {
                            BaseTile neighborTile = HexGridDictionary[neighborCoord]?.GetComponent<BaseTile>();
                            if (neighborTile.ceilClass.isCollapsed)
                            {
                                collapsedNeighbors++;
                            }
                        }
                    }

                    // Find tile with the most collapsed neighbors
                    if (collapsedNeighbors > maxCollapsedNeighbors)
                    {
                        maxCollapsedNeighbors = collapsedNeighbors;
                        bestTile = tileObject;
                    }
                }
            }
        }

        Debug.Log($"Tile with the most collapsed neighbors is {bestTile?.GetComponent<BaseTile>().ceilClass.hexCoord} with {maxCollapsedNeighbors} collapsed neighbors.");
        return bestTile;
    }

    private IEnumerator CheckBordersWithDelay(Transform targetTileTransform, bool collapseTile)
    {
        BaseTile baseTile = targetTileTransform?.GetComponent<BaseTile>();
        if (baseTile != null)
        {
            Vector3 baseHexCoord = baseTile.ceilClass.hexCoord;
            List<List<int>> borderList = new List<List<int>>(); // Liste de listes pour les valeurs des bordures

            // Initialisation de la liste de bordures
            for (int i = 0; i < 6; i++)
            {
                borderList.Add(new List<int>());
                
            }

            bool hasCollapsedNeighbor = false;

            // Vérifiez les bordures des voisins
            for (int i = 0; i < directions.Count; i++)
            {
                Vector3 neighborCoord = baseHexCoord + directions[i];
                if (HexGridDictionary.ContainsKey(neighborCoord))
                {
                    BaseTile neighborTile = HexGridDictionary[neighborCoord].GetComponent<BaseTile>();
                    if (neighborTile != null )
                    {
                        if (neighborTile.ceilClass.isCollapsed)
                        {
                            hasCollapsedNeighbor = true;
                            List<int> neighborBordersList = neighborTile.cellType.borders[i];
                            int neighborRotation = (int)(neighborTile.transform.rotation.eulerAngles.y / 60);
                            int borderIndex = (i + 3 - neighborRotation+6) % 6;

                            borderList[i] = new List<int>(neighborTile.cellType.borders[borderIndex]);
                            //Debug.LogWarning($"borderIndex {borderIndex} is out of range for neighborBordersList with length {neighborBordersList.Count} GO: {baseTile.transform.gameObject}");
                        }
                        else
                        {
                            List<int> neighborBordersList = neighborTile.cellType.borders[i];
                            int neighborRotation = (int)(neighborTile.transform.rotation.eulerAngles.y / 60);
                            int borderIndex = (i + 3 - neighborRotation+6) % 6;  
                            borderList[i] = new List<int>(neighborTile.cellType.borders[borderIndex]);
                        }


                    }
                }
                else
                {
                    borderList[i].Add(0);
                }
            }

        
            List<(GameObject tilePrefab, int rotation)> matchingTilePrefabsWithRotation = FindMatchingTilePrefabsWithRotation(borderList);
            if (matchingTilePrefabsWithRotation.Count > 0)
            {
                (GameObject bestMatchingTilePrefab, int bestRotation) = SelectBestMatchingTile(matchingTilePrefabsWithRotation, borderList);
                if (bestMatchingTilePrefab != null)
                {
                    GameObject newTile = Instantiate(bestMatchingTilePrefab, targetTileTransform.position,
                        Quaternion.Euler(0, bestRotation * 60, 0), transform);

                    BaseTile newBaseTile = newTile?.GetComponent<BaseTile>();
                    if (newBaseTile != null)
                    {
                        newBaseTile.Initialize(baseHexCoord);
                        newBaseTile.ReturnInput();
                    }

                    baseTile.ceilClass.isCollapsed = collapseTile;
                    if (collapseTile)
                    {
                        HexGridNotCollapsedYet.Remove(baseHexCoord);
                        
                    }
                    else
                    {
                       
                        CheckedButNotCollapsedTiles.Add(baseHexCoord);
                    }

                    Destroy(targetTileTransform.gameObject);

                    HexGridDictionary[baseHexCoord] = newTile;
                }

                for (int i = 0; i < directions.Count; i++)
                {
                    Vector3 neighborCoord = baseHexCoord + directions[i];
                    if (HexGridDictionary.ContainsKey(neighborCoord))
                    {
                        BaseTile neighborTile = HexGridDictionary[neighborCoord]?.GetComponent<BaseTile>();
                        if (neighborTile != null && !neighborTile.ceilClass.isCollapsed && !TilesWhoNeedToGetCheck.Contains(neighborCoord) && !CheckedButNotCollapsedTiles.Contains(neighborCoord))
                        {
                            TilesWhoNeedToGetCheck.Add(neighborCoord);
                        }
                    }
                }

                yield return new WaitForSeconds(checkDelay);
            }
            else
            {
                Debug.LogError("No matching tiles found ! ");
                 string concatenatedBorders = string.Join(",", borderList.SelectMany(subList => subList));//Base debug for list 
                Debug.LogError($"bordures pour la liste {concatenatedBorders}.");
                
               // baseTile.ceilClass.isCollapsed = collapseTile;
            }
        }
    
}



    private List<(GameObject tilePrefab, int rotation)> FindMatchingTilePrefabsWithRotation(List<List<int>> borderList)
    {
       // string concatenatedBorders = string.Join(",", borderList.SelectMany(subList => subList));//Base debug for list 
        //Debug.LogWarning($"bordures pour la liste {concatenatedBorders}."); //Base debug for list 
        
        List<(GameObject tilePrefab, int rotation)> matchingTiles = new List<(GameObject, int)>();
        List<List<int>> SearchingList = ScriptHelper.GenerateConfigurations(borderList);
        if (SearchingList.Count > 0)
        {
            foreach (var border in SearchingList)
            {
                int[] borderArray = new int[6];
                borderArray = border.ToArray();
              //  Debug.LogWarning(string.Join(",", borderArray));
                for (int rotation = 0; rotation < 6; rotation++)
                {
                    BigInteger hash = GetBorderHash(borderArray);
                    if (hashToTileMap.TryGetValue(hash, out List<(GameObject, int)> matchingTilePrefabs))
                    {/*
                        foreach (var varPrefab in matchingTilePrefabs)
                        {
                            Debug.LogWarning(varPrefab.Item1 + "rota : "+ varPrefab.Item2);
                        }*/
                        matchingTiles.AddRange(matchingTilePrefabs);
                    }

                    
                    borderArray = RotateBorderArray(borderArray, 1);
                }
            }
        }else{Debug.LogError("vide liste");}

        return matchingTiles;
    }

    private (GameObject, int) SelectBestMatchingTile(List<(GameObject tilePrefab, int rotation)> matchingTilePrefabsWithRotation, List<List<int>> borderList)
    {
        if (matchingTilePrefabsWithRotation.Count > 0)
        {
            foreach (var (tilePrefab, rotation) in matchingTilePrefabsWithRotation)
            {
                BaseTile baseTile = tilePrefab.GetComponent<BaseTile>();
                if (baseTile != null)
                {
                    List<List<int>> tileBorders = baseTile.cellType.borders.ToList();
                    bool isMatch = true;
                    for (int i = 0; i < directions.Count; i++)
                    {
                        int borderIndex = (i + rotation) % 6;
                        if (!tileBorders[borderIndex].SequenceEqual(borderList[i]))
                        {
                            
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        return (tilePrefab, rotation);
                    }
                }
            }
        }

        // Default return if no exact match is found
        return(null,0); //matchingTilePrefabsWithRotation[0];

    }

    private void PrecomputeTileHashes()
    {
        hashToTileMap.Clear();

        foreach (var tilePrefab in AllTilesGO)
        {
            BaseTile baseTile = tilePrefab.GetComponent<BaseTile>();
            if (baseTile != null)
            {
                baseTile.ReturnInput();
            }

            if (baseTile != null && baseTile.cellType != null && baseTile.cellType.borders != null)
            {
                List<List<int>> needGenerationForList = new List<List<int>>();
                for (int i = 0; i < 6; i++)
                {
                    needGenerationForList.Add(baseTile.cellType.borders[i]);
                }

                List<List<int>> generatedListOfBorder = ScriptHelper.GenerateConfigurations(needGenerationForList);
                foreach (var borderList in generatedListOfBorder)
                {
                    int[] borderArray = new int[6];
                    borderArray = borderList.ToArray();
                    for (int i = 0; i < 6; i++)
                    {
                        borderArray[i] = baseTile.cellType.borders[i][0];
                    }

                    // Compute hashes for all 6 rotations of the border array
                    for (int rotation = 0; rotation < 6; rotation++)
                    {
                        BigInteger hash = GetBorderHash(borderArray);
                        if (!hashToTileMap.ContainsKey(hash))
                        {
                            hashToTileMap[hash] = new List<(GameObject, int)>();
                            hashToTileMap[hash].Add((tilePrefab, rotation));
                        }
                        else
                        {
                            List<(GameObject, int)> checkList = new List<(GameObject, int)>();
                            List<(GameObject, int)> tileList = new List<(GameObject, int)>();
                            tileList.Add((tilePrefab,rotation));
                            bool isInList = false;
                            if (hashToTileMap.TryGetValue(hash,out checkList))
                            {
                                foreach (var check in checkList)
                                {
                                    if( check.Item1 == tilePrefab && check.Item2 == rotation)
                                    {
                                        Debug.LogError($"{check.Item1}_{check.Item2} est ! :  {tilePrefab}_{rotation}");
                                        isInList = true;
                                    }
                                    else
                                    {
                                        Debug.LogError($"{check.Item1}_{check.Item2} n'est pas {tilePrefab}_{rotation}");
                                    }
                                }

                                if (!isInList)
                                {
                                    hashToTileMap[hash].Add((tilePrefab, rotation));
                                }
                               
                            }
                        }

                        // Rotate the border array for the next rotation
                        borderArray = RotateBorderArray(borderArray, 1);
                    }
                }
            }
        }
        Debug.Log("tilemap max = " + hashToTileMap.Count);
    }

    private int[] RotateBorderArray(int[] borderArray, int rotations)
    {
        int[] rotatedBorders = new int[6];
        int length = borderArray.Length;

        // Effectuer les rotations
        for (int i = 0; i < length; i++)
        {
            int newIndex = (i + rotations) % length;
            rotatedBorders[newIndex] = borderArray[i];
        }

        return rotatedBorders;
    }

    public BigInteger GetBorderHash(int[] borders)
    {
        BigInteger hash = 17;
        foreach (int border in borders)
        {
            hash = hash * 31 + border;
        }
        return hash;
    }
    
    public  int[] ReverseHash(BigInteger hash, int length)
    {
        int[] borders = new int[length];
        BigInteger tempHash = hash;

        for (int i = length - 1; i >= 0; i--)
        {
            borders[i] = (int)(tempHash % 31);
            tempHash = (tempHash - borders[i]) / 31;
        }

        return borders;
    }

    private Vector3 HexToWorldPosition(Vector3 hexCoord)
    {
        float x = (hexCoord.x + hexCoord.z * 0.5f) * width;
        float y = hexCoord.z * height ;

        return new Vector3(x, 0, y);
    }
}
