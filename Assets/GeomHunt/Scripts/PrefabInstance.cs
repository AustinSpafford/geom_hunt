using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif // UNITY_EDITOR

// Original concept: http://framebunker.com/blog/poor-mans-nested-prefabs/

[ExecuteInEditMode]
public class PrefabInstance : MonoBehaviour
{
	public enum InstantiationMode
	{
		InsertInstanceDuringGameBuild,
		OnlyInstantiateOnScriptStart,
	}

	public GameObject Prefab = null;

	public InstantiationMode Instantiation = InstantiationMode.InsertInstanceDuringGameBuild;
	
#if UNITY_EDITOR
	public void OnValidate()
	{
		UpdatePreviewNodes();
	}

	public void OnEnable()
	{
		UpdatePreviewNodes();

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
			// Attempting to imitate the mesh-lines color for when objects are normally selected.
			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
			
			Matrix4x4 previewRootTransform = transform.localToWorldMatrix;
			
			foreach (PrefabInstantiationPreview instantiationPreview in instantiationPreviews)
			{
				if (instantiationPreview.SourcePrefabInstance.Prefab.activeSelf)
				{
					foreach (MeshPreview meshPreview in instantiationPreview.PreviewMeshes)
					{
						Gizmos.matrix = (previewRootTransform * meshPreview.Transform);

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
			Matrix4x4 previewRootTransform = transform.localToWorldMatrix;

			foreach (PrefabInstantiationPreview instantiationPreview in instantiationPreviews)
			{
				if (instantiationPreview.SourcePrefabInstance.Prefab.activeSelf)
				{
					foreach (MeshPreview meshPreview in instantiationPreview.PreviewMeshes)
					{
						for (int materialIndex = 0;
							materialIndex < meshPreview.Materials.Count;
							++materialIndex)
						{
							Graphics.DrawMesh(
								meshPreview.Mesh,
								(previewRootTransform * meshPreview.Transform),
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
		// NOTE: This method of detecting changes to previewed prefabs is flawed, specifically
		// in that we will not receive a notification if the prefab is being edited directly
		// through the asset browser, and it's not instantiated in the scene heirarchy.
		// This can be made more robust by additionally using a FileSystemWatcher to
		// watch AssetDatabase.GetAssetPath(Prefab), but even that doesn't catch changes until the scene is saved.

		GameObject prefabSource = PrefabUtility.GetPrefabParent(prefabInstance) as GameObject;

		if (prefabSource != null)
		{
			bool prefabIsBeingPreviewed = false;

			foreach (PrefabInstantiationPreview instantiationPreview in instantiationPreviews)
			{
				if (instantiationPreview.SourcePrefabInstance.Prefab == prefabSource)
				{
					prefabIsBeingPreviewed = true;

					break;
				}
			}
			
			if (prefabIsBeingPreviewed)
			{
				UpdatePreviewNodes();
			}
		}
	}

	private struct MeshPreview
	{
		public Mesh Mesh;
		public Matrix4x4 Transform;
		public List<Material> Materials;
	}

	private struct PrefabInstantiationPreview
	{
		public PrefabInstance SourcePrefabInstance;
		public List<MeshPreview> PreviewMeshes;
	}

	[System.NonSerialized]
	private List<PrefabInstantiationPreview> instantiationPreviews = new List<PrefabInstantiationPreview>();

	private void UpdatePreviewNodes()
	{
		instantiationPreviews.Clear();

		if (enabled &&
			(Prefab != null))
		{
			AppendPreviewNodesRecursive(
				this,
				Matrix4x4.identity,
				instantiationPreviews);
		}
	}
	
	private static void AppendPreviewNodesRecursive(
		PrefabInstance nodePrefabInstantiator,
		Matrix4x4 nodeTransform,
		List<PrefabInstantiationPreview> inoutInstantiationPreviews)
	{
		GameObject nodePrefabSource = nodePrefabInstantiator.Prefab;

		Matrix4x4 baseTransform = (
			nodeTransform *
			Matrix4x4.TRS(
				(-1 * nodePrefabSource.transform.position),
				Quaternion.identity,
				Vector3.one));

		List<MeshPreview> nodeMeshPreviews = new List<MeshPreview>();

		foreach (Renderer nodeRenderer in nodePrefabSource.GetComponentsInChildren<Renderer>(includeInactive: true))
		{
			nodeMeshPreviews.Add(new MeshPreview()
			{
				Mesh = nodeRenderer.GetComponent<MeshFilter>().sharedMesh,
				Transform = (baseTransform * nodeRenderer.transform.localToWorldMatrix),
				Materials = new List<Material>(nodeRenderer.sharedMaterials),
			});
		}

		inoutInstantiationPreviews.Add(new PrefabInstantiationPreview()
		{
			SourcePrefabInstance = nodePrefabInstantiator,
			PreviewMeshes = nodeMeshPreviews,
		});

		foreach (PrefabInstance prefabInstance in nodePrefabSource.GetComponentsInChildren<PrefabInstance>(includeInactive: true))
		{
			if ((prefabInstance.Prefab != null) &&
				prefabInstance.enabled && 
				prefabInstance.gameObject.activeSelf)
			{
				AppendPreviewNodesRecursive(
					prefabInstance,
					(baseTransform * prefabInstance.transform.localToWorldMatrix),
					inoutInstantiationPreviews);
			}
		}
	}
#endif // UNITY_EDITOR
}
