using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ClickTarget))] // The click-target is what sends us messages.
abstract public class ClickCausesInstantAction : MonoBehaviour
{
	public abstract void OnInstantClick(ClickSource clickSource);

	public void OnClickBegin(
		ClickSource clickSource)
	{
		OnInstantClick(clickSource);

		// Enforce the instant-action behavior by immediately freeing ourselves.
		clickSource.TryTerminateClick();
	}
}
