using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Si tu utilises le new Input System
//[]
public class HexaTilePainter : MonoBehaviour
{

    public GameObject tileToPlace;
    public KeyCode placementKey = KeyCode.Mouse0;
    [HideInInspector] public bool canTakeInput = true;
    public HexaWaveFonctCollapse wfc;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(placementKey))
        {
            if (canTakeInput)
            {
                if (!IsPointerOverUI())
                {
                    TryPlaceTileUnderCursor();
                    Debug.Log("mouse click");
                }
                else
                {
                    Debug.Log("mouse over UI");
                    TryPlaceTileUnderCursor();
                }
            }
            else
            {
                Debug.Log("input was blocked from here");
            }
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }



    void TryPlaceTileUnderCursor()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);
        if (results.Capacity != 0)
        {
            foreach (RaycastResult result in results)
            {
                Debug.Log("UI touché : " + result.gameObject.name);
                IClickable clickTarget = result.gameObject.GetComponent<IClickable>();
                clickTarget?.OnSelected();
            }

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.LogWarning(hit.transform.gameObject);
                IClickable clickTarget = hit.transform.gameObject.GetComponent<IClickable>();
                clickTarget?.OnSelected();
                if (LayerMask.NameToLayer("UI") != hit.transform.gameObject.layer)
                {
                    Debug.Log($" not same layer {LayerMask.NameToLayer("UI")} et {hit.transform.gameObject.layer}");
                }
                else
                {

                    Debug.Log($" same layer {LayerMask.NameToLayer("UI")} et {hit.transform.gameObject.layer}");

                    Vector3 clickPos = hit.point;

                    // Trouver la tile la plus proche
                    Vector3 closestHex = FindClosestHex(clickPos);

                    if (wfc.HexGridDictionary.TryGetValue(closestHex, out GameObject existingTile))
                    {
                        // Détruire l’ancienne
                        Destroy(existingTile);

                        // Calculer position monde
                        Vector3 worldPos = wfc.HexToWorldPosition(closestHex);

                        // Instancier la nouvelle tile
                        GameObject newTile = Instantiate(tileToPlace, worldPos, Quaternion.identity, transform);

                        BaseTile baseTile = newTile.GetComponent<BaseTile>();
                        if (baseTile != null)
                        {
                            baseTile.Initialize(closestHex);
                            baseTile.ReturnInput();
                        }

                        // Mise à jour du dictionnaire
                        wfc.HexGridDictionary[closestHex] = newTile;

                        Debug.Log($"Tile remplacée à {closestHex} par {tileToPlace.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Aucune tile trouvée à cette position.");
                    }
                }
            }
        }

        Vector3 FindClosestHex(Vector3 worldPos)
        {
            Vector3 closest = Vector3.zero;
            float minDist = float.MaxValue;

            foreach (Vector3 hexCoord in wfc.HexGridDictionary.Keys)
            {
                Vector3 hexWorldPos = wfc.HexToWorldPosition(hexCoord);
                float dist = Vector3.Distance(worldPos, hexWorldPos);
                if (dist < minDist)
                {
                    closest = hexCoord;
                    minDist = dist;
                }
            }

            return closest;
        }
    }
}
