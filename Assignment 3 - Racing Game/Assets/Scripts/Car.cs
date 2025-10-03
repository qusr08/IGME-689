using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private RacingFeatureLayerComponent racingFeatureLayerComponent;
    [Space]
    [SerializeField] private float acceleration;
    [SerializeField] private float brakeDeceleration;
    [SerializeField] private float turnSpeed;
    [SerializeField] private bool locked;

    private void Update()
    {
        // Prevent the car from gaining gravity speed when it is locked
        rigidBody.maxLinearVelocity = locked ? 0f : 80f;

        // When the race has not started, set the position and rotation of the car
        if (!locked)
        {
            return;
        }

        // Set the car's location above the track
        int index1 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 2;
        int index2 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 3;
        Vector3 position = racingFeatureLayerComponent.GetSplineKnotMidpoint(index1, index2);
        Quaternion rotation = racingFeatureLayerComponent.GetSplineKnotRotation(index1, index2);
        transform.SetPositionAndRotation(position + new Vector3(0f, 8f, 0f), rotation);
    }

    private void FixedUpdate()
    {
        if (locked)
        {
            return;
        }

        // This is stupid and for some reason the car gets stuck in the ground if the center of mass doesn't change
        // I think it might have something to do with the wheel colliders
        rigidBody.centerOfMass = rigidBody.linearVelocity.magnitude > 0.1 ? Vector3.zero : Vector3.up;

        // Move the car forward/backwards with force
        if (Input.GetKey(KeyCode.W))
        {
            rigidBody.AddForce(acceleration * transform.forward, ForceMode.Impulse);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rigidBody.AddForce(brakeDeceleration * -transform.forward, ForceMode.Impulse);
        }

        // Turn the car using torque
        int direction = Vector3.Dot(rigidBody.linearVelocity, transform.forward) > 0 ? 1 : -1;
        if (Input.GetKey(KeyCode.A))
        {
            rigidBody.AddTorque(-turnSpeed * direction * transform.up, ForceMode.Impulse);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.AddTorque(turnSpeed * direction * transform.up, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // The player loses when the car falls off the track and hits the ground
        if (collision.gameObject.name.Contains("ArcGISGameObject"))
        {
            Debug.Log("LOSE");
        }
    }
}
