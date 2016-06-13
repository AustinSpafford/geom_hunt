using UnityEngine;
using System.Collections;

public class GrippedObjectSpawner : MonoBehaviour
{
	public GameObject GrippedObjectPrefab = null;

	public Transform ParentForSpawnedObjects = null;

	public void Start()
	{
		if (GrippedObjectPrefab != null)
		{
			VelociGripper gripper = GetComponent<VelociGripper>();

			if (gripper != null)
			{
				GameObject newObject = 
					GameObject.Instantiate(
						GrippedObjectPrefab,
						transform.position,
						transform.rotation) as GameObject;
				
				newObject.transform.parent = ParentForSpawnedObjects;

				gripper.GripObject(newObject);
			}
		}
	}
}
