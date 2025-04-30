using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagone3dVec : MonoBehaviour
{
    public float height = 60;  // hauteur de l'hexagone
    public float width = 69;   // largeur de l'hexagone
    public float radiusMap = 5;
    public GameObject BaseGrass;
    public List<Vector3> globalPos = new List<Vector3>();
    public List<GameObject> prefab = new List<GameObject>();
    public Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();
    public Dictionary<Vector3, Vector3> worldToHexMap = new Dictionary<Vector3, Vector3>(); // Mapping position mondiale -> coordonnées hexagonales

    void Start()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            Destroy(child);
        }
        globalPos.Clear();
        prefab.Clear();
        hexGrid.Clear();
        StartCoroutine(GenerateTilesWithDelay());
    }

    private IEnumerator GenerateTilesWithDelay()
    {
        for (int q = -Mathf.CeilToInt(radiusMap); q <= Mathf.CeilToInt(radiusMap); q++)
        {
            for (int r = -Mathf.CeilToInt(radiusMap); r <= Mathf.CeilToInt(radiusMap); r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= radiusMap)
                {
                    Vector3 hexCoord = new Vector3(q, r, s);
                    Vector3 worldPosition = HexToWorldPosition(hexCoord);

                    if (!hexGrid.ContainsKey(hexCoord))
                    {
                        GameObject currentTile = Instantiate(BaseGrass, worldPosition, Quaternion.identity, gameObject.transform);
                        hexGrid.Add(hexCoord, currentTile);
                        globalPos.Add(hexCoord);
                        prefab.Add(currentTile);
                        worldToHexMap.Add(worldPosition, hexCoord); // Ajouter au mapping
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
    }

    public Vector3 HexToWorldPosition(Vector3 hexCoord)
    {
        float x = width * (hexCoord.x + hexCoord.z / 2.0f);
        float z = height * hexCoord.z;
        return new Vector3(x, 0, z);
    }

    public void ReplaceTile(Vector3 hexCoord, SO_TileType newSoTileType)
    {
        if (hexGrid.ContainsKey(hexCoord))
        {
            Destroy(hexGrid[hexCoord]);
            Vector3 worldPosition = HexToWorldPosition(hexCoord);
            GameObject newTile = Instantiate(newSoTileType.ceil[0], worldPosition, Quaternion.identity, gameObject.transform);
            hexGrid[hexCoord] = newTile;
        }
    }
}
