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
	
	[Space(10)]

	public Vector3 AdditionalTranslation = Vector3.zero;
	public Vector3 AdditionalRotation = Vector3.zero;
	public Vector3 AdditionalScaling = Vector3.one;
	
	[Space(10)]

	[Tooltip("When enabled and the instantiator's host object is within the scene hierarchy, hitting Play or Build will immediately instantiate the prefab. Otherwise instantiation occurs when the script Starts (creating a delay).")]
	public bool BakeInstantiationWhenPossible = true;

	public bool IgnorePrefabsLocalPosition = true;
	public bool IgnorePrefabsLocalRotation = false;
	public bool IgnorePrefabsLocalScale = false;
	
	[Space(10)]

	public bool DebugEnabled = false;

	public void Start()
	{
		// If we're either a runtime-only (non-baking) instantiator, or the
		// host object was dynamically instantiated (rather than pre-existing in the scene), 
		// proceed with instantiation.
		if (Application.isPlaying)
		{
			TryInstantiatePrefab();
		}
	}
	
#if UNITY_EDITOR
	public void OnEnable()
	{
		UpdateInstantiationPreview("enabled");
	}

	public void OnValidate()
	{
		UpdateInstantiationPreview("validation");
	}
#endif // UNITY_EDITOR
	
#if UNITY_EDITOR
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
#endif // UNITY_EDITOR

	private GameObject TryInstantiatePrefab()
	{
		GameObject result = null;

		if (DebugEnabled)
		{
			Debug.LogFormat(
				"[{0}].[{1}] Attempting instantiation. Enabled=[{2}]. Prefab=[{3}].",
				gameObject.name,
				this.GetType().Name,
				enabled,
				(Prefab ? Prefab.name : "<null>"));
		}

		if (enabled &&
			(Prefab != null))
		{
			// Just to protect against weird reentrancy situations (eg. circular-substitutions), 
			// we'll disable ourselves in case the call stack winds its way back to us.
			enabled = false;

			bool prefabWasActive = Prefab.activeSelf;

			// We need to temporarily disable the prefab to prevent its instantiation
			// from waking/starting before we assign it to its parent.
			Prefab.SetActive(false);

			result = GameObject.Instantiate<GameObject>(Prefab);

			Prefab.SetActive(prefabWasActive);
			
			result.transform.parent = transform;

			result.transform.localPosition = (
				(IgnorePrefabsLocalPosition ? Vector3.zero : Prefab.transform.localPosition) + 
				AdditionalTranslation);

			result.transform.localRotation = (
				Quaternion.Euler(AdditionalRotation) * 
				(IgnorePrefabsLocalRotation ? Quaternion.identity : Prefab.transform.localRotation));

			result.transform.localScale = 
				Vector3.Scale(
					(IgnorePrefabsLocalScale ? Vector3.one : Prefab.transform.localScale), 
					AdditionalScaling);

			result.SetActive(prefabWasActive);
		}

		return result;
	}
	
#if UNITY_EDITOR
	private void UpdateInstantiationPreview(
		string debugUpdateReason)
	{
		if (DebugEnabled)
		{
			Debug.LogFormat(
				"[{0}].[{1}] Updating instantiation-preview. Reason=[{2}].",
				gameObject.name,
				this.GetType().Name,
				debugUpdateReason);
		}

		var instantiationPreviewer = GetComponent<InstantiationPreviewer>();

		instantiationPreviewer.ClearInstantiationPreviewsForSource(this);
		
		if (Prefab != null)
		{
			var previewFlags = (
				(IgnorePrefabsLocalPosition ? InstantiationPreviewer.PreviewFlags.IgnorePrefabPosition : 0) |
				(IgnorePrefabsLocalRotation ? InstantiationPreviewer.PreviewFlags.IgnorePrefabRotation : 0) |
				(IgnorePrefabsLocalScale ? InstantiationPreviewer.PreviewFlags.IgnorePrefabScale : 0));

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
