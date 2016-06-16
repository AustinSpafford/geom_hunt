using UnityEngine;
using System.Collections;

public class ClickSource : MonoBehaviour
{
	public bool DebugEnabled = false;

	public void Awake()
	{
		trackedController = GetComponentInParent<SteamVR_TrackedController>();

		if (trackedController == null)
		{
			throw new System.InvalidOperationException("The \"SteamVR_TrackedController\" component needs to be added to each controller. It's located in \"SteamVR\\Extras\".");
		}

		trackedController.TriggerClicked += OnTriggerClicked;
		trackedController.TriggerUnclicked += OnTriggerUnclicked;
	}

	public void TerminateClick()
	{
		if (currentClickTarget != null)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat("Terminating click on <b>{0}</b>.", currentClickTarget.name);
			}

			currentClickTarget.LeaveClickScope(this);
			
			currentClickTarget = null;
		}
	}

	private void OnTriggerClicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		ClickTarget nearestClickTarget = null;
		float nearestClickTargetDistanceSquared = float.MaxValue;

		// This is super-janky!
		// ...
		// *shrug*
		foreach (ClickTarget candidate in GameObject.FindObjectsOfType<ClickTarget>())
		{
			float candidateDistanceSquared =
				(transform.position - candidate.transform.position).sqrMagnitude;

			if (candidateDistanceSquared < nearestClickTargetDistanceSquared)
			{
				nearestClickTarget = candidate;
				nearestClickTargetDistanceSquared = candidateDistanceSquared;
			}
		}

		if (nearestClickTarget != null)
		{
			currentClickTarget = nearestClickTarget;

			if (DebugEnabled)
			{
				Debug.LogFormat("Entering click on <b>{0}</b>.", currentClickTarget.name);
			}

			currentClickTarget.EnterClickScope(this);
		}
	}

	private void OnTriggerUnclicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		TerminateClick();
	}

	private SteamVR_TrackedController trackedController = null;
	private ClickTarget currentClickTarget = null;
}
