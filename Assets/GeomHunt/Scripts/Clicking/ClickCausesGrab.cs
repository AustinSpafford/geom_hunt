using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesGrab : MonoBehaviour
{
	public void OnClickBegin(
		ClickSource clickSource)
	{
		VelociGripper velociGripper = clickSource.GetComponent<VelociGripper>();

		velociGripper.GripObject(this.gameObject);
	}

	public void OnClickEnd(
		ClickSource clickSource)
	{
		VelociGripper velociGripper = clickSource.GetComponent<VelociGripper>();

		velociGripper.ReleaseObject();
	}
}
