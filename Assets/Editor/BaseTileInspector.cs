using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseTile))]
public class BaseTileInspector : Editor
{
    public override void OnInspectorGUI()
    {
        BaseTile baseTile = (BaseTile)target;

        // Affichez les champs de l'Inspector par défaut
        DrawDefaultInspector();

        if (baseTile.cellType == null)
        {
            EditorGUILayout.HelpBox("CellType is null", MessageType.Error);
            return;
        }

        // Assurez-vous que le tableau de bordures a 6 éléments
        if (baseTile.cellType.borders.Length != 6)
        {
            EditorGUILayout.HelpBox("The borders array must have exactly 6 elements.", MessageType.Error);
            return;
        }

        // Affichez les données des bordures en forme de grille
        EditorGUILayout.LabelField("Borders", EditorStyles.boldLabel);

        for (int i = 0; i < baseTile.cellType.borders.Length; i++)
        {
            // Assurez-vous qu'il y a au moins un item avec une valeur par défaut
            if (baseTile.cellType.borders[i].Count == 0)
            {
                baseTile.cellType.borders[i].Add(0);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box); // Crée un conteneur avec une bordure
            EditorGUILayout.LabelField($"Border {i + 1}", EditorStyles.boldLabel, GUILayout.Width(100));

            // Affichez chaque item de chaque border en horizontal
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < baseTile.cellType.borders[i].Count; j++)
            {
                baseTile.cellType.borders[i][j] = EditorGUILayout.IntField(baseTile.cellType.borders[i][j], GUILayout.Width(40));
            }
            EditorGUILayout.EndHorizontal();

            // Ajoutez des boutons pour ajouter et retirer des items
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item", GUILayout.Width(80)))
            {
                baseTile.cellType.borders[i].Add(0);
            }
            if (baseTile.cellType.borders[i].Count > 1 && GUILayout.Button("Remove Last Item", GUILayout.Width(120)))
            {
                baseTile.cellType.borders[i].RemoveAt(baseTile.cellType.borders[i].Count - 1);
            }
            EditorGUILayout.EndHorizontal();

            // Ajoutez un trait de séparation
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Marquez l'objet comme modifié pour que Unity enregistre les modifications
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
