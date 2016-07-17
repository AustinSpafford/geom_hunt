using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct ProgressChangedEventArgs
{
	public string ProgressName;

	public bool OldValueAsBool;
	public bool NewValueAsBool;

	public float OldValueAsFloat;
	public float NewValueAsFloat;

	public int OldValueAsInt;
	public int NewValueAsInt;
}

public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs eventArgs);

public class ProgressStorage : MonoBehaviour
{
	public static event ProgressChangedEventHandler ProgressChanged;

	public static ProgressStorage LocalPlayerProgressStorage
	{
		get
		{
			UpdateCachedLocalPlayerStorageReference();

			return cachedLocalPlayerProgressStorage;
		}
	}

	public bool DebugEnabled = false;

	public bool GetProgressValueAsBool(
		string progressName)
	{
		return GetOrCreateProgressEntry(progressName).ProgressValueAsBool;	
	}

	public void SetProgressValueAsBool(
		string progressName,
		bool newValue)
	{
		SetProgressValueInternal(
			progressName, 
			(newValue ? 1.0f : 0.0f));
	}

	public float GetProgressValueAsFloat(
		string progressName)
	{
		return GetOrCreateProgressEntry(progressName).ProgressValueAsFloat;
	}

	public void SetProgressValueAsFloat(
		string progressName,
		float newValue)
	{
		SetProgressValueInternal(progressName, newValue);
	}

	public float GetProgressValueAsInt(
		string progressName)
	{
		return GetOrCreateProgressEntry(progressName).ProgressValueAsInt;
	}

	public void SetProgressValueAsInt(
		string progressName,
		int newValue)
	{
		SetProgressValueInternal(progressName, newValue);
	}

	private class ProgressEntry
	{
		public string ProgressName { get; set; }

		public float ProgressValueStorage { get; set; }

		public bool ProgressValueAsBool
		{
			get { return (Mathf.Approximately(ProgressValueAsFloat, 0.0f) == false); }
		}

		public float ProgressValueAsFloat
		{
			get { return ProgressValueStorage; }
		}

		public int ProgressValueAsInt
		{
			get { return Mathf.RoundToInt(ProgressValueAsFloat); }
		}
	}

	private static ProgressStorage cachedLocalPlayerProgressStorage = null;

	private Dictionary<string, ProgressEntry> progressEntries = new Dictionary<string, ProgressEntry>();
	
	private static void UpdateCachedLocalPlayerStorageReference()
	{
		// If we need initialize or reinitialize the cache (eg. after a total scene-wipe).
		if (cachedLocalPlayerProgressStorage == null)
		{
			ProgressStorage[] progressStorages = FindObjectsOfType<ProgressStorage>();

			if (progressStorages.Length == 0)
			{
				throw new System.InvalidOperationException(
					"There are no active ProgressStorage components, thus there's no progress storage for the local player.");
			}

			if (progressStorages.Length > 1)
			{
				throw new System.InvalidOperationException(
					"There are multiple active ProgressStorage components, thus it's ambiguous as to which one is for the local player. This likely means ProgressStorage itself needs to be further iterated upon to remove the ambiguity.");
			}

			cachedLocalPlayerProgressStorage = progressStorages[0];
		}
	}

	private ProgressEntry GetOrCreateProgressEntry(
		string progressName)
	{
		ProgressEntry result = null;

		if (string.IsNullOrEmpty(progressName))
		{
			throw new System.ArgumentNullException(
				progressName, 
				"Progress names must be a non-empty string.");
		}

		if (progressEntries.TryGetValue(progressName, out result))
		{
			// Great! We're done.
		}
		else
		{
			result = new ProgressEntry
			{
				ProgressName = progressName,
				ProgressValueStorage = 0.0f,
			};

			progressEntries.Add(result.ProgressName, result);
		}

		return result;
	}

	private void SetProgressValueInternal(
		string progressName,
		float newValue)
	{
		ProgressEntry progressEntry = GetOrCreateProgressEntry(progressName);

		if (newValue != progressEntry.ProgressValueStorage)
		{
			var eventArgs = new ProgressChangedEventArgs();

			eventArgs.ProgressName = progressName;

			eventArgs.OldValueAsBool = progressEntry.ProgressValueAsBool;
			eventArgs.OldValueAsFloat = progressEntry.ProgressValueAsFloat;
			eventArgs.OldValueAsInt = progressEntry.ProgressValueAsInt;

			// Now that the old values have been recorded, actualize the progress-change.
			progressEntry.ProgressValueStorage = newValue;			

			eventArgs.NewValueAsBool = progressEntry.ProgressValueAsBool;
			eventArgs.NewValueAsFloat = progressEntry.ProgressValueAsFloat;
			eventArgs.NewValueAsInt = progressEntry.ProgressValueAsInt;
			
			if (DebugEnabled)
			{
				Debug.LogFormat(
					"Changing progress [{0}] from [{1:F2}] to [{2:F2}].",
					eventArgs.ProgressName,
					eventArgs.OldValueAsFloat,
					eventArgs.NewValueAsFloat);
			}

			if (ProgressChanged != null)
			{
				ProgressChanged(this, eventArgs);
			}
		}
	}
}
