using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesExplosion : MonoBehaviour
{
	public void OnClickBegin(
		ClickSource clickSource)
	{
		// TODO: Explode.
	}
}
