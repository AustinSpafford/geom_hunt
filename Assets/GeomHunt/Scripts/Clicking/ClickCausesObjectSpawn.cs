using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ClickTarget))] // The click-target is what sends us messages.
public class ClickCausesObjectSpawn : MonoBehaviour
{
	public GameObject SpawneePrefab = null;

	public Transform SpawneeParent = null;

	public bool ForwardClickToSpawnee = false;

	public void OnClickBegin(
		ClickSource clickSource)
	{
		if (SpawneePrefab != null)
		{
			Vector3 spawneePosition = 
				Vector3.Lerp(
					transform.position,
					clickSource.transform.position,
					0.5f);

			Quaternion spawneeOrientation = transform.rotation;

			GameObject spawnee = GameObject.Instantiate(SpawneePrefab);
		
			spawnee.transform.parent = SpawneeParent;

			spawnee.transform.position = spawneePosition;
			spawnee.transform.rotation = spawneeOrientation;
			spawnee.transform.localScale = SpawneePrefab.transform.localScale;

			if (ForwardClickToSpawnee)
			{
				var spawneeClickTarget = spawnee.GetComponent<ClickTarget>();

				if (spawneeClickTarget)
				{
					clickSource.ForceClickOnTarget(spawneeClickTarget);
				}
			}
		}
	}
}
