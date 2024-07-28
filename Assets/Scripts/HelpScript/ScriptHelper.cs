using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptHelper : MonoBehaviour
{
    
/*
    void Start()
    {
        foreach (var config in baseTileTest.cellType.borders)
        {
            Debug.Log($"Configuration 1 : {string.Join(", ", config)}");
        }
        baseTileTest.ReturnInput(); 
        foreach (var config in baseTileTest.cellType.borders)
        {
            Debug.Log($"Configuration 2 : {string.Join(", ", config)}");
        }
        // Génération des configurations
        var allConfigurations = GenerateConfigurations(baseTileTest);

        // Affichage des configurations
        foreach (var config in allConfigurations)
        {
            Debug.Log($"Configuration: {string.Join(", ", config)}");
        }
    }*/

   public static List<List<int>> GenerateConfigurations(List<List<int>> ListConfig)
    {
        var result = new List<List<int>>();
        if (ListConfig.Count > 0)
        {
            GenerateConfigurationsRecursive(ListConfig, 0, new List<int>(), result);
            return result;
        }
        return result;
    }

   public  static void  GenerateConfigurationsRecursive(List<List<int>> lists, int index, List<int> current, List<List<int>> result)
    {
        if (index == lists.Count)
        {
            // Ajouter la configuration actuelle à la liste des résultats
            result.Add(new List<int>(current));
            return;
        }

        foreach (var value in lists[index])
        {
            current.Add(value);
            GenerateConfigurationsRecursive(lists, index + 1, current, result);
            current.RemoveAt(current.Count - 1);
        }
    }
}