using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class TilePlacementSystem : MonoBehaviour
{
    public static TilePlacementSystem Instance;

    private void Awake() => Instance = this;

    public void Place(Vector3 worldPos)
    {
        var wfc = GameManager.Instance.wfc;

        Vector3 hex = wfc.FindClosestHex(worldPos);

        if (!wfc.HexGridDictionary.TryGetValue(hex, out var tile))
            return;

        ReplaceTile(hex, tile);
    }

    void ReplaceTile(Vector3 coord, GameObject oldTile)
    {
        var prefab = PreviewSystem.Instance.GetCurrentPrefab();

        if (prefab == null) return;

        Destroy(oldTile);
        var wfc = GameManager.Instance.wfc;
        Vector3 newPos = GameManager.Instance.wfc.HexToWorldPosition(coord);
        var newTile = Instantiate(prefab, newPos, Quaternion.identity,wfc.transform);
        if(newTile.TryGetComponent<BaseTile>(out var tileInfo))
        {
            tileInfo.cellType.prefab = prefab;
            tileInfo.Initialize(coord);
            tileInfo.ceilClass = new CeilClass(coord);
        }


        GameManager.Instance.wfc.HexGridDictionary[coord] = newTile;
    }
}