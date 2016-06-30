using UnityEngine;
using System.Collections;

public class ClickCausesSpawnerReset : ClickCausesInstantAction
{
	public override void OnInstantClick(
		ClickSource clickSource)
	{
		Debug.LogWarning("TODO: Reset the spawner.");
	}
}
