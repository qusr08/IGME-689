using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
	[SerializeField] private Rigidbody rigidBody;
	[SerializeField] private List<AxleInfo> axleInfos;
	[SerializeField] private float maxMotorTorque;
	[SerializeField] private float maxBrakeTorque;
	[SerializeField] private float maxSteeringAngle;
	[Space]
	[SerializeField] private Camera carCamera;
	[SerializeField] private float cameraHeight;
	[SerializeField] private float cameraDistance;
	[SerializeField, Range(0f, 1f)] private float cameraTilt;
	[SerializeField, Range(0f, 1f)] private float cameraSmoothingSpeed;
	[Space]
	[SerializeField] private RacingFeatureLayerComponent racingComponent;
	[SerializeField] private int currentCheckpoint;
	public bool IsLocked;

	private Vector3 cameraPosition;
	private Vector3 cameraToPosition;
	private Vector3 cameraVelocity;
	private Vector2 input;

	private void Start ( )
	{
		currentCheckpoint = 0;
	}

	private void Update ( )
	{
		// Prevent the car from gaining gravity speed when it is locked
		rigidBody.maxLinearVelocity = IsLocked ? 0f : 9999f;

		// Get input from player controls
		input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		// When the race has not started, set the position and rotation of the car
		if (!IsLocked)
		{
			return;
		}

		int index1 = racingComponent.SplineContainer.Splines[0].Count - 1;
		int index2 = racingComponent.SplineContainer.Splines[0].Count - 2;
		Vector3 position = racingComponent.GetSplineKnotMidpoint(index1, index2);
		Quaternion rotation = racingComponent.GetSplineKnotRotation(index1, index2);
		transform.SetPositionAndRotation(position + new Vector3(0f, 8f, 0f), rotation);
		UpdateCameraPosition(0f, instantly: true);
	}

	private void FixedUpdate ( )
	{
		float steerAngleDegrees = maxSteeringAngle * input.x;
		float motorTorque = maxMotorTorque * Mathf.Max(0, input.y);
		float brakeTorque = maxBrakeTorque * -Mathf.Min(0, input.y);

		if (IsLocked)
		{
			return;
		}

		UpdateCameraPosition(steerAngleDegrees);

		foreach (AxleInfo axleInfo in axleInfos)
		{
			if (axleInfo.IsSteering)
			{
				axleInfo.LeftWheel.SteerAngleSmoothDamp = steerAngleDegrees;
				axleInfo.RightWheel.SteerAngleSmoothDamp = steerAngleDegrees;
			}

			if (axleInfo.IsMotor)
			{
				axleInfo.LeftWheel.MotorTorque = motorTorque;
				axleInfo.RightWheel.MotorTorque = motorTorque;
			}

			axleInfo.LeftWheel.BrakeTorque = brakeTorque;
			axleInfo.RightWheel.BrakeTorque = brakeTorque;

			axleInfo.LeftWheel.UpdateMeshTransform( );
			axleInfo.RightWheel.UpdateMeshTransform( );
		}
	}

	private void OnCollisionEnter (Collision collision)
	{
		// The player loses when the car falls off the track and hits the ground
		if (collision.gameObject.name.Contains("ArcGISGameObject"))
		{
			racingComponent.TriggerLose( );
		}
	}

	private void OnTriggerEnter (Collider other)
	{
		if (other.TryGetComponent(out Checkpoint checkpoint))
		{
			if (checkpoint.SplineIndex != currentCheckpoint)
			{
				return;
			}

			if (++currentCheckpoint == racingComponent.CheckpointList.Count)
			{
				racingComponent.TriggerWin( );
			}
		}
	}

	private void UpdateCameraPosition (float angle, bool instantly = false)
	{
		float cameraAngleRadians = (transform.eulerAngles.y + angle) * Mathf.Deg2Rad;
		cameraToPosition = new Vector3(cameraDistance * -Mathf.Sin(cameraAngleRadians), cameraHeight, cameraDistance * -Mathf.Cos(cameraAngleRadians));
		cameraPosition = instantly ? cameraToPosition : Vector3.SmoothDamp(cameraPosition, cameraToPosition, ref cameraVelocity, cameraSmoothingSpeed);

		carCamera.transform.position = transform.position + cameraPosition;
		carCamera.transform.LookAt(transform.position + ((1 - cameraTilt) * cameraHeight * Vector3.up));
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