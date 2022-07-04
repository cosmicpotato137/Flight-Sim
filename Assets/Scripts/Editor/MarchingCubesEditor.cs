using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubes))]
[CanEditMultipleObjects]
public class MarchingCubesEditor : Editor
{
    MarchingCubes mc;

    private void OnEnable()
    {
        mc = target as MarchingCubes;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        EditorGUI.BeginDisabledGroup(mc.realtimeGeneration);
        if (GUILayout.Button("Generate Mesh"))
            mc.GenerateMesh();
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Clear Mesh"))
            mc.ClearMesh();

        // End the code block and update the label if a change occurred
        if (EditorGUI.EndChangeCheck())
        {
            if (mc.realtimeGeneration)
                mc.GenerateMesh();
        }
    }

    
}
