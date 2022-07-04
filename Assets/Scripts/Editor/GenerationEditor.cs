using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generation))]
[CanEditMultipleObjects]
public class GenerationEditor : Editor
{
	Generation gen;

	public void OnEnable()
	{
		gen = (Generation)target;
	}

	public override void OnInspectorGUI()
	{
		if (gen.bounds == null && !gen.GetComponent<BoxCollider>())
		{
			Debug.Log("Script requires BoxCollider");
			gen.bounds = gen.gameObject.AddComponent<BoxCollider>();
		}
		else if (gen.bounds == null)
		{
			Debug.Log("Script requires BoxCollider");
			gen.bounds = gen.GetComponent<BoxCollider>();
		}
		if (!gen.bounds.isTrigger)
			gen.bounds.isTrigger = true;

		if (GUILayout.Button("Generate Map"))
        {
			gen.ResetLayout();
        }
		if (GUILayout.Button("Delete Map"))
        {
			gen.ClearBoxes();
        }
		base.OnInspectorGUI();
	}
}

