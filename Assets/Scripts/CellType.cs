using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellType
{
    public string name;          // Nom du type de tuile
    public GameObject prefab;    // Préfabriqué de la tuile
    public int tier; // tier de 0 à 5 pour les constructions
    [Tooltip("1 start from the Left top one , and it goes clockwise")]
    public int[] borders = new int[6];  // Types de bordures pour les 6 côtés
}