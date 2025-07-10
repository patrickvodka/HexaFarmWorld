using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TileType", menuName = "Tiles/TileType", order = 1)]
public class SO_TileType : ScriptableObject
{
    public GameObject[] ceil;
    public string[] tileName;
    public E_BiomeType.BiomeType biome;
    
    public GameObject GetTileTypeByBiome(E_BiomeType.BiomeType biome)
    {
        return ceil.FirstOrDefault(t => t.GetComponent<BaseTile>().cellType.biome == biome);
    }
}