using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class MeshCombiner : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Combine + Save Mesh Asset")]
    public void CombineAndSave()
    {
        Combine();

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        string path = "Assets/CombinedMeshes/" + gameObject.name + ".asset";

        System.IO.Directory.CreateDirectory("Assets/CombinedMeshes");

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh sauvegardé : " + path);
    }
#endif
    private void Start()
    {
        //if (GetComponent<MeshFilter>().sharedMesh == null)
          //  StartCoroutine(Delay(2));
    }

     public IEnumerator Delay(float delay)
    {
        Debug.Log("start delay");
        Combine();
        yield return new WaitForSeconds(delay);
        var obj = transform.GetChild(1);
        Debug.Log(obj);
        Destroy(obj);
        
        
    } 
    public void Combine()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);

        Dictionary<Material, List<CombineInstance>> materialMap = new();

        Matrix4x4 rootMatrix = transform.worldToLocalMatrix;

        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;
            if (mf.transform == transform) continue; // skip root

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null || !mr.enabled) continue;

            Mesh mesh = mf.sharedMesh;
            Material[] materials = mr.sharedMaterials;

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                Material mat = (sub < materials.Length) ? materials[sub] : materials[0];

                if (!materialMap.TryGetValue(mat, out var list))
                {
                    list = new List<CombineInstance>();
                    materialMap.Add(mat, list);
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = sub,
                    // TRANSFORM CORRECT (important pour prefab + WFC)
                    transform = rootMatrix * mf.transform.localToWorldMatrix
                };

                list.Add(ci);
            }

            // Disable renderer (plus safe que SetActive)
            mr.enabled = false;

            // Optionnel : disable collider
            var col = mf.GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        // Création mesh final avec submeshes (1 par matériau)
        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Material> finalMaterials = new();
        List<CombineInstance> finalCombine = new();

        int subMeshIndex = 0;

        foreach (var kvp in materialMap)
        {
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

            finalCombine.Add(new CombineInstance
            {
                mesh = subMesh,
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            });

            finalMaterials.Add(kvp.Key);
            subMeshIndex++;
        }

        finalMesh.CombineMeshes(finalCombine.ToArray(), false, false);
        finalMesh.RecalculateBounds();
        finalMesh.RecalculateNormals();
        // Assignation root
        MeshFilter rootMF = GetComponent<MeshFilter>();
        if (!rootMF) rootMF = gameObject.AddComponent<MeshFilter>();

        MeshRenderer rootMR = GetComponent<MeshRenderer>();
        if (!rootMR) rootMR = gameObject.AddComponent<MeshRenderer>();

        rootMF.sharedMesh = finalMesh;
        rootMR.sharedMaterials = finalMaterials.ToArray();

        // IMPORTANT : marquer static si map fixe
        //gameObject.isStatic = true;
        Debug.Log($"Combine terminé : {finalMaterials.Count} matériaux, {finalMesh.vertexCount} vertices");
    }
}