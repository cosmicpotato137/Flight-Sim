//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleWing))]
[CanEditMultipleObjects]
public class SimpleWingEditor : Editor
{
	private bool showDebug = false;
	SimpleWing wing;

	SerializedProperty dimensions;
	SerializedProperty applyForcesToCenter;
	SerializedProperty wingCurves;
	SerializedProperty liftMultiplier;
	SerializedProperty dragMultiplier;

	SerializedProperty isControlSurface;
	bool controlSurfaceFoldout = true;
	SerializedProperty controlAxis;
	SerializedProperty multiplier;

	SerializedProperty hinge;
	SerializedProperty max;
	SerializedProperty min;
	SerializedProperty moveSpeed;
	SerializedProperty targetDeflection;
	SerializedProperty maxTorque;

	public void OnEnable()
    {
		dimensions = serializedObject.FindProperty("dimensions");
		applyForcesToCenter = serializedObject.FindProperty("applyForcesToCenter");
		wingCurves = serializedObject.FindProperty("wingCurves");
		liftMultiplier = serializedObject.FindProperty("liftMultiplier");
		dragMultiplier = serializedObject.FindProperty("dragMultiplier");

		isControlSurface = serializedObject.FindProperty("isControlSurface");
		controlAxis = serializedObject.FindProperty("controlAxis");
		multiplier = serializedObject.FindProperty("multiplier");

		hinge = serializedObject.FindProperty("hinge");
		max = serializedObject.FindProperty("max");
		min = serializedObject.FindProperty("min");
		moveSpeed = serializedObject.FindProperty("moveSpeed");
		targetDeflection = serializedObject.FindProperty("targetDeflection");
		maxTorque = serializedObject.FindProperty("maxTorque");

		wing = (SimpleWing)target;
	}

    public override void OnInspectorGUI()
	{
		serializedObject.Update();
		//base.OnInspectorGUI();
		EditorGUILayout.LabelField("Wing", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(dimensions);
		EditorGUILayout.PropertyField(applyForcesToCenter);
		EditorGUILayout.PropertyField(wingCurves);
		EditorGUILayout.PropertyField(liftMultiplier);
		EditorGUILayout.PropertyField(dragMultiplier);

		controlSurfaceFoldout = EditorGUILayout.Foldout(controlSurfaceFoldout, "Control Surface", true, EditorStyles.foldoutHeader);
		if (controlSurfaceFoldout)
        {
			EditorGUILayout.PropertyField(isControlSurface);
			if (wing.isControlSurface)
			{
				EditorGUILayout.PropertyField(controlAxis);
				EditorGUILayout.PropertyField(multiplier);

				EditorGUILayout.LabelField("Deflection", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(hinge);
				EditorGUILayout.PropertyField(max);
				EditorGUILayout.PropertyField(min);
				EditorGUILayout.PropertyField(moveSpeed);
				EditorGUILayout.PropertyField(targetDeflection);
				EditorGUILayout.PropertyField(maxTorque);
			}
        }

		EditorGUILayout.Space();
		showDebug = EditorGUILayout.ToggleLeft("Show debug values", showDebug, EditorStyles.boldLabel);

		if (showDebug)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Wing Area: ", wing.WingArea.ToString("0.00"));
			EditorGUILayout.LabelField("Angle of Attack: ", wing.AngleOfAttack.ToString("0.00"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Lift Coefficient: ", wing.LiftCoefficient.ToString("0.00"));
			EditorGUILayout.LabelField("Lift Force: ", wing.LiftForce.ToString("0.00"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Drag Coefficient: ", wing.DragCoefficient.ToString("0.00"));
			EditorGUILayout.LabelField("Drag Force: ", wing.DragForce.ToString("0.00"));

			if (Application.isPlaying)
			{
				Repaint();
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}

