using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ProgressCondition))]
public class DoppelgangObjectOnProgressCondition : MonoBehaviour
{
	public enum ConditionPolarity
	{
		ShowVictimWhenConditionIsTrue,
		ShowDoppelgangerWhenConditionIsTrue,
	}

	public GameObject VictimObject = null;

	public Material DoppelgangerMaterial = null;

	public ConditionPolarity Polarity = ConditionPolarity.ShowVictimWhenConditionIsTrue;

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
		if ((VictimObject != null) &&
			(progressCondition != null))
		{
			bool doppelgangerShouldBeActive;
			
			switch (Polarity)
			{
				case ConditionPolarity.ShowDoppelgangerWhenConditionIsTrue:
					doppelgangerShouldBeActive = progressCondition.ConditionIsTrue;
					break;
					
				case ConditionPolarity.ShowVictimWhenConditionIsTrue:
					doppelgangerShouldBeActive = (progressCondition.ConditionIsTrue == false);
					break;

				default:
					throw new System.ComponentModel.InvalidEnumArgumentException(Polarity.ToString());
			}

			if (doppelgangerShouldBeActive && (doppelgangerInstance == null))
			{
				doppelgangerInstance = GameObject.Instantiate(VictimObject);

				doppelgangerInstance.transform.parent = VictimObject.transform.parent;

				doppelgangerInstance.transform.localPosition = VictimObject.transform.localPosition;
				doppelgangerInstance.transform.localRotation= VictimObject.transform.localRotation;
				doppelgangerInstance.transform.localScale = VictimObject.transform.localScale;

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

				// Hide the victim, as their doppelganger has taken their place.
				VictimObject.SetActive(false);
			}
			else if ((doppelgangerShouldBeActive == false) && (doppelgangerInstance != null))
			{
				Destroy(doppelgangerInstance);

				VictimObject.SetActive(true);
			}
		}
	}
}
