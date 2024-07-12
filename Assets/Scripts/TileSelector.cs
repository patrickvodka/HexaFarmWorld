using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public Hexagone3dVec hexGridGenerator;
    public float distanceTolerance = 0.01f; // Tolérance pour considérer deux distances comme égales

    void Start()
    {
        hexGridGenerator = GetComponent<Hexagone3dVec>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectTileUnderMouse();
        }
    }

    void DetectTileUnderMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero); // Plan horizontal

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            bool canAct;
            Vector3 hexCoord = FindNearestHex(worldPosition, out canAct);

            if (canAct)
            {
                if (hexGridGenerator.hexGrid.ContainsKey(hexCoord))
                {
                    GameObject tile = hexGridGenerator.hexGrid[hexCoord];
                    Debug.Log("Tuile sélectionnée : " + tile.name);
                    Destroy(tile);
                    hexGridGenerator.hexGrid[hexCoord] = Instantiate(hexGridGenerator.BaseGrass, hexGridGenerator.HexToWorldPosition(hexCoord), Quaternion.identity, hexGridGenerator.transform);
                }
                else
                {
                    Debug.Log("Aucune tuile trouvée à cette position.");
                }
            }
            else
            {
                Debug.Log("Aucun hexagone unique trouvé à cette position.");
            }
        }
    }

    Vector3 FindNearestHex(Vector3 worldPosition, out bool canAct)
    {
        float minDist = float.MaxValue; // Initialisation de la distance minimale à une valeur très élevée
        Vector3 nearestHex = Vector3.zero; // Initialisation de la variable pour stocker les coordonnées de l'hexagone le plus proche
        float secondMinDist = float.MaxValue; // Initialisation de la deuxième plus petite distance

        foreach (var kvp in hexGridGenerator.hexGrid)
        {
            // Calcul de la distance entre la position du clic et la position du centre de l'hexagone courant
            float dist = Vector3.Distance(worldPosition, kvp.Value.transform.position);

            if (dist < minDist)
            {
                // Mise à jour de la deuxième plus petite distance avant de mettre à jour la distance minimale
                secondMinDist = minDist;
                minDist = dist;
                nearestHex = kvp.Key;
            }
            else if (dist < secondMinDist)
            {
                secondMinDist = dist;
            }
        }

        // Vérifier si la distance minimale et la deuxième plus petite distance sont presque égales
        if (Mathf.Abs(minDist - secondMinDist) < distanceTolerance)
        {
            canAct = false; // Aucune sélection si les deux distances sont trop proches
        }
        else
        {
            canAct = true;
        }

        return nearestHex;
    }
}
