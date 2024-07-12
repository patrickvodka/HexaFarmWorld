using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AllTiles", menuName = "Tiles/AllTiles", order = 1)]
public class SO_AllTiles : ScriptableObject
{
    public SO_TileType[] ceil;
    public string[] tileSetName;
}
