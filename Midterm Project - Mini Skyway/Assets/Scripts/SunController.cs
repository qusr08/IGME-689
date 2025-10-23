using UnityEngine;

public class SunController : MonoBehaviour
{
	[SerializeField] private float rotationSpeed;

	private void Update()
	{
		transform.rotation *= Quaternion.Euler(0f, Time.deltaTime * rotationSpeed, 0f);
	}
}
