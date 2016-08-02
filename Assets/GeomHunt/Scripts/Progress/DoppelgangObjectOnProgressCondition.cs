using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ProgressCondition))]
public class DoppelgangObjectOnProgressCondition : MonoBehaviour
{
	public enum ConditionPolarity
	{
		ShowTargetWhenConditionIsTrue,
		ShowDoppelgangerWhenConditionIsTrue,
	}

	public GameObject TargetObject = null;

	public Material DoppelgangerMaterial = null;

	public ConditionPolarity Polarity = ConditionPolarity.ShowTargetWhenConditionIsTrue;

	public bool DebugEnabled = false;

	public void Start()
	{
		progressCondition = GetComponent<ProgressCondition>();

		UpdateDoppelgangerExistence();
	}
	
	public void OnEnable()
	{
		// Refresh just in case we were disabled when a condition-changed message was sent.
		UpdateDoppelgangerExistence();
	}

	private ProgressCondition progressCondition = null;

	private GameObject doppelgangerInstance = null;
	
	private void OnProgressConditionChanged(
		ProgressConditionChangedMessageArgs messageArgs)
	{
		UpdateDoppelgangerExistence();
	}

	private void UpdateDoppelgangerExistence()
	{
		if ((TargetObject != null) &&
			(progressCondition != null))
		{
			bool doppelgangerShouldBeActive;
			
			switch (Polarity)
			{
				case ConditionPolarity.ShowDoppelgangerWhenConditionIsTrue:
					doppelgangerShouldBeActive = progressCondition.ConditionIsTrue;
					break;
					
				case ConditionPolarity.ShowTargetWhenConditionIsTrue:
					doppelgangerShouldBeActive = (progressCondition.ConditionIsTrue == false);
					break;

				default:
					throw new System.ComponentModel.InvalidEnumArgumentException(Polarity.ToString());
			}

			if (doppelgangerShouldBeActive && (doppelgangerInstance == null))
			{
				doppelgangerInstance = GameObject.Instantiate(TargetObject);

				doppelgangerInstance.transform.parent = TargetObject.transform.parent;

				doppelgangerInstance.transform.localPosition = TargetObject.transform.localPosition;
				doppelgangerInstance.transform.localRotation= TargetObject.transform.localRotation;
				doppelgangerInstance.transform.localScale = TargetObject.transform.localScale;

				if (DoppelgangerMaterial != null)
				{
					foreach (Renderer renderer in doppelgangerInstance.GetComponentsInChildren<Renderer>())
					{
						var newMaterials = new Material[renderer.sharedMaterials.Length];
						
						for (int index = 0; index < newMaterials.Length; index++)
						{
							newMaterials[index] = DoppelgangerMaterial;
						}

						renderer.sharedMaterials = newMaterials;
					}
				}

				// Hide the target, as their doppelganger has taken their place.
				TargetObject.SetActive(false);
			}
			else if ((doppelgangerShouldBeActive == false) && (doppelgangerInstance != null))
			{
				Destroy(doppelgangerInstance);

				TargetObject.SetActive(true);
			}
		}
	}
}
