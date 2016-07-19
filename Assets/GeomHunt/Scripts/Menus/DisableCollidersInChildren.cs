using UnityEngine;
using System.Collections;

public class DisableCollidersInChildren : MonoBehaviour
{
	public void Start()
	{
		foreach (Collider collider in GetComponentsInChildren<Collider>())
		{
			collider.enabled = false;
		}
	}
}