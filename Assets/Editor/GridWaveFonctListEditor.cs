using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

[CustomEditor(typeof(GridWaveFonctList))]
public class GridWaveFonctListEditor : Editor
{
    private GridWaveFonctList _gridWaveFonctList;

    private void OnEnable()
    {
        _gridWaveFonctList = (GridWaveFonctList)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Afficher Dictionnaire des Hashes"))
        {
            ShowTileHashes();
        }
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

            Debug.Log($"Hash: {hash}");

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