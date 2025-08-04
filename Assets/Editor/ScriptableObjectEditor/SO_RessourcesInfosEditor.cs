 using UnityEngine;
using UnityEditor;
//[]
[CustomEditor(typeof(SO_RessourcesInfos))]
public class SO_RessourcesInfosEditor : Editor
{
    
    SerializedProperty cost;
    private E_Ressources.RessourceType[] RessourcesTypeEnum;
    //private E_Ressources.RessourceType ressourcesType;
    void OnEnable()
    {
        cost = serializedObject.FindProperty("Cost");
        RessourcesTypeEnum = (E_Ressources.RessourceType[])System.Enum.GetValues(typeof(E_Ressources.RessourceType));
        
    }
   

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        int size = RessourcesTypeEnum.Length;
        

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Ressources", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Cost", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < size; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{RessourcesTypeEnum[i]}", GUILayout.MaxWidth(100));
            EditorGUILayout.PropertyField(cost.GetArrayElementAtIndex(i), GUIContent.none);

            EditorGUILayout.EndHorizontal();
        }


        serializedObject.ApplyModifiedProperties();
    }
}
