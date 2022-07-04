using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VolumeViewer))]
[CanEditMultipleObjects]
public class VolumeViewerEditor : Editor
{
    VolumeViewer volumeViewer;
    SerializedProperty slicer;
    SerializedProperty layer;
    SerializedProperty axis;
    SerializedProperty volume;

    private void OnEnable()
    {
        volumeViewer = (VolumeViewer)target;
        slicer = serializedObject.FindProperty("volumeSliceShader");
        layer = serializedObject.FindProperty("layer");
        axis = serializedObject.FindProperty("axis");
        volume = serializedObject.FindProperty("volume");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(slicer);
        EditorGUILayout.PropertyField(volume);

        if (volumeViewer.volume && volumeViewer.volume.IsCreated())
        {
            EditorGUILayout.IntSlider(layer, 1, volumeViewer.volume.volumeDepth);
            EditorGUILayout.IntSlider(axis, 0, 2);
            GUILayout.Box(new GUIContent(volumeViewer.slice));
            
            volumeViewer.CreateSlicer();
            volumeViewer.Slice();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
