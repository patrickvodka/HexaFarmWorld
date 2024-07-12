using UnityEngine;

[CreateAssetMenu(fileName = "TileType", menuName = "Tiles/TileType", order = 1)]
public class SO_TileType : ScriptableObject
{
    public GameObject[] ceil;
    public string[] tileName;
}