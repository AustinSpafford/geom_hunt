using UnityEngine;
using System.Collections;

public class GripperController : MonoBehaviour
{
	public void Awake()
	{
		trackedController = GetComponentInParent<SteamVR_TrackedController>();
		velociGripper = GetComponentInParent<VelociGripper>();

		trackedController.TriggerClicked += OnTriggerClicked;
		trackedController.TriggerUnclicked += OnTriggerUnclicked;
	}

	private void OnTriggerClicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		GameObject nearestGrabbableObject = null;
		float nearestGrabbableObjectDistanceSquared = float.MaxValue;

		// This is super-janky!
		// ...
		// *shrug*
		foreach (GameObject candidate in GameObject.FindGameObjectsWithTag("Grabbable"))
		{
			float candidateDistanceSquared =
				(transform.position - candidate.transform.position).sqrMagnitude;

			if (candidateDistanceSquared < nearestGrabbableObjectDistanceSquared)
			{
				nearestGrabbableObject = candidate;
				nearestGrabbableObjectDistanceSquared = candidateDistanceSquared;
			}
		}

		if (nearestGrabbableObject != null)
		{
			velociGripper.GripObject(nearestGrabbableObject);
		}
	}

	private void OnTriggerUnclicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		velociGripper.ReleaseObject();
	}

	private SteamVR_TrackedController trackedController = null;
	private VelociGripper velociGripper = null;
}
