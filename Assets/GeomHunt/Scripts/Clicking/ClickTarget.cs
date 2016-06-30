using UnityEngine;
using System.Collections;

public class ClickTarget : MonoBehaviour
{
	public int HoverDepth { get; private set; }
	public ClickSource ActiveClickSource { get; private set; }
	
	public bool IsHovered { get { return (HoverDepth > 0); } }
	public bool IsClicked { get { return (ActiveClickSource != null); } }

	public bool DebugEnabled = false;

	public void EnterHoverScope()
	{
		bool wasHovered = IsHovered;

		HoverDepth++;
		
		if (DebugEnabled)
		{
			Debug.LogFormat("Entering hover scope, hover-depth is now <b>{0}</b>.", HoverDepth);
		}

		if (wasHovered == false)
		{
			gameObject.SendMessage(
				"OnHoverBegin",
				SendMessageOptions.DontRequireReceiver);
		}
	}

	public void LeaveHoverScope()
	{
		if (IsHovered == false)
		{
			throw new System.InvalidOperationException("Leaving a hover that we never entered. This should never happen.");
		}
		
		HoverDepth--;
		
		if (DebugEnabled)
		{
			Debug.LogFormat("Leaving hover scope, hover-depth is now <b>{0}</b>.", HoverDepth);
		}

		if (IsHovered == false)
		{
			gameObject.SendMessage(
				"OnHoverEnd",
				SendMessageOptions.DontRequireReceiver);
		}
	}

	public void EnterClickScope(
		ClickSource clickSource)
	{
		// If a second click source (such as the other hand) is beginning a click, 
		// force ourselves to cleanly leave the prior click to avoid any fighting, such
		// as being grabbed by two controllers at once.
		if (ActiveClickSource != null)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat("A second click source is incoming while we're already clicked. Forcing an early termination on the first click-source.");
			}

			ActiveClickSource.TryTerminateClick();
			
			if (ActiveClickSource != null)
			{
				throw new System.InvalidOperationException("Failed to leave a prior click before entering a new click. This might happen if click-respondants pull shenanigans.");
			}
		}
		
		if (DebugEnabled)
		{
			Debug.LogFormat("Entering a click-scope under <b>{0}</b>.", clickSource);
		}

		ActiveClickSource = clickSource;
		
		gameObject.SendMessage(
			"OnClickBegin",
			clickSource,
			SendMessageOptions.DontRequireReceiver);
	}

	public void LeaveClickScope(
		ClickSource clickSource)
	{
		if (clickSource != ActiveClickSource)
		{
			throw new System.InvalidOperationException(
				string.Format(
					"Being asked to leave a click that we were not engaged in. Prior click-source was <b>{0}</b>, and new click-source is <b>{1}</b>",
					((ActiveClickSource != null) ? ActiveClickSource.name : "NULL"),
					clickSource.name));
		}
		
		if (DebugEnabled)
		{
			Debug.LogFormat("Leaving a click-scope under <b>{0}</b>.", clickSource);
		}
		
		gameObject.SendMessage(
			"OnClickEnd",
			clickSource,
			SendMessageOptions.DontRequireReceiver);

		ActiveClickSource = null;
	}
}
