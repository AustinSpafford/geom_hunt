using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesSpawnerReset : MonoBehaviour
{
	public void OnClickBegin(
		ClickSource clickSource)
	{
		// TODO: Reset the spawned objects.
	}
}
