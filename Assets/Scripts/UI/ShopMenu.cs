using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour
{
    public SO_ShopTileCategory shopTileCategory;
    public GameObject shopSlotPrefab;
    void Start()
    {
        CreateSlotShop();
    }

    private void CreateSlotShop()
    {
        for (int i = 0; i < shopTileCategory.ShopArray.Length; i++)
        {
            var spawnSlot = Instantiate(shopSlotPrefab, transform);
            if (spawnSlot.TryGetComponent<RawImage>(out RawImage rawImageComp))
            {
                if (shopTileCategory.ShopArray[i].texture != null && shopTileCategory.ShopArray[i].texture != null)
                {
                    rawImageComp.texture = shopTileCategory.ShopArray[i].texture;
                }
                else
                {
                    Debug.LogWarning($"Texture in ShopSlot == null in {i} case: {shopTileCategory.ShopArray[i].name}");
                }
            }
            else
            {
                Debug.LogWarning($"Texture in RawImamge == null in {i} case: {shopTileCategory.ShopArray[i].name}");
            }

            if (spawnSlot.TryGetComponent<BuyImage>(out BuyImage buyImageComp))
            {
                if (buyImageComp != null && shopTileCategory.ShopArray[i] != null)
                {
                    buyImageComp.shopSlot = shopTileCategory.ShopArray[i];
                }
                else
                {
                    Debug.LogWarning($"buyImage in ShopSlot == null in {i} case: {shopTileCategory.ShopArray[i].name}");
                }
            }
            else
            {
                Debug.LogWarning($"BuyImage == null in {i} case: {shopTileCategory.ShopArray[i].name}");
            }

        }
    }
}
