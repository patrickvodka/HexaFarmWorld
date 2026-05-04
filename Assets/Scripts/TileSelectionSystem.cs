using UnityEngine;

public class TileSelectionSystem : MonoBehaviour
{
    public static TileSelectionSystem Instance;

    private void Awake() => Instance = this;

    private BaseTile current;

    public void Select(BaseTile tile)
    {
        current = tile;
        Debug.Log("Tile clicked on " + tile.name);
    }
}