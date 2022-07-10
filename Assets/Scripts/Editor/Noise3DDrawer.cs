using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Noise3D), true)]
public class Noise3DDrawer : NoiseDrawer
{
    protected override float GetExpandedHeight(SerializedProperty prop, SerializedObject serializedObject, Noise config, float totalHeight)
    {
        totalHeight = base.GetExpandedHeight(prop, serializedObject, config, totalHeight);
        if (config.showPreview)
            totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
        return totalHeight;
    }

    protected override float PreviewTextureGUI(Rect position, float y, Noise config)
    {
        y = base.PreviewTextureGUI(position, y, config);

        if (config.showPreview)
        {
            Noise3D noise3D = config as Noise3D;
            EditorGUI.indentLevel++;
            noise3D.axis = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Axis", noise3D.axis, 0, 2);
            y = NextLine(y);
            noise3D.layer = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Layer", noise3D.layer, 1, noise3D.resolution);
            y = NextLine(y);
            EditorGUI.indentLevel--;
        }

        return y;
    }
}