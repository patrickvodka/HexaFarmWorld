
using System.Collections.Generic;
using UnityEngine;

public class BaseTile : MonoBehaviour
{
    public CellType cellType;
    public CeilClass ceilClass;

    // Méthode pour initialiser la tuile
    public void Initialize(Vector3 hexCoord, List<GameObject> allTileTypes)
    {
        if (ceilClass == null || ceilClass.hexCoord == Vector3.zero)
        {
            ceilClass = new CeilClass(hexCoord);
        }

        cellType.prefab = transform.gameObject;
    }


    public void DisplayCeilClass()
    {
        Debug.Log($"Hex Coord: {ceilClass.hexCoord}");
        Debug.Log($"Is Collapsed: {ceilClass.isCollapsed}");
        Debug.Log($"Selected Tile: {ceilClass.selectedPrefab?.name}");
        
    }
}