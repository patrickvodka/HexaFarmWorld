using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "UI_ShopSlot", menuName = "UI/ShopSlot", order = 1)]
public class SO_ShopSlot : ScriptableObject
{
    public string slotName;
    public Texture texture;
    public GameObject Go_Tile;
    public int[] Cost;
    public E_Ressources.RessourceType[] RessourceType;
}
