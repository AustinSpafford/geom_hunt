using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesProgressChange : MonoBehaviour
{
	public enum ProgressChangeType
	{
		Assign,
		Increment,
	}

	public string ProgressName = "";
	public float ProgressValue = 0.0f;
	public ProgressChangeType ChangeType = ProgressChangeType.Assign;

	public void OnClickBegin(
		ClickSource clickSource)
	{
		var progressStorage = ProgressStorage.LocalPlayerProgressStorage;

		float newProgressValue;

		switch (ChangeType)
		{
			case ProgressChangeType.Assign:
				newProgressValue = ProgressValue;
				break;

			case ProgressChangeType.Increment:
				newProgressValue = 
					(progressStorage.GetProgressValueAsFloat(ProgressName) + ProgressValue);
				break;

			default:
				throw new System.ComponentModel.InvalidEnumArgumentException(ChangeType.ToString());
		}
		
		progressStorage.SetProgressValueAsFloat(ProgressName, newProgressValue);
	}
}
