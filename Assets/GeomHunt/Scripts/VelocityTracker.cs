using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VelocityTracker : MonoBehaviour
{
	[Tooltip("Increasing the window-size gives smoother results, but is less responsive the flicking motions.")]
	[SerializeField] // Make this only exposed/changeable in the editor.
	private float AveragingWindowSeconds = 0.2f;

	public Vector3 AverageLinearVelocity { get; private set; }
	public Vector3 AverageAngularVelocity { get; private set; }

	public void Start()
	{
		AverageLinearVelocity = Vector3.zero;
		AverageAngularVelocity = Vector3.zero;

		lastKnownPosition = transform.position;
		lastKnownOrientation = transform.rotation;
	}

	public void FixedUpdate()
	{
		Vector3 immediateLinearVelocity;
		Vector3 immediateAngularVelocity;

		// Compute our immediate velocities (which will be incredibly noisey between updates).
		{
			immediateLinearVelocity = 
				(transform.position - lastKnownPosition) / Time.fixedDeltaTime;

			Quaternion orientationChange = 
				(transform.rotation * Quaternion.Inverse(lastKnownOrientation));

			Vector3 orientationChangeAxis;
			float orientationChangeAngle;
			orientationChange.ToAngleAxis(
				out orientationChangeAngle,
				out orientationChangeAxis);
			
			// NOTE: ToAngleAxis() returns an axis with NaN-components when the angle approaches zero.
			immediateAngularVelocity = (
				(orientationChangeAngle > Mathf.Epsilon) ?
					(orientationChangeAxis * (orientationChangeAngle / Time.fixedDeltaTime)) :
					Vector3.zero);
				
			lastKnownPosition = transform.position;
			lastKnownOrientation = transform.rotation;
		}

		// Slide the averaging-window forward.
		{
			int targetHistoryLength = Mathf.CeilToInt(AveragingWindowSeconds / Time.fixedDeltaTime);

			if (linearVelocityIncrementHistory.Count <= targetHistoryLength)
			{
				Vector3 linearVelocityIncrement = (immediateLinearVelocity / targetHistoryLength);
				Vector3 angularVelocityIncrement = (immediateAngularVelocity / targetHistoryLength);
			
				AverageLinearVelocity += linearVelocityIncrement;
				AverageAngularVelocity += angularVelocityIncrement;

				linearVelocityIncrementHistory.Enqueue(linearVelocityIncrement);
				angularVelocityIncrementHistory.Enqueue(angularVelocityIncrement);
			}
			
			if (linearVelocityIncrementHistory.Count > targetHistoryLength)
			{
				AverageLinearVelocity -= linearVelocityIncrementHistory.Dequeue();
				AverageAngularVelocity -= angularVelocityIncrementHistory.Dequeue();
			}
		}
	}

	private Vector3 lastKnownPosition = Vector3.zero;
	private Quaternion lastKnownOrientation = Quaternion.identity;

	private Queue<Vector3> linearVelocityIncrementHistory = new Queue<Vector3>();
	private Queue<Vector3> angularVelocityIncrementHistory = new Queue<Vector3>();
}
