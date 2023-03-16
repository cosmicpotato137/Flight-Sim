using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeightMap))]
[CanEditMultipleObjects]
public class HeightMapEditor : Editor
{
    HeightMap hm;
    SerializedProperty n;

    private void OnEnable()
    {
        hm = target as HeightMap;
        n = serializedObject.FindProperty("noise");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        EditorGUI.BeginDisabledGroup(hm.realtimeGeneration);
        if (GUILayout.Button("Generate Mesh"))
            hm.GenerateOnce();
        EditorGUI.EndDisabledGroup();

        bool changed = EditorGUI.EndChangeCheck();

        if (GUILayout.Button("Clear Mesh"))
            hm.ClearMesh();


        EditorGUI.BeginChangeCheck();
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(n);

        // End the code block and update the label if a change occurred
        if (changed || EditorGUI.EndChangeCheck())
        {
            if (hm.realtimeGeneration)
                hm.GenerateOnce();
        }
    }
}
