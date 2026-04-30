using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MeshCombiner : MonoBehaviour
{
    [ContextMenu("Combine Meshes (Multi Material)")]
    public void Combine()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);

        // Map : Material -> CombineInstances
        Dictionary<Material, List<CombineInstance>> materialMap = new Dictionary<Material, List<CombineInstance>>();

        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            Mesh mesh = mf.sharedMesh;
            Material[] materials = mr.sharedMaterials;

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                Material mat = materials.Length > sub ? materials[sub] : materials[0];

                if (!materialMap.ContainsKey(mat))
                    materialMap[mat] = new List<CombineInstance>();

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = sub;
                ci.transform = mf.transform.localToWorldMatrix;

                materialMap[mat].Add(ci);
            }

            // Désactiver après traitement
            if (mf.transform != transform)
                mf.gameObject.SetActive(false);
        }

        // Création des meshes par matériau
        List<Mesh> subMeshes = new List<Mesh>();
        List<Material> finalMaterials = new List<Material>();

        foreach (var kvp in materialMap)
        {
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

            subMeshes.Add(subMesh);
            finalMaterials.Add(kvp.Key);
        }

        // Combine final (multi-submesh)
        CombineInstance[] finalCombine = new CombineInstance[subMeshes.Count];

        for (int i = 0; i < subMeshes.Count; i++)
        {
            finalCombine[i] = new CombineInstance
            {
                mesh = subMeshes[i],
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            };
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(finalCombine, false, false); // IMPORTANT

        // Assignation
        MeshFilter rootMF = GetComponent<MeshFilter>();
        if (!rootMF) rootMF = gameObject.AddComponent<MeshFilter>();

        MeshRenderer rootMR = GetComponent<MeshRenderer>();
        if (!rootMR) rootMR = gameObject.AddComponent<MeshRenderer>();

        rootMF.sharedMesh = finalMesh;
        rootMR.sharedMaterials = finalMaterials.ToArray();

        Debug.Log($"Combine terminé : {finalMaterials.Count} matériaux");
    }
}