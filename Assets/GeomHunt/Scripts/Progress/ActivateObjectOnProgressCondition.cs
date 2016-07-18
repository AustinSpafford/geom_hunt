using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ProgressCondition))]
public class ActivateObjectOnProgressCondition : MonoBehaviour
{
	public GameObject TargetObject = null;

	public bool DebugEnabled = false;

	public void Start()
	{
		progressCondition = GetComponent<ProgressCondition>();

		UpdateTargetObjectActivation();
	}
	
	public void OnEnable()
	{
		// Refresh just in case we were disabled when a condition-changed message was sent.
		UpdateTargetObjectActivation();
	}

	private ProgressCondition progressCondition = null;

	private void OnProgressConditionChanged(
		ProgressConditionChangedMessageArgs messageArgs)
	{
		UpdateTargetObjectActivation();
	}

	private void UpdateTargetObjectActivation()
	{
		if ((TargetObject != null) &&
			(progressCondition != null))
		{
			bool targetShouldBeActive = progressCondition.ConditionIsTrue;

			if (targetShouldBeActive != TargetObject.activeSelf)
			{
				TargetObject.SetActive(targetShouldBeActive);
			}
		}
	}
}
