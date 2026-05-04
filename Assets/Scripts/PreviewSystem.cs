using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    public static PreviewSystem Instance;

    private GameObject ghostInstance;
    private GameObject currentPrefab;
    private float currentRotation = 0f;

    //getters
    public GameObject GetCurrentPrefab() => currentPrefab;

    private void Awake() => Instance = this;

    public void SetPreview(GameObject prefab)
    {
        ClearPreview();

        currentPrefab = prefab;

        ghostInstance = Instantiate(prefab);
       var meshColl =  ghostInstance.GetComponent<MeshCollider>();
        Destroy(meshColl);
        SetTransparent(ghostInstance, 0.1f);
    }

    public void UpdatePreview(Vector3 worldPos)
    {
        if (ghostInstance == null) return;

        var wfc = GameManager.Instance.wfc;
        Vector3 hex = wfc.FindClosestHex(worldPos);

        ghostInstance.transform.position = wfc.HexToWorldPosition(hex);
        ghostInstance.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    public void Rotate(float dir)
    {
        if (ghostInstance == null) return;

        currentRotation += dir * 60f;
    }

    public void ClearPreview()
    {
        if (ghostInstance != null)
            Destroy(ghostInstance);

        ghostInstance = null;
        currentPrefab = null;
        currentRotation = 0f;
    }

   

    private void SetTransparent(GameObject obj, float alpha)//doesn't work
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;

                mat.SetFloat("_Mode", 3); // Transparent (Standard shader)
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }
}