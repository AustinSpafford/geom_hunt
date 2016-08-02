using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(InstantiationPreviewer))]
public class InventoryBaubleSpawner : MonoBehaviour
{
	public GameObject EmptyInventoryBaublePrefab = null;

	public GameObject PrefabSpawnedByClick = null;
	
	[Space(10)]

	[Tooltip("When specified, this prefab will be used in the bauble's preview. This can be useful if the normal prefab is especially high-detail.")]
	public GameObject OptionalPreviewOverridePrefab = null;

	public Vector3 PreviewAdditionalTranslation = Vector3.zero;
	public Vector3 PreviewAdditionalRotation = Vector3.zero;
	public Vector3 PreviewAdditionalScaling = Vector3.one;
	
	[Space(10)]

	public ProgressCondition.ConditionExpression BaubleUnlockedProgressCondition = null;
	
	[Space(10)]
	
	public bool DebugEnabled = false;

	public void Start()
	{
		if (Application.isPlaying)
		{
			TryInstantiateInventoryBauble();

			enabled = false;
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
	
	private GameObject TryInstantiateInventoryBauble()
	{
		GameObject result = null;

		if (DebugEnabled)
		{
			Debug.LogFormat(
				"[{0}].[{1}] Attempting instantiation. Prefab=[{3}].",
				gameObject.name,
				this.GetType().Name,
				(PrefabSpawnedByClick ? PrefabSpawnedByClick.name : "<null>"));
		}

		if (enabled &&
			(EmptyInventoryBaublePrefab != null))
		{
			// Just to protect against weird reentrancy situations (eg. circular-substitutions), 
			// we'll disable ourselves in case the call stack winds its way back to us.
			enabled = false;

			bool baublePrefabWasActive = EmptyInventoryBaublePrefab.activeSelf;

			// We need to temporarily disable the prefab to prevent its instantiation
			// from waking/starting before we fully configure it.
			EmptyInventoryBaublePrefab.SetActive(false);

			result = GameObject.Instantiate<GameObject>(EmptyInventoryBaublePrefab);

			EmptyInventoryBaublePrefab.SetActive(baublePrefabWasActive);

			// Configure the transform.
			{
				result.transform.parent = transform;

				result.transform.localPosition = EmptyInventoryBaublePrefab.transform.localPosition;
				result.transform.localRotation = EmptyInventoryBaublePrefab.transform.localRotation;
				result.transform.localScale = EmptyInventoryBaublePrefab.transform.localScale;
			}

			// Configure the progress-condition.
			{
				var progressCondition = result.GetComponent<ProgressCondition>();

				// Rather than bothering with a deep-copy of the condition, just transfer ownership.
				progressCondition.Condition = BaubleUnlockedProgressCondition;
				BaubleUnlockedProgressCondition = null;
			}

			// Configure the preview.
			{
				var doppelgangOnCondition = result.GetComponent<DoppelgangObjectOnProgressCondition>();

				GameObject previewRoot = doppelgangOnCondition.TargetObject;

				GameObject previewPrefab =
					(OptionalPreviewOverridePrefab != null) ?
						OptionalPreviewOverridePrefab :
						PrefabSpawnedByClick;
				
				// NOTE: Since the bauble-root is currently disabled, we don't have to worry
				// about temporarily disabling the preview object.
				GameObject previewInstance = GameObject.Instantiate<GameObject>(previewPrefab);

				previewInstance.transform.parent = previewRoot.transform;

				previewInstance.transform.localPosition = 
					PreviewAdditionalTranslation;

				previewInstance.transform.localRotation = (
					Quaternion.Euler(PreviewAdditionalRotation) * 
					previewPrefab.transform.localRotation);

				previewInstance.transform.localScale =
					Vector3.Scale(
						previewPrefab.transform.localScale,
						PreviewAdditionalScaling);
			}

			// Configure the click-causes-spawn script.
			{
				var clickCausesObjectSpawn = result.GetComponent<ClickCausesObjectSpawn>();

				clickCausesObjectSpawn.SpawneePrefab = PrefabSpawnedByClick;
			}

			result.SetActive(baublePrefabWasActive);
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
		
		if (EmptyInventoryBaublePrefab != null)
		{
			instantiationPreviewer.AddInstantiationPreview(
				this,
				EmptyInventoryBaublePrefab,
				transform,
				InstantiationPreviewer.PreviewFlags.None);
		}
		
		if (PrefabSpawnedByClick != null)
		{
			instantiationPreviewer.AddInstantiationPreviewWithAdditionalTransformation(
				this,
				PrefabSpawnedByClick,
				transform,
				InstantiationPreviewer.PreviewFlags.IgnorePrefabPosition,
				PreviewAdditionalTranslation,
				Quaternion.Euler(PreviewAdditionalRotation),
				Vector3.Scale(
					EmptyInventoryBaublePrefab.transform.localScale,
					PreviewAdditionalScaling));
		}
	}
#endif // UNITY_EDITOR
}
