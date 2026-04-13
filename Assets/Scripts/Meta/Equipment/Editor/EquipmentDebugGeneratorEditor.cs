using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentDebugGenerator))]
public class EquipmentDebugGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EquipmentDebugGenerator generator = (EquipmentDebugGenerator)target;

        if (GUILayout.Button("Generate Test Item"))
        {
            generator.GenerateTestItem();
            EditorUtility.SetDirty(generator);
        }
    }
}
