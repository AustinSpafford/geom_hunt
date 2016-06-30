using UnityEngine;
using System.Collections;

public class ClickCausesExplosion : ClickCausesInstantAction
{
	public override void OnInstantClick(
		ClickSource clickSource)
	{
		Debug.LogWarning("TODO: KABOOM!");
	}
}
