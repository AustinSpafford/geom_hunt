using UnityEngine;
using System.Collections;

public class ClickSource : MonoBehaviour
{
	public enum InteractionMode
	{
		MomentaryClicks, // Laser shown at all times, click duration matches trigger-presses.
		ToggledClicks, // First trigger-press shows laser and starts the click, second trigger-presss terminates the click.
	}

	public InteractionMode UserInterationMode = InteractionMode.MomentaryClicks;

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
	}

	public void OnEnable()
	{
		trackedController.TriggerClicked += OnTriggerClicked;
		trackedController.TriggerUnclicked += OnTriggerUnclicked;
	}

	public void OnDisable()
	{
		trackedController.TriggerClicked -= OnTriggerClicked;
		trackedController.TriggerUnclicked -= OnTriggerUnclicked;

		TryTerminateClick();
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
				TryTerminateHover();

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

	public bool TryClickHoverTarget()
	{
		bool result = false;

		if (currentHoverTarget != null)
		{
			TryTerminateClick();

			currentClickTarget = currentHoverTarget;

			if (DebugEnabled)
			{
				Debug.LogFormat("Entering click on <b>{0}</b>.", currentClickTarget.name);
			}

			TryTerminateHover();

			currentClickTarget.EnterClickScope(this);

			result = true;
		}

		return result;
	}

	public bool TryTerminateHover()
	{
		bool result = false;

		if (currentHoverTarget != null)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat("Terminating hover on <b>{0}</b>.", currentHoverTarget.name);
			}

			currentHoverTarget.LeaveHoverScope();
			
			currentHoverTarget = null;

			result = true;
		}

		return result;
	}

	public bool TryTerminateClick()
	{
		bool result = false;

		if (currentClickTarget != null)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat("Terminating click on <b>{0}</b>.", currentClickTarget.name);
			}

			currentClickTarget.LeaveClickScope(this);
			
			currentClickTarget = null;

			result = true;
		}

		return result;
	}

	private void OnTriggerClicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryClicks:
			{
				TryClickHoverTarget();

				break;
			}
				
			case InteractionMode.ToggledClicks:
			{
				// In the toggle-mode, major state changes only occur on the trigger-release.
				break;
			}

			default:
			{
				throw new System.InvalidOperationException();
			}
		}
	}

	private void OnTriggerUnclicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryClicks:
			{
				TryTerminateClick();

				break;
			}
				
			case InteractionMode.ToggledClicks:
			{
				// If this is the end of the first trigger-press.
				if (currentClickTarget == null)
				{
					TryClickHoverTarget();
				}
				else 
				{
					TryTerminateClick();
				}

				break;
			}

			default:
			{
				throw new System.InvalidOperationException();
			}
		}
	}

	private bool ShouldBeSeekingTarget()
	{
		bool result = false;

		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryClicks:
			{
				// Momentary-mode always seeks for a target while we're not clicking.
				result = (currentClickTarget == null);

				break;
			}
				
			case InteractionMode.ToggledClicks:
			{
				if (currentClickTarget == null)
				{
					// Only seek for targets while the trigger is pressed.
					result = trackedController.triggerPressed;
				}
				else 
				{
					// We're clicking, so there's definitely no need to search.
					result = false;
				}

				break;
			}

			default:
			{
				throw new System.InvalidOperationException();
			}
		}

		return result;
	}
	
	private SteamVR_TrackedController trackedController = null;

	private ClickTarget currentHoverTarget = null;
	private ClickTarget currentClickTarget = null;
}
