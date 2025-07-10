using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellType
{
    public string name;          // Nom du type de tuile
    public GameObject prefab;    // Préfabriqué de la tuile
    public int tier; // tier de 0 à 5 pour les constructions
    public E_BiomeType.BiomeType biome; // current used biome for this tile
    [Tooltip("1 start from the Left top one , and it goes clockwise")]
    public List<int>[] borders = new List<int>[6]; // Liste de configurations de bordures pour les 6 côtés
    
    public CellType()
    {
        // Initialiser les 6 listes d'entiers
        for (int i = 0; i < 6; i++)
        {
            borders[i] = new List<int>();
        }
    }
}