using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeightMap))]
[CanEditMultipleObjects]
public class HeightMapEditor : Editor
{
    HeightMap hm;

    private void OnEnable()
    {
        hm = target as HeightMap;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        EditorGUI.BeginDisabledGroup(hm.realtimeGeneration);
        if (GUILayout.Button("Generate Mesh"))
            hm.GenerateMesh();
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Clear Mesh"))
            hm.ClearMesh();

        // End the code block and update the label if a change occurred
        if (EditorGUI.EndChangeCheck())
        {
            if (hm.realtimeGeneration)
                hm.GenerateMesh();
        }
    }


}
