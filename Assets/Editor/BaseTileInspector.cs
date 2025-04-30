using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseTile))]
public class BaseTileInspector : Editor
{
    // Déclare une variable pour stocker la position de défilement
    private Vector2 scrollPosition;
    private bool _scrollMode = false;
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
        

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button($"Scroll Mode"))
        {
            _scrollMode = !_scrollMode;
        }
        EditorGUILayout.LabelField("Borders :", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (_scrollMode)
        {
            // Début de la zone défilante
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(110));
        }
        // Affichez les données des bordures en forme de grille
        
        for (int i = 0; i < baseTile.cellType.borders.Length; i++)
        {
            // Assurez-vous qu'il y a au moins un item avec une valeur par défaut
            if (baseTile.cellType.borders[i].Count == 0)
            {
                baseTile.cellType.borders[i].Add(0);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box); // Crée un conteneur avec une bordure
            EditorGUILayout.LabelField($"Border {i + 1}", EditorStyles.boldLabel, GUILayout.Width(100));
            baseTile.ReturnInput();
            EditorGUILayout.Space();

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

        if (_scrollMode)
        {
            // Fin de la zone défilante
            EditorGUILayout.EndScrollView();
        }

        // Ajoutez un bouton de validation et de sauvegarde
        if (GUILayout.Button("Validate and Save"))
        {
            baseTile.SaveInput();
        }
        
        

        // Marquez l'objet comme modifié pour que Unity enregistre les modifications
        if (GUI.changed)
        {
            baseTile.SaveInput();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}
