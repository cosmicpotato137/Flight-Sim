using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Noise3D), true)]
public class Noise3DDrawer : NoiseDrawer
{
    protected override float GetExpandedHeight(SerializedProperty prop, SerializedObject serializedObject, Noise config, float totalHeight)
    {
        totalHeight = base.GetExpandedHeight(prop, serializedObject, config, totalHeight);
        totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
        return totalHeight;
    }
    protected override float PreviewTextureGUI(Rect position, float y, Noise config)
    {
        y = base.PreviewTextureGUI(position, y, config);
        Noise3D noise3D = config as Noise3D;

        noise3D.axis = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Axis", noise3D.axis, 0, 2);
        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        noise3D.layer = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Layer", noise3D.layer, 1, config.resolution);
        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        return y;
    }
}