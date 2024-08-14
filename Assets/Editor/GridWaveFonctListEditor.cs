using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

[CustomEditor(typeof(GridWaveFonctList))]
public class GridWaveFonctListEditor : Editor
{
    private GridWaveFonctList _gridWaveFonctList;
    private int[] borders = new int[6];
    private void OnEnable()
    {
        _gridWaveFonctList = (GridWaveFonctList)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.BeginHorizontal();
        
        for (int i = 0; i < 6; i++)
        {
            EditorGUILayout.LabelField($"Border {i}:", EditorStyles.boldLabel, GUILayout.Width(60));
            borders[i] = EditorGUILayout.IntField(borders[i], GUILayout.Width(25));
            
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Afficher Dictionnaire des Hashes"))
        {
            ShowTileHashes();
        }
        if (GUILayout.Button("Afficher Dictionnaire précis de Hashes"))
        {
            
            BigInteger test = _gridWaveFonctList.GetBorderHash(this.borders);
            ShowLastHashes(test);
            //int[] borders = _gridWaveFonctList.ReverseHash(test, 6);
            //Debug.LogError($"Les bordures retrouvées sont : {string.Join(", ", borders)}");

        }
    }

    private void ShowLastHashes(BigInteger currentHash)
    {
        var tileHashes = _gridWaveFonctList.hashToTileMap;
        List<(GameObject, int)> checkList = new List<(GameObject, int)>();
        if (tileHashes.TryGetValue(currentHash, out checkList))
        {
            if (tileHashes == null || tileHashes.Count == 0)
            {
                Debug.Log("Aucun hash trouvé.");
                return;
            }
            Debug.Log($"Hash: {currentHash}");
            foreach (var kvp in checkList)
            {
                    GameObject tilePrefab = kvp.Item1;
                    int rotation = kvp.Item2;

                    if (tilePrefab != null)
                    {
                        Debug.Log($"Prefab: {tilePrefab.name}, Rotation: {rotation}");
                    }
                    else
                    {
                        Debug.LogWarning("Prefab est nul.");
                    }
                
            }
        }else{Debug.Log("recherche de list raté ");}
    }

    private void ShowTileHashes()
    {
        var tileHashes = _gridWaveFonctList.hashToTileMap;

        if (tileHashes == null || tileHashes.Count == 0)
        {
            Debug.Log("Aucun hash trouvé.");
            return;
        }

        foreach (var kvp in tileHashes)
        {
            BigInteger hash = kvp.Key;
            List<(GameObject tilePrefab, int rotation)> tileList = kvp.Value;
            int[] fakeBorder = new int[6];
           fakeBorder =  _gridWaveFonctList.ReverseHash(hash, 6);
           Debug.LogWarning( hash+" border : "+ string.Join(",", fakeBorder));
            //Debug.Log($"Hash: {hash}");

            foreach (var tileInfo in tileList)
            {
                GameObject tilePrefab = tileInfo.tilePrefab;
                int rotation = tileInfo.rotation;

                if (tilePrefab != null)
                {
                    Debug.Log($"Prefab: {tilePrefab.name}, Rotation: {rotation}");
                }
                else
                {
                    Debug.LogWarning("Prefab est nul.");
                }
            }
        }
    }
}