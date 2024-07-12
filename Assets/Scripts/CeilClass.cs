using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CeilClass
{
    public Vector3 hexCoord;           // Coordonnées hexagonales
    public bool isCollapsed;           // Indicateur si la tuile est collapsée
    public GameObject selectedPrefab; // Tuile sélectionnée après collapse

    public CeilClass(Vector3 hexCoord)
    {
        this.hexCoord = hexCoord;
        this.isCollapsed = false;
        this.selectedPrefab = null;
    }

    
}