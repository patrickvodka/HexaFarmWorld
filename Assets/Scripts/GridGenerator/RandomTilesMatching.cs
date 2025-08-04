using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public partial class  HexaWaveFonctCollapse
{
    private E_BiomeType.BiomeType biome;
    private void GenerateNoiseTileMap()
    {
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
                        float noiseScale = 0.2f;
                        float noiseValue = Mathf.PerlinNoise(q * noiseScale + 1000f, r * noiseScale + 1000f);

                        // Convert noise to a biome type
                        E_BiomeType.BiomeType currentBiome = GetBiomeFromNoise(noiseValue);
                        List<GameObject> ValidGoBiome = new List<GameObject>();
                        // Find tile type matching this biome
                        foreach (var Go in AllTilesGO)
                        {
                            BaseTile Tile = Go.GetComponent<BaseTile>();
                            if (Tile != null)
                            {
                                if (Tile.cellType.biome == currentBiome)
                                {
                                    //Debug.LogError($"No prefab found for biome {currentBiome} == {Tile.cellType.biome} : {Go.name}");
                                    ValidGoBiome.Add(Go);
                                }
                            }
                            else
                            {
                                Debug.LogError("Tile == Null !!!");
                            }
                            
                        }

                        //Debug.LogWarning($"{currentBiome}");
                        if (ValidGoBiome.Count == 0)
                        {
                            Debug.LogError($"No prefab found for biome {currentBiome} == {GetBiomeFromNoise(noiseValue)}: {noiseValue}"); 
                            GameObject prefab = AllTilesGO[3];
                            GameObject tileGO = Instantiate(prefab, worldPosition, Quaternion.identity, transform);

                            BaseTile baseTile = tileGO?.GetComponent<BaseTile>();
                            if (baseTile != null)
                            {
                                baseTile.Initialize(hexCoord);
                                baseTile.ReturnInput();
                                baseTile.ceilClass = new CeilClass(hexCoord);
                            }

                            HexGridDictionary.Add(hexCoord, tileGO);
                            HexGridNotCollapsedYet.Add(hexCoord);
                        }
                        else
                        {

                            GameObject prefab = ValidGoBiome[Random.Range(0, ValidGoBiome.Count)];
                            GameObject tileGO = Instantiate(prefab, worldPosition, Quaternion.identity, transform);

                            BaseTile baseTile = tileGO?.GetComponent<BaseTile>();
                            if (baseTile != null)
                            {
                                baseTile.Initialize(hexCoord);
                                baseTile.ReturnInput();
                                baseTile.ceilClass = new CeilClass(hexCoord);
                            }

                            HexGridDictionary.Add(hexCoord, tileGO);
                            HexGridNotCollapsedYet.Add(hexCoord);
                        }
                    }
                }
            }
        }

        float totalWidth = ((radiusMap * 2) + 1) * width;   
        float totalHeight = ((radiusMap*2)+1) * height;          

        planeTrans.transform.localScale = new Vector3(totalWidth/10, 1, totalHeight/10);

        fogOfWar.GenerateFogTextureFromHexGrid();
    }
    E_BiomeType.BiomeType GetBiomeFromNoise(float noise)
    {
        if ( noise <= 0.20f) return E_BiomeType.BiomeType.Water;
        if (noise > 0.20f && noise <= 0.30f) return E_BiomeType.BiomeType.Sand;
        if (noise > 0.30f && noise <= 0.50f) return E_BiomeType.BiomeType.Dirt;
        if (noise > 0.50f && noise <= 0.75f) return E_BiomeType.BiomeType.Forest;//forest
        if (noise > 0.75f && noise <= 0.80f) return E_BiomeType.BiomeType.Mountain;
        if (noise > 0.80f && noise <= 0.90f) return E_BiomeType.BiomeType.City;
        if (noise > 0.90f) return E_BiomeType.BiomeType.Dirt;
        return E_BiomeType.BiomeType.Dirt;
    }


    private List<(GameObject tilePrefab, int rotation)> FindMatchingTilePrefabsWithRotation(List<List<int>> borderList)
    {
        string concatenatedBorders = string.Join(",", borderList.SelectMany(subList => subList));//Base debug for list 
        Debug.LogWarning($"bordures pour la liste {concatenatedBorders}."); //Base debug for list 
        
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

    private (GameObject, int) SelectBestMatchingTile(
        List<(GameObject tilePrefab, int rotation)> matchingTilePrefabsWithRotation,
        List<List<int>> borderList)
    {
        if (matchingTilePrefabsWithRotation.Count == 0)
            return (null, -1); // Aucun tile disponible
        
        List<(GameObject, int)> ValidTilesList = new List<(GameObject, int)>();
        
        foreach (var (tilePrefab, rotation) in matchingTilePrefabsWithRotation)
        {
            BaseTile baseTile = tilePrefab.GetComponent<BaseTile>();
            if (baseTile == null)
            {
                Debug.LogWarning($"Tile {tilePrefab.name} n'a pas de BaseTile.");
                continue;
            }

            List<List<int>> originalBorders = baseTile.cellType.borders.ToList();
            List<List<int>> rotatedBorders = RotateBordersList(originalBorders, rotation);

            Debug.Log($"--- TEST TILE: {tilePrefab.name}, Rotation: {rotation} ---");
            Debug.Log($"Tile Borders (rotated): {string.Join(" | ", rotatedBorders.Select(b => string.Join("-", b)))}");
            Debug.Log($"Target Borders        : {string.Join(" | ", borderList.Select(b => string.Join("-", b)))}");

            bool isMatch = true;

            for (int i = 0; i < borderList.Count; i++)
            {
                List<int> target = borderList[i];
                List<int> candidate = rotatedBorders[i];

                //Debug.Log($"Comparing side {i}: Candidate {string.Join("-", candidate)} | Target {string.Join("-", target)}");

                bool same = candidate.SequenceEqual(target);
                bool intersects = candidate.Intersect(target).Any();

                if (!same && !intersects)
                {
                   // Debug.LogWarning($"Mismatch on side {i}");
                    isMatch = false;
                    break;// break only the for loop
                }

               // Debug.Log($"Match on side {i}");
            }

            if (isMatch)
            {
                Debug.LogWarning($"Matched tile: {tilePrefab.name} with rotation {rotation}");
                //return (tilePrefab, rotation);
                ValidTilesList.Add((tilePrefab,rotation));
            }
        }

        if (ValidTilesList.Count > 0)
        {
            var randomNbr = Random.Range(0, ValidTilesList.Count);
            return (ValidTilesList[randomNbr].Item1,ValidTilesList[randomNbr].Item2);
        }
        else
        {
            Debug.LogWarning("No matching tile found.");
            return (null, -1);
        }
    }
}
