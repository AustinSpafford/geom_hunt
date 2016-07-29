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

	public bool IgnorePrefabsLocalPosition = true;

	public Vector3 AdditionalTranslation = Vector3.zero;
	public Vector3 AdditionalRotation = Vector3.zero;
	public Vector3 AdditionalScaling = Vector3.one;

	public bool BakeInstantiationWhenPossible = true;

	public bool DebugEnabled = false;

	public void Start()
	{
		// If we're either a runtime-only (non-baking) instantiator, or the
		// host object was dynamically instantiated (rather than pre-existing in the scene), 
		// proceed with instantiation.
		if (EditorApplication.isPlaying)
		{
			TryInstantiatePrefab();
		}
	}
	
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
		var unvisitedInstantiators = new Stack<PrefabInstantiator>(FindObjectsOfType<PrefabInstantiator>());

		while (unvisitedInstantiators.Count > 0)
		{
			PrefabInstantiator currentInstantiator = unvisitedInstantiators.Pop();
			
			if (currentInstantiator.BakeInstantiationWhenPossible)
			{
				GameObject instantiatedPrefab = currentInstantiator.TryInstantiatePrefab();
			
				if (instantiatedPrefab)
				{
					foreach (PrefabInstantiator newSubstitution in instantiatedPrefab.GetComponentsInChildren<PrefabInstantiator>())
					{
						unvisitedInstantiators.Push(newSubstitution);
					}
				}
			}
		}
	}

	private GameObject TryInstantiatePrefab()
	{
		GameObject result = null;

		if (DebugEnabled)
		{
			Debug.LogFormat(
				"Attempting instantiation. (enabled=[{0}]) (Prefab=[{1}])",
				enabled,
				(Prefab ? Prefab.name : "<null>"));
		}

		if (enabled &&
			(Prefab != null))
		{
			// Just to protect against weird reentrancy situations (eg. circular-substitutions), 
			// we'll disable ourselves in case the call stack winds its way back to us.
			enabled = false;

			result = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
			
			result.transform.parent = transform;

			result.transform.localPosition = (
				(IgnorePrefabsLocalPosition ? Vector3.zero : Prefab.transform.localPosition) + 
				AdditionalTranslation);

			result.transform.localRotation = (
				Quaternion.Euler(AdditionalRotation) * 
				Prefab.transform.localRotation);

			result.transform.localScale = 
				Vector3.Scale(Prefab.transform.localScale, AdditionalScaling);
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
			var previewFlags = InstantiationPreviewer.PreviewFlags.None;

			if (IgnorePrefabsLocalPosition)
			{
				previewFlags |= InstantiationPreviewer.PreviewFlags.IgnorePrefabPosition;
			}

			instantiationPreviewer.AddInstantiationPreviewWithAdditionalTransformation(
				this,
				Prefab,
				transform,
				previewFlags,
				AdditionalTranslation,
				Quaternion.Euler(AdditionalRotation),
				AdditionalScaling);
		}
	}
#endif // UNITY_EDITOR
}
