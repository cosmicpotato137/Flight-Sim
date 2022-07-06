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

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        float y = base.GetPropertyHeight(property, label);

        if (property.isExpanded && property.objectReferenceValue)
        {
            Noise3D noise3D = property.objectReferenceValue as Noise3D;
            noise3D.axis = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Axis", noise3D.axis, 0, 2);
            y = NextLine(y);
            noise3D.layer = EditorGUI.IntSlider(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Layer", noise3D.layer, 1, noise3D.resolution);
            y = NextLine(y);
        }
    }

    //protected override float PreviewTextureGUI(Rect position, float y, Noise config)
    //{
    //    y = base.PreviewTextureGUI(position, y, config);

    //    return y;
    //}
}