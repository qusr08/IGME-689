using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
	[SerializeField] private Rigidbody rigidBody;
	[SerializeField] private RacingFeatureLayerComponent racingFeatureLayerComponent;
	[Space]
	[SerializeField] private List<AxleInfo> axleInfos;
	[SerializeField] private float maxMotorTorque;
	[SerializeField] private float maxBrakeTorque;
	[SerializeField] private float maxSteeringAngle;
	[SerializeField] private bool locked;

	private void Update ( )
	{
		// Prevent the car from gaining gravity speed when it is locked
		rigidBody.maxLinearVelocity = locked ? 0f : 9999f;

		// When the race has not started, set the position and rotation of the car
		if (!locked)
		{
			return;
		}

		int index1 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 2;
		int index2 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 3;
		Vector3 position = racingFeatureLayerComponent.GetSplineKnotMidpoint(index1, index2);
		Quaternion rotation = racingFeatureLayerComponent.GetSplineKnotRotation(index1, index2);
		transform.SetPositionAndRotation(position + new Vector3(0f, 8f, 0f), rotation);
	}

	private void FixedUpdate ( )
	{
		if (locked)
		{
			return;
		}

		foreach (AxleInfo axleInfo in axleInfos)
		{
			float verticalAxis = Input.GetAxis("Vertical");
			float horizontalAxis = Input.GetAxis("Horizontal");

			if (axleInfo.IsSteering)
			{
				axleInfo.LeftWheel.SteerAngleSmoothDamp = maxSteeringAngle * horizontalAxis;
				axleInfo.RightWheel.SteerAngleSmoothDamp = maxSteeringAngle * horizontalAxis;
			}

			if (axleInfo.IsMotor)
			{
				axleInfo.LeftWheel.MotorTorque = maxMotorTorque * Mathf.Max(0, verticalAxis);
				axleInfo.RightWheel.MotorTorque = maxMotorTorque * Mathf.Max(0, verticalAxis);
			}

			axleInfo.LeftWheel.BrakeTorque = maxBrakeTorque * -Mathf.Min(0, verticalAxis);
			axleInfo.RightWheel.BrakeTorque = maxBrakeTorque * -Mathf.Min(0, verticalAxis);

			axleInfo.LeftWheel.UpdateMeshTransform( );
			axleInfo.RightWheel.UpdateMeshTransform( );
		}
	}

	private void OnCollisionEnter (Collision collision)
	{
		// The player loses when the car falls off the track and hits the ground
		if (collision.gameObject.name.Contains("ArcGISGameObject"))
		{
			Debug.Log("LOSE");
		}
	}

	private float Remap (float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
}

[System.Serializable]
public class AxleInfo
{
	public Wheel LeftWheel;
	public Wheel RightWheel;
	public bool IsMotor;
	public bool IsSteering;
}

[System.Serializable]
public class Wheel
{
	public WheelCollider Collider;
	public Transform MeshTransform;
	[Range(0f, 1f)] public float SteerSmoothingSpeed;

	public float SteerAngle { get => Collider.steerAngle; set => Collider.steerAngle = value; }
	public float SteerAngleSmoothDamp { set => SteerAngle = Mathf.SmoothDampAngle(SteerAngle, value, ref steerAngleVelocity, SteerSmoothingSpeed); }
	public float MotorTorque { get => Collider.motorTorque; set => Collider.motorTorque = value; }
	public float BrakeTorque { get => Collider.brakeTorque; set => Collider.brakeTorque = value; }
	public float Radius => Collider.radius;

	private float steerAngleVelocity;

	public void UpdateMeshTransform ( )
	{
		Collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
		MeshTransform.SetPositionAndRotation(position, rotation);
	}
}