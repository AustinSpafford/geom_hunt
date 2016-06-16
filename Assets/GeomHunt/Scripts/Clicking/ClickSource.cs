using UnityEngine;
using System.Collections;

public class ClickSource : MonoBehaviour
{
	public GameObject AimingLaser = null;
	public float AimingLaserMaxRenderedLength = 10.0f;
	public float AimingDownwardAngle = 0.0f;
	public float AimingMaxSearchDistance = 100.0f;

	public float DirectTouchRadius = 0.1f;

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

	public void Update()
	{
		if (ShouldBeSeekingTarget())
		{
			Vector3 rayOrigin = transform.position;

			Quaternion rayOrientation = 
				(transform.rotation * Quaternion.Euler(AimingDownwardAngle, 0.0f, 0.0f));
			
			Vector3 rayDirection = 
				(rayOrientation * Vector3.forward);

			RaycastHit raycastHit;
			Physics.Raycast(
				new Ray(rayOrigin, rayDirection),
				out raycastHit,
				AimingMaxSearchDistance,
				Physics.DefaultRaycastLayers);

			if (raycastHit.transform != currentHoverTarget)
			{
				TerminateHover();

				if (raycastHit.transform != null)
				{
					ClickTarget raycastHitClickTarget = raycastHit.transform.GetComponent<ClickTarget>();

					if (raycastHitClickTarget != null)
					{
						currentHoverTarget = raycastHitClickTarget;

						if (DebugEnabled)
						{
							Debug.LogFormat("Entering hover on <b>{0}</b>.", currentHoverTarget.name);
						}

						currentHoverTarget.EnterHoverScope();
					}
				}
			}

			if (AimingLaser != null)
			{
				AimingLaser.SetActive(true);

				AimingLaser.transform.position = rayOrigin;
				AimingLaser.transform.rotation = rayOrientation;

				float laserLength = 
					Mathf.Min(
						AimingLaserMaxRenderedLength,
						((raycastHit.transform != null) ? raycastHit.distance : AimingMaxSearchDistance));
			
				AimingLaser.transform.localScale = 
					new Vector3(
						AimingLaser.transform.localScale.x,
						AimingLaser.transform.localScale.y,
						laserLength);
			}
		}
		else // We're not seeking a target.
		{
			if (AimingLaser != null)
			{
				AimingLaser.SetActive(false);
			}
		}
	}

	public void TerminateHover()
	{
		if (currentHoverTarget != null)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat("Terminating hover on <b>{0}</b>.", currentHoverTarget.name);
			}

			currentHoverTarget.LeaveHoverScope();
			
			currentHoverTarget = null;
		}
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
		if (currentHoverTarget != null)
		{
			currentClickTarget = currentHoverTarget;

			if (DebugEnabled)
			{
				Debug.LogFormat("Entering click on <b>{0}</b>.", currentClickTarget.name);
			}

			TerminateHover();

			currentClickTarget.EnterClickScope(this);
		}
	}

	private void OnTriggerUnclicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		TerminateClick();
	}

	private bool ShouldBeSeekingTarget()
	{
		return (currentClickTarget == null);
	}

	private SteamVR_TrackedController trackedController = null;
	private ClickTarget currentHoverTarget = null;
	private ClickTarget currentClickTarget = null;
}
