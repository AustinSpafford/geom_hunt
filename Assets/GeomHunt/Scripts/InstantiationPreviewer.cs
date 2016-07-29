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
	public bool DebugEnabled = false;

#if UNITY_EDITOR
	public void AddInstantiationPreview(
		Component instantiator,
		GameObject prefab,
		Transform instanceParent)
	{
		AddInstantiationPreviewWithAdditionalTransformation(
			instantiator,
			prefab,
			instanceParent,
			Vector3.zero, // additionalTranslation
			Quaternion.identity, // additionalRotation
			Vector3.one); // additionalScaling
	}

	public void AddInstantiationPreviewWithAdditionalTransformation(
		Component instantiator,
		GameObject prefab,
		Transform instanceParent,
		Vector3 additionalTranslation,
		Quaternion additionalRotation,
		Vector3 additionalScaling)
	{
		PrefabInstantiation prefabInstantiation = new PrefabInstantiation()
		{
			Instantiator = instantiator,
			Prefab = prefab,
			InstanceParent = instanceParent,
			AdditionalTranslation = additionalTranslation,
			AdditionalRotation = additionalRotation,
			AdditionalScaling = additionalScaling,
		};

		prefabInstantiations.Add(prefabInstantiation);

		InvalidateCache("preview_added");
	}

	public void ClearInstantiationPreviewsForSource(
		Component instantiator)
	{
		int removedCount = 
			prefabInstantiations.RemoveAll(
				entry => (entry.Instantiator == instantiator));

		if (removedCount > 0)
		{
			InvalidateCache("preview_removed");
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
		
		InvalidateCache("disabled");
	}

	public void OnDrawGizmosSelected()
	{
		if (EditorApplication.isPlaying == false)
		{
			RefreshCache();
			
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;

			// Imitate the mesh-lines color for when objects are normally selected.
			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
			
			foreach (PrefabPreview prefabPreview in cachedPrefabPreviews)
			{			
				if (prefabPreview.SourcePrefab.activeSelf)
				{
					foreach (MeshPreview meshPreview in prefabPreview.MeshPreviews)
					{
						Gizmos.matrix = (localToWorldMatrix * meshPreview.LocalToMeshMatrix);

						Gizmos.DrawWireMesh(meshPreview.Mesh);
					}
				}
			}
		}
	}

	public void Update()
	{
		if (EditorApplication.isPlaying == false)
		{
			RefreshCache();
			
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;

			foreach (PrefabPreview prefabPreview in cachedPrefabPreviews)
			{
				if (prefabPreview.SourcePrefab.activeSelf)
				{
					foreach (MeshPreview meshPreview in prefabPreview.MeshPreviews)
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

			foreach (PrefabPreview instantiationPreview in cachedPrefabPreviews)
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
		public Component Instantiator;

		public GameObject Prefab;

		public Transform InstanceParent;

		public Vector3 AdditionalTranslation;
		public Quaternion AdditionalRotation;
		public Vector3 AdditionalScaling;
	}

	private struct MeshPreview
	{
		public Mesh Mesh;
		public Matrix4x4 LocalToMeshMatrix;
		public List<Material> Materials;
	}

	private struct PrefabPreview
	{
		public GameObject SourcePrefab;
		public List<MeshPreview> MeshPreviews;
	}

	private List<PrefabInstantiation> prefabInstantiations = new List<PrefabInstantiation>();

	private bool cacheIsValid = false;

	[System.NonSerialized]
	private List<PrefabPreview> cachedPrefabPreviews = new List<PrefabPreview>();

	private void InvalidateCache(
		string debugInvalidationReason)
	{
		cachedPrefabPreviews.Clear();

		cacheIsValid = false;
		
		if (DebugEnabled)
		{
			Debug.LogFormat(
				"Invalidated cache. (reason=[{0}])",
				debugInvalidationReason);
		}
	}

	private void RefreshCache()
	{
		if (cacheIsValid == false)
		{
			if (enabled &&
				gameObject.activeSelf &&
				(EditorApplication.isPlaying == false))
			{
				AppendPreviewNodesRecursive(
					this,
					Matrix4x4.identity,
					cachedPrefabPreviews);
			}

			cacheIsValid = true;

			if (DebugEnabled)
			{
				Debug.LogFormat(
					"Refreshed cache. Enabled=[{0}]. IsPlaying=[{1}].", 
					enabled, 
					EditorApplication.isPlaying);
			}
		}
	}
	
	private static void AppendPreviewNodesRecursive(
		InstantiationPreviewer previewer,
		Matrix4x4 simulatedPreviewerLocalToWorldMatrix,
		List<PrefabPreview> inoutPrefabPreviews)
	{
		foreach (PrefabInstantiation prefabInstantiation in previewer.prefabInstantiations)
		{
			GameObject prefabSource = prefabInstantiation.Prefab;

			Matrix4x4 instantiationLocalToWorldMatrix;
			{
				Matrix4x4 instantiationAdditionMatrix = 
					Matrix4x4.TRS(
						prefabInstantiation.AdditionalTranslation,
						prefabInstantiation.AdditionalRotation,
						prefabInstantiation.AdditionalScaling);
				
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
						instantiationAdditionMatrix);
				}
				else
				{
					// Since the instantiation is apparently a root-object, completely
					// reject the heirarchy built up so far.
					instantiationLocalToWorldMatrix = instantiationAdditionMatrix;
				}
			}

			List<MeshPreview> nodeMeshPreviews = new List<MeshPreview>();

			foreach (MeshRenderer nodeRenderer in prefabSource.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
			{
				nodeMeshPreviews.Add(new MeshPreview()
				{
					Mesh = nodeRenderer.GetComponent<MeshFilter>().sharedMesh,
					LocalToMeshMatrix = (instantiationLocalToWorldMatrix * nodeRenderer.transform.localToWorldMatrix),
					Materials = new List<Material>(nodeRenderer.sharedMaterials),
				});
			}

			inoutPrefabPreviews.Add(new PrefabPreview()
			{
				SourcePrefab = prefabInstantiation.Prefab,
				MeshPreviews = nodeMeshPreviews,
			});

			foreach (InstantiationPreviewer childPreviewer in prefabSource.GetComponentsInChildren<InstantiationPreviewer>())
			{
				// NOTE: We're not checking activeInHierarchy because
				// it returns false for prefabs that are not in a scene.
				if (childPreviewer.enabled &&
					childPreviewer.gameObject.activeSelf)
				{
					AppendPreviewNodesRecursive(
						childPreviewer,
						(instantiationLocalToWorldMatrix * childPreviewer.transform.localToWorldMatrix),
						inoutPrefabPreviews);
				}
			}
		}
	}
#endif // UNITY_EDITOR
}
