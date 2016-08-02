using UnityEngine;
using System.Collections;

public class DestroyRigidbodiesInChildren : MonoBehaviour
{
	public void Start()
	{
		foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
		{
			Destroy(rigidbody);
		}
	}
}