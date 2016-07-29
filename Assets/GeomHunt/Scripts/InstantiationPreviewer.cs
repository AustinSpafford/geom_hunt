using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

// Original concept: http://framebunker.com/blog/poor-mans-nested-prefabs/
// This version doesn't directly perform the instantiation, but is instead a
// general tool for visualizing the results when other components instantiate game objects.

[ExecuteInEditMode]
[DisallowMultipleComponent] // This component already facilitates multiple siblings.
public class InstantiationPreviewer : MonoBehaviour
{
	[Flags]
	public enum PreviewFlags
	{
		None = 0,
		IgnorePrefabPosition = (1 << 0),
		IgnorePrefabRotation = (1 << 1),
		IgnorePrefabScale = (1 << 2),
	}

	public bool DebugEnabled = false;

#if UNITY_EDITOR
	public void AddInstantiationPreview(
		Behaviour instantiator,
		GameObject prefab,
		Transform instanceParent,
		PreviewFlags previewFlags)
	{
		AddInstantiationPreviewWithAdditionalTransformation(
			instantiator,
			prefab,
			instanceParent,
			previewFlags,
			Vector3.zero, // additionalTranslation
			Quaternion.identity, // additionalRotation
			Vector3.one); // additionalScaling
	}

	public void AddInstantiationPreviewWithAdditionalTransformation(
		Behaviour instantiator,
		GameObject prefab,
		Transform instanceParent,
		PreviewFlags previewFlags,
		Vector3 additionalTranslation,
		Quaternion additionalRotation,
		Vector3 additionalScaling)
	{
		PrefabInstantiation prefabInstantiation = new PrefabInstantiation()
		{
			Instantiator = instantiator,
			Prefab = prefab,
			InstanceParent = instanceParent,
			Flags = previewFlags,
			AdditionalTranslation = additionalTranslation,
			AdditionalRotation = additionalRotation,
			AdditionalScaling = additionalScaling,
		};

		prefabInstantiations.Add(prefabInstantiation);

		InvalidateCache("instantiation_added");
	}

	public void ClearInstantiationPreviewsForSource(
		Behaviour instantiator)
	{
		int removedCount = 
			prefabInstantiations.RemoveAll(
				entry => (entry.Instantiator == instantiator));

		if (removedCount > 0)
		{
			InvalidateCache("instantiation_removed");
		}
	}

	public void OnEnable()
	{
		InvalidateCache("enabled");

		PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
	}

	public void OnDisable()
	{
		PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdated;
	}

	public void OnDrawGizmosSelected()
	{
		if (EditorApplication.isPlaying == false)
		{
			RefreshCache();
			
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;

			// Imitate the mesh-lines color for when objects are normally selected.
			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
			
			foreach (MeshPreview meshPreview in GetVisibleMeshPreviews())
			{
				Gizmos.matrix = (localToWorldMatrix * meshPreview.LocalToMeshMatrix);

				Gizmos.DrawWireMesh(meshPreview.Mesh);
			}
		}
	}

	public void Update()
	{
		if (EditorApplication.isPlaying == false)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat(
					"[{0}].[{1}] Starting editor-update. InstantiationCount=[{2}].",
					gameObject.name,
					this.GetType().Name,
					prefabInstantiations.Count);
			}

			RefreshCache();

			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
			
			foreach (MeshPreview meshPreview in GetVisibleMeshPreviews())
			{
				for (int materialIndex = 0;
					materialIndex < meshPreview.Materials.Count;
					++materialIndex)
				{
					Graphics.DrawMesh(
						meshPreview.Mesh,
						(localToWorldMatrix * meshPreview.LocalToMeshMatrix),
						meshPreview.Materials[materialIndex],
						gameObject.layer,
						null, // camera
						materialIndex);
				}
			}
		}
	}

	public void OnPrefabInstanceUpdated(
		GameObject prefabInstance)
	{
		// NOTE: This method of detecting changes to previewed prefabs is flawed in that
		// we will not receive a notification when there's no instantiated prefab in
		// the scene and the prefab-source is being edited directly through the asset browser.
		// If needed, this can be made more robust by additionally using a FileSystemWatcher to
		// watch AssetDatabase.GetAssetPath(Prefab), but even that doesn't catch changes until the scene is saved.

		GameObject prefabSource = PrefabUtility.GetPrefabParent(prefabInstance) as GameObject;

		if (prefabSource != null)
		{
			bool prefabIsBeingPreviewed = false;

			foreach (PrefabPreview instantiationPreview in cachedPrefabPreviewRoots)
			{
				if (instantiationPreview.SourcePrefab == prefabSource)
				{
					prefabIsBeingPreviewed = true;

					break;
				}
			}
			
			if (prefabIsBeingPreviewed)
			{
				InvalidateCache("prefab_updated");
			}
		}
	}

	private struct PrefabInstantiation
	{
		public Behaviour Instantiator;

		public GameObject Prefab;

		public Transform InstanceParent;

		public PreviewFlags Flags;

		public Vector3 AdditionalTranslation;
		public Quaternion AdditionalRotation;
		public Vector3 AdditionalScaling;
	}

	private class MeshPreview
	{
		public MeshRenderer SourceMeshRenderer;

		public Mesh Mesh;
		public Matrix4x4 LocalToMeshMatrix;
		public List<Material> Materials;
	}

	private class PrefabPreview
	{
		public InstantiationPreviewer SourcePreviewer;
		public GameObject SourcePrefab;
		public Behaviour Instantiator;

		public List<MeshPreview> MeshPreviews;

		public List<PrefabPreview> ChildPrefabPreviews;
	}

	private List<PrefabInstantiation> prefabInstantiations = new List<PrefabInstantiation>();

	private bool cacheIsValid = false;

	[System.NonSerialized]
	private List<PrefabPreview> cachedPrefabPreviewRoots = new List<PrefabPreview>();

	private IEnumerable<MeshPreview> GetVisibleMeshPreviews()
	{
		var unvisitedPrefabPreviews = new Stack<PrefabPreview>(cachedPrefabPreviewRoots);

		while (unvisitedPrefabPreviews.Count > 0)
		{
			PrefabPreview prefabPreview = unvisitedPrefabPreviews.Pop();

			// NOTE: We're not checking activeInHierarchy because
			// it returns false for prefabs that are not in a scene.
			// NOTE: Some editor operations (eg. drag-and-drop of components) can
			// destroy the previewer on an instance without notifying us, so we'll gracefully
			// ignore all broken references until the prefab-update triggers a cleanup.
			if ((prefabPreview.SourcePreviewer != null) &&
				prefabPreview.SourcePreviewer.enabled &&
				prefabPreview.SourcePreviewer.gameObject.activeSelf &&
				(prefabPreview.SourcePrefab != null) &&
				prefabPreview.SourcePrefab.activeSelf &&
				(prefabPreview.Instantiator != null) &&
				prefabPreview.Instantiator.enabled)
			{
				foreach (MeshPreview meshPreview in prefabPreview.MeshPreviews)
				{
					if (meshPreview.SourceMeshRenderer.enabled)
					{
						yield return meshPreview;
					}
				}

				foreach (PrefabPreview childPrefabPreview in prefabPreview.ChildPrefabPreviews)
				{
					unvisitedPrefabPreviews.Push(childPrefabPreview);
				}
			}
		}
	}

	private void InvalidateCache(
		string debugInvalidationReason)
	{
		cachedPrefabPreviewRoots.Clear();

		cacheIsValid = false;
		
		if (DebugEnabled)
		{
			Debug.LogFormat(
				"[{0}].[{1}] Invalidated cache. Reason=[{2}]. InstantiationCount=[{3}].",
				gameObject.name,
				this.GetType().Name,
				debugInvalidationReason,
				prefabInstantiations.Count);
		}
	}

	private void RefreshCache()
	{
		if (cacheIsValid == false)
		{
			if (EditorApplication.isPlaying == false)
			{
				AppendPrefabPreviewsRecursive(
					this,
					Matrix4x4.identity,
					cachedPrefabPreviewRoots);
			}

			cacheIsValid = true;

			if (DebugEnabled)
			{
				Debug.LogFormat(
					"[{0}].[{1}] Refreshed cache. Enabled=[{2}]. IsPlaying=[{3}]. InstantiationCount=[{4}].", 
					gameObject.name,
					this.GetType().Name,
					enabled, 
					EditorApplication.isPlaying,
					prefabInstantiations.Count);
			}
		}
	}
	
	private static void AppendPrefabPreviewsRecursive(
		InstantiationPreviewer previewer,
		Matrix4x4 simulatedPreviewerLocalToWorldMatrix,
		List<PrefabPreview> inoutPrefabPreviews)
	{
		foreach (PrefabInstantiation prefabInstantiation in previewer.prefabInstantiations)
		{
			GameObject prefabSource = prefabInstantiation.Prefab;
			
			// We'll build up the matrix from the leaf-transform back to the root.
			Matrix4x4 instantiationLocalToWorldMatrix;
			{
				Vector3 filteredPrefabLocalPosition =
					((prefabInstantiation.Flags & PreviewFlags.IgnorePrefabPosition) != 0) ?
						Vector3.zero :
						prefabSource.transform.localPosition;
				
				Quaternion filteredPrefabLocalRotation =
					((prefabInstantiation.Flags & PreviewFlags.IgnorePrefabRotation) != 0) ?
						Quaternion.identity :
						prefabSource.transform.localRotation;
				
				Vector3 filteredPrefabLocalScale =
					((prefabInstantiation.Flags & PreviewFlags.IgnorePrefabScale) != 0) ?
						Vector3.one :
						prefabSource.transform.localScale;

				// NOTE: It's confusing as hell, but we're first undoing the 
				// prefab's local transform, and then redoing the parts of it
				// we actually want. This is frankly easier than decomposing the 
				// transform and then just applying the portions we actually want included.
				instantiationLocalToWorldMatrix =
					Matrix4x4.TRS(
						filteredPrefabLocalPosition,
						filteredPrefabLocalRotation,
						filteredPrefabLocalScale) *
					Matrix4x4.TRS(
						prefabSource.transform.localPosition,
						prefabSource.transform.localRotation,
						prefabSource.transform.localScale).inverse;

				// Factor in the additional-transformation.
				instantiationLocalToWorldMatrix = (
					Matrix4x4.TRS(
						prefabInstantiation.AdditionalTranslation,
						prefabInstantiation.AdditionalRotation,
						prefabInstantiation.AdditionalScaling) *
					instantiationLocalToWorldMatrix);
				
				if (prefabInstantiation.InstanceParent != null)
				{
					// NOTE: The previewer and instanceParent must either both be 
					// instantiated (aka. in-scene), or both be within the same prefab. 
					// In either case it means it's possible to compare their matrices.
					Matrix4x4 previewerToInstanceParentMatrix = (
						previewer.transform.localToWorldMatrix.inverse *
						prefabInstantiation.InstanceParent.localToWorldMatrix);

					instantiationLocalToWorldMatrix = (
						simulatedPreviewerLocalToWorldMatrix *
						previewerToInstanceParentMatrix * 
						instantiationLocalToWorldMatrix);
				}
				else
				{
					// Since the instantiation is apparently a root-object, we're already
					// done because it's rejecting the heirarchy's simulated-previewer transform.
				}
			}

			List<MeshPreview> meshPreviews = new List<MeshPreview>();

			foreach (MeshRenderer meshRenderer in prefabSource.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
			{
				meshPreviews.Add(new MeshPreview()
				{
					SourceMeshRenderer = meshRenderer,
					Mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh,
					LocalToMeshMatrix = (instantiationLocalToWorldMatrix * meshRenderer.transform.localToWorldMatrix),
					Materials = new List<Material>(meshRenderer.sharedMaterials),
				});
			}

			PrefabPreview prefabPreview = new PrefabPreview()
			{
				SourcePreviewer = previewer,
				SourcePrefab = prefabInstantiation.Prefab,
				Instantiator = prefabInstantiation.Instantiator,
				MeshPreviews = meshPreviews,
				ChildPrefabPreviews = new List<PrefabPreview>(),
			};

			inoutPrefabPreviews.Add(prefabPreview);

			foreach (InstantiationPreviewer childPreviewer in prefabSource.GetComponentsInChildren<InstantiationPreviewer>())
			{
				AppendPrefabPreviewsRecursive(
					childPreviewer,
					(instantiationLocalToWorldMatrix * childPreviewer.transform.localToWorldMatrix),
					prefabPreview.ChildPrefabPreviews);
			}
		}
	}
#endif // UNITY_EDITOR
}
