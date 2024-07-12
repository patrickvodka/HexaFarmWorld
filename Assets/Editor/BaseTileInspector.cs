using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BaseTile))]
public class BaseTileInspector : Editor
{
    public override void OnInspectorGUI()
    {
        BaseTile baseTile = (BaseTile)target;

        // Afficher les propriétés de BaseTile
        DrawDefaultInspector();

        if (baseTile.ceilClass != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CeilClass Details", EditorStyles.boldLabel);

            EditorGUILayout.Vector3Field("Hex Coord", baseTile.ceilClass.hexCoord);

            baseTile.ceilClass.isCollapsed = EditorGUILayout.Toggle("Is Collapsed", baseTile.ceilClass.isCollapsed);
            baseTile.ceilClass.selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Selected Tile", baseTile.ceilClass.selectedPrefab, typeof(SO_TileType), false);

            // Afficher les types de tuiles possibles
            EditorGUILayout.LabelField("Possible Tiles:", EditorStyles.boldLabel);
            
        }
        else
        {
            EditorGUILayout.HelpBox("CeilClass is not initialized.", MessageType.Warning);
        }
    }
}