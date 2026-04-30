using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "UI_Group_Shop", menuName = "UI/ShopGroup", order = 1)]
public class SO_ShopTileCategory : ScriptableObject
{
    public SO_ShopSlot[] ShopArray;
}
