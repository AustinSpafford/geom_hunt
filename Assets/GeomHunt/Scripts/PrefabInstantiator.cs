using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif // UNITY_EDITOR

[ExecuteInEditMode]
[RequireComponent(typeof(InstantiationPreviewer))]
public class PrefabInstantiator : MonoBehaviour
{
	public GameObject Prefab = null;

	public Vector3 AdditionalTranslation = Vector3.zero;
	public Vector3 AdditionalRotation = Vector3.zero;
	public Vector3 AdditionalScaling = Vector3.one;
	
#if UNITY_EDITOR
	public void OnValidate()
	{
		UpdateInstantiationPreview();
	}

	public void OnEnable()
	{
		UpdateInstantiationPreview();
	}

	public void OnDisable()
	{
		UpdateInstantiationPreview();
	}

	[PostProcessScene]
	public static void OnPostprocessScene()
	{
		var unvisitedSubstitutions = new Stack<PrefabInstantiator>(FindObjectsOfType<PrefabInstantiator>());
		var garbageSubstitutions = new List<PrefabInstantiator>();

		while (unvisitedSubstitutions.Count > 0)
		{
			PrefabInstantiator currentSubstitution = unvisitedSubstitutions.Pop();
			
			GameObject instantiatedPrefab = currentSubstitution.TryInstantiatePrefab();
			
			if (instantiatedPrefab)
			{
				foreach (PrefabInstantiator newSubstitution in instantiatedPrefab.GetComponentsInChildren<PrefabInstantiator>())
				{
					unvisitedSubstitutions.Push(newSubstitution);
				}
			}

			garbageSubstitutions.Add(currentSubstitution);
		}

		// Clean up all of the substitutions.
		for (int index = 0;
			index < garbageSubstitutions.Count;
			++index)
		{
			Destroy(garbageSubstitutions[index]);
		}
	}

	private GameObject TryInstantiatePrefab()
	{
		GameObject result = null;

		if (enabled &&
			(Prefab != null))
		{
			// Just to protect against weird reentrancy situations (eg. circular-substitutions), 
			// we'll disable ourselves in case the call stack winds its way back to us.
			enabled = false;

			result = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
			
			result.transform.parent = transform;

			result.transform.localPosition = (Prefab.transform.localPosition + AdditionalTranslation);
			result.transform.localRotation = (Quaternion.Euler(AdditionalRotation) * Prefab.transform.localRotation);
			result.transform.localScale = Vector3.Scale(Prefab.transform.localScale, AdditionalScaling);
		}

		return result;
	}

	private void UpdateInstantiationPreview()
	{
		var instantiationPreviewer = GetComponent<InstantiationPreviewer>();

		instantiationPreviewer.ClearInstantiationPreviewsForSource(this);

		if (enabled &&
			(Prefab != null))
		{
			instantiationPreviewer.AddInstantiationPreviewWithAdditionalTransformation(
				this,
				Prefab,
				transform,
				AdditionalTranslation,
				Quaternion.Euler(AdditionalRotation),
				AdditionalScaling);
		}
	}
#endif // UNITY_EDITOR
}
