﻿using UnityEngine;
using System.Collections;

public class Turntable : MonoBehaviour
{
	public Vector3 RotationAxis = Vector3.up;

	public float DegreesPerSecond = 30.0f;

	public void Update()
	{
		transform.rotation *= 
			Quaternion.AngleAxis(
				(DegreesPerSecond * Time.deltaTime),
				RotationAxis);
	}
}