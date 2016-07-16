using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesProgressChange : MonoBehaviour
{
	public string ProgressName = "";
	public float ProgressValue = 0.0f;

	public void OnClickBegin(
		ClickSource clickSource)
	{
		var progressStorage = ProgressStorage.LocalPlayerProgressStorage;
		
		progressStorage.SetProgressValueAsFloat(ProgressName, ProgressValue);
	}
}
