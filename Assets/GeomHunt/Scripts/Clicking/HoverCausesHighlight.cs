using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class HoverCausesHighlight : MonoBehaviour
{
	public void OnHoverBegin()
	{
		// TODO: Fade in highlight.
	}

	public void OnHoverEnd()
	{
		// TODO: Fade out highlight.
	}
}
