using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Noise), true)]
[CanEditMultipleObjects]
public class NoiseEditor : Editor
{
    protected Noise noise;
    bool showPreview;

    protected virtual void OnEnable()
    {
        noise = target as Noise;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        if (GUILayout.Button("Save Texture"))
            noise.SaveTexture();

        showPreview = EditorGUILayout.Foldout(showPreview, "Preview", true);
        if (showPreview)
        {
            noise.CalculatePreview();
            GUILayout.Box(noise.previewRT);
        }
    }
}
