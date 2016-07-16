using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject[] SpawneePrefabs = null;

	public int TotalSpawneeCount = 100;
	public float SpawnsPerSecond = 10.0f;

	public string SpawneeContainerName = "spawned_objects";

	public void Update()
	{
		if ((spawneeCount < TotalSpawneeCount) &&
			(SpawneePrefabs.Length > 0))
		{
			remainingSecondsUntilSpawn -= Time.deltaTime;

			// While we're still due to spawn objects.
			while (remainingSecondsUntilSpawn <= 0.0f)
			{
				// Set the spawnee as either a sibling or cousin, rather than a direct child.
				// This allows us to use the scale to define the spawning volume.
				GameObject spawneesContainer = null;
				{
					if (string.IsNullOrEmpty(SpawneeContainerName))
					{
						spawneesContainer = transform.parent.gameObject;
					}
					else
					{
						Transform existingSpawneesContainerTransform = 
							transform.parent.Find(SpawneeContainerName);
						
						if (existingSpawneesContainerTransform != null)
						{
							spawneesContainer = existingSpawneesContainerTransform.gameObject;
						}
						else
						{
							spawneesContainer = new GameObject(SpawneeContainerName);

							spawneesContainer.transform.parent = transform.parent;
							
							spawneesContainer.transform.localPosition = Vector3.zero;
							spawneesContainer.transform.localScale = Vector3.one;
							spawneesContainer.transform.localScale = Vector3.one;
						}
					}
				}

				Vector3 localRandomSpaneePosition = 
					new Vector3(
						Random.Range(-0.5f, 0.5f),
						Random.Range(-0.5f, 0.5f),
						Random.Range(-0.5f, 0.5f));
				
				Vector3 randomSpawneePosition = 
					transform.TransformPoint(localRandomSpaneePosition);

				GameObject spawnee = 
					GameObject.Instantiate(
						SpawneePrefabs[randomSequence.Next(SpawneePrefabs.Length)],
						randomSpawneePosition,
						Random.rotationUniform) as GameObject;

				spawnee.transform.parent = spawneesContainer.transform;

				++spawneeCount;
				remainingSecondsUntilSpawn += (1.0f / SpawnsPerSecond);
			}
		}
	}
	
	private int spawneeCount = 0;

	private float remainingSecondsUntilSpawn = 0.0f;

	private System.Random randomSequence = new System.Random();
}
