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
	
	public GameObject MenuRootPrefab = null;

	public InteractionMode UserInterationMode = InteractionMode.ToggledMenu;

	public bool CloseWhenAnotherMenuOpens = true;

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
		ControllerMenuOpened += OnControllerMenuOpened;

		trackedController.PadClicked += OnPadClicked;
		trackedController.PadUnclicked += OnPadUnclicked;
	}

	public void OnDisable()
	{
		ControllerMenuOpened -= OnControllerMenuOpened;

		trackedController.PadClicked -= OnPadClicked;
		trackedController.PadUnclicked -= OnPadUnclicked;
	}
	
	private SteamVR_TrackedController trackedController = null;

	private GameObject InstantiatedMenuRoot = null;

	private void OpenMenu()
	{
		if (InstantiatedMenuRoot == null)
		{
			InstantiatedMenuRoot = GameObject.Instantiate(MenuRootPrefab);

			InstantiatedMenuRoot.transform.parent = transform;

			InstantiatedMenuRoot.transform.localPosition = Vector3.zero;
			InstantiatedMenuRoot.transform.localRotation = Quaternion.identity;
			InstantiatedMenuRoot.transform.localScale = Vector3.one;

			if (ControllerMenuOpened != null)
			{
				var eventArgs = new ControllerMenuOpenedEventArgs();

				ControllerMenuOpened(this, eventArgs);
			}
		}
	}

	private void CloseMenu()
	{
		if (InstantiatedMenuRoot != null)
		{
			DestroyObject(InstantiatedMenuRoot);

			InstantiatedMenuRoot = null;
		}
	}

	private void OnControllerMenuOpened(
		object sender, 
		ControllerMenuOpenedEventArgs eventArgs)
	{
		if ((Object.ReferenceEquals(sender, this) == false) &&
			CloseWhenAnotherMenuOpens)
		{
			CloseMenu();
		}
	}

	private void OnPadClicked(
		object sender,
		ClickedEventArgs eventArgs)
	{
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryMenu:
			{
				OpenMenu();

				break;
			}
				
			case InteractionMode.ToggledMenu:
			{
				if (InstantiatedMenuRoot == null)
				{
					OpenMenu();
				}
				else
				{
					CloseMenu();
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
		switch (UserInterationMode)
		{
			case InteractionMode.MomentaryMenu:
			{
				CloseMenu();

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
