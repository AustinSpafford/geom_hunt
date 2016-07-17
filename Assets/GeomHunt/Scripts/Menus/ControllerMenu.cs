using UnityEngine;
using System.Collections;

public struct ControllerMenuOpenedEventArgs
{
}

public delegate void ControllerMenuOpenedEventHandler(object sender, ControllerMenuOpenedEventArgs eventArgs);

public class ControllerMenu : MonoBehaviour
{
	public enum InteractionMode
	{
		MomentaryMenu, // The menu is only open while the button is held down.
		ToggledMenu, // The first button press opens the menu, another press closes it.
	}

	public static event ControllerMenuOpenedEventHandler ControllerMenuOpened;
	
	public GameObject TargetMenuRoot = null;

	public InteractionMode UserInterationMode = InteractionMode.ToggledMenu;

	public bool CloseWhenAnotherMenuOpens = true;

	public bool DebugEnabled = false;

	public void Awake()
	{
		trackedController = GetComponentInParent<SteamVR_TrackedController>();

		if (trackedController == null)
		{
			throw new System.InvalidOperationException("The \"SteamVR_TrackedController\" component needs to be added to each controller. It's located in \"SteamVR\\Extras\".");
		}
	}

	public void Start()
	{
		TryCloseMenu();
	}

	public void OnEnable()
	{
		if (trackedController.padPressed)
		{
			OnPadClickedInternal();
		}

		ControllerMenuOpened += OnControllerMenuOpened;

		trackedController.PadClicked += OnPadClicked;
		trackedController.PadUnclicked += OnPadUnclicked;
	}

	public void OnDisable()
	{
		ControllerMenuOpened -= OnControllerMenuOpened;

		trackedController.PadClicked -= OnPadClicked;
		trackedController.PadUnclicked -= OnPadUnclicked;
		
		if (trackedController.padPressed)
		{
			OnPadUnclickedInternal();
		}
	}
	
	private SteamVR_TrackedController trackedController = null;

	private bool TryOpenMenu()
	{
		bool result = false;

		if ((TargetMenuRoot != null) &&
			(TargetMenuRoot.activeSelf == false))
		{
			TargetMenuRoot.SetActive(true);

			if (DebugEnabled)
			{
				Debug.Log("Opened menu.");
			}

			if (ControllerMenuOpened != null)
			{
				var eventArgs = new ControllerMenuOpenedEventArgs();

				ControllerMenuOpened(this, eventArgs);
			}

			result = true;
		}

		return result;
	}

	private bool TryCloseMenu()
	{
		bool result = false;

		if ((TargetMenuRoot != null) &&
			TargetMenuRoot.activeSelf)
		{
			TargetMenuRoot.SetActive(false);

			if (DebugEnabled)
			{
				Debug.Log("Closed menu.");
			}

			result = true;
		}

		return result;
	}

	private void OnControllerMenuOpened(
		object sender, 
		ControllerMenuOpenedEventArgs eventArgs)
	{
		if ((Object.ReferenceEquals(sender, this) == false) &&
			CloseWhenAnotherMenuOpens)
		{
			TryCloseMenu();
		}
	}

	private void OnPadClicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		OnPadClickedInternal();
	}

	private void OnPadClickedInternal()
	{
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryMenu:
			{
				TryOpenMenu();

				break;
			}
				
			case InteractionMode.ToggledMenu:
			{
				if ((TargetMenuRoot != null) && 
					(TargetMenuRoot.activeSelf == false))
				{
					TryOpenMenu();
				}
				else
				{
					TryCloseMenu();
				}

				break;
			}

			default:
				throw new System.ComponentModel.InvalidEnumArgumentException(UserInterationMode.ToString());
		}
	}

	private void OnPadUnclicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		OnPadUnclickedInternal();
	}

	private void OnPadUnclickedInternal()
	{
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryMenu:
			{
				TryCloseMenu();

				break;
			}
				
			case InteractionMode.ToggledMenu:
			{
				// Toggles only process the click, so the unclick is ignored.

				break;
			}

			default:
				throw new System.ComponentModel.InvalidEnumArgumentException(UserInterationMode.ToString());
		}
	}
}
