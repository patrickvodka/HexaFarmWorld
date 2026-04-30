using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SO_ShopSlot))]
public class SO_ShopSlotEditor : Editor
{
    SerializedProperty cost;
    private E_Ressources.RessourceType[] ressourcesTypeEnum;

    void OnEnable()
    {
        cost = serializedObject.FindProperty("Cost");
        ressourcesTypeEnum = (E_Ressources.RessourceType[])System.Enum.GetValues(typeof(E_Ressources.RessourceType));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Dessine tout sauf ça
        DrawPropertiesExcluding(serializedObject, "m_Script", "Cost","RessourceType");
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        // EditorGUILayout.Space();

        int enumSize = ressourcesTypeEnum.Length;

        if (cost.arraySize != enumSize)
            cost.arraySize = enumSize;

        float lineHeight = EditorGUIUtility.singleLineHeight;

        Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);

        float labelWidth = 120f;
        float fieldWidth = 80f;
        float nbrFieldWidth = 45f;
        // Header
        EditorGUI.LabelField(
            new Rect(rect.x, rect.y, labelWidth, lineHeight),
            "Ressources", EditorStyles.boldLabel);

        EditorGUI.LabelField(
            new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
            "Cost", EditorStyles.boldLabel);

        // Rows
        for (int i = 0; i < enumSize; i++)
        {
            rect = EditorGUILayout.GetControlRect(false, lineHeight);

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, labelWidth, lineHeight),
                ressourcesTypeEnum[i].ToString());

            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth, rect.y, nbrFieldWidth, lineHeight),
                cost.GetArrayElementAtIndex(i),
                GUIContent.none);
        }

        serializedObject.ApplyModifiedProperties();
    }
}