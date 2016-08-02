using UnityEngine;
using System.Collections;

public class DisableDuringPlay : MonoBehaviour
{
	public void Awake()
	{
		gameObject.SetActive(false);
	}
}
