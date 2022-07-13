using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using cosmicpotato.noisetools.Editor;

[CustomEditor(typeof(MarchingCubes))]
[CanEditMultipleObjects]
public class MarchingCubesEditor : Editor
{
    MarchingCubes mc;
    SerializedProperty n;

    private void OnEnable()
    {
        mc = target as MarchingCubes;
        n = serializedObject.FindProperty("noise");
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

        GUILayout.Space(10);
        EditorGUILayout.PropertyField(n);

        // End the code block and update the label if a change occurred
        if (EditorGUI.EndChangeCheck())
        {
            if (mc.realtimeGeneration)
                mc.GenerateMesh();
        }
    }
}
