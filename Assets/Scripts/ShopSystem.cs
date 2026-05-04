using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance;

    private void Awake() => Instance = this;

    public GameObject currentSelectedPrefab;

    public void SelectItem(SO_ShopSlot slot)
    {
        currentSelectedPrefab = slot.Go_Tile;
        Debug.Log("Selected tile: " + slot.name);
        PreviewSystem.Instance.SetPreview(slot.Go_Tile);
    }
    
}