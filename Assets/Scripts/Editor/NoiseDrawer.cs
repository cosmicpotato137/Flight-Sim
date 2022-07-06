using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(Noise), true)]
public class NoiseDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
        {
            var data = property.objectReferenceValue as ScriptableObject;

            if (data == null) return EditorGUIUtility.singleLineHeight;
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();
            Noise config = property.objectReferenceValue as Noise;

            //catch (InvalidCastException)
            //{
            //    // do nothing

            //}
                

            totalHeight = GetExpandedHeight(prop, serializedObject, config, totalHeight);
            totalHeight += EditorGUIUtility.standardVerticalSpacing * 2;

        }
        return totalHeight;
    }

    protected virtual float GetExpandedHeight(SerializedProperty prop, SerializedObject serializedObject, Noise config, float totalHeight)
    {
        // Noise properties
        if (prop.NextVisible(true))
        {
            do
            {
                if (prop.name == "m_Script") continue;
                else
                {
                    var subProp = serializedObject.FindProperty(prop.name);
                    float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
                    totalHeight += height;
                }
            }
            while (prop.NextVisible(false));
        }

        // save button
        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // realtime editing
        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // preview window
        if (config.showPreview && config)
                totalHeight += config.previewRes + EditorGUIUtility.standardVerticalSpacing;
        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
            EditorGUI.PropertyField(new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property, GUIContent.none, true);
            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
            if (property.objectReferenceValue == null) EditorGUIUtility.ExitGUI();

            // get objects
            var data = (ScriptableObject)property.objectReferenceValue;
            SerializedObject serializedObject = new SerializedObject(data);
            var config = (Noise)property.objectReferenceValue;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Draw background
                GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, Screen.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

                // Iterate over all the serialized fields and draw them
                SerializedProperty prop = serializedObject.GetIterator();
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (prop.NextVisible(true))
                {
                    do
                    {
                        // Don't bother drawing the class file
                        if (prop.name == "m_Script") continue;
                        else
                        {
                            float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                            Rect pos = new Rect(position.x, y, position.width, height);
                            if (prop.name == "alpha")
                                prop.floatValue = EditorGUI.Slider(pos, prop.displayName, prop.floatValue, 0, 1);
                            else
                                EditorGUI.PropertyField(pos, prop, true);
                            y += height + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    while (prop.NextVisible(false));
                }

                // save texture button
                if (GUI.Button(EditorGUI.IndentedRect(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight)), "Save Texture"))
                    config.SaveTexture();
                y = NextLine(y);

                // realtime preview
                config.realtime = EditorGUI.Toggle(new Rect(position.x, y, position.width / 2, EditorGUIUtility.singleLineHeight), "Realtime Editing", config.realtime);

                // preview calculation
                EditorGUI.BeginDisabledGroup(config.realtime);
                if (GUI.Button(EditorGUI.IndentedRect(new Rect(position.x + position.width / 2, y, position.width / 2, EditorGUIUtility.singleLineHeight)), "Calculate Noise"))
                    config.CalculatePreview();
                EditorGUI.EndDisabledGroup();
                if (config.realtime)
                    config.CalculatePreview();
                y = NextLine(y);

                y = PreviewTextureGUI(position, y, config);

                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUI.ObjectField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property, label);
        }
        property.serializedObject.ApplyModifiedProperties();
        
        EditorGUI.EndProperty();
    }

    protected virtual float PreviewTextureGUI(Rect position, float y, Noise config)
    {
        config.showPreview = EditorGUI.Foldout(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), config.showPreview, new GUIContent("Preview Noise"), true);
        y = NextLine(y);

        if (config.showPreview)
        {
            GUI.Box(EditorGUI.IndentedRect(new Rect(position.x, y, position.width, config.previewRes)), config.previewRT);
            y += config.previewRes + EditorGUIUtility.standardVerticalSpacing;
        }

        return y;
    }

    protected float NextLine(float y)
    {
        return y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

}