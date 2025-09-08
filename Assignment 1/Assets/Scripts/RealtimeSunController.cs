using Esri.ArcGISMapsSDK.Components;
using System;
using UnityEngine;

public class RealtimeSunController : MonoBehaviour
{
	[SerializeField] private MapLocationController mapLocationController;
	[SerializeField] private ArcGISCameraComponent arcGISCameraController;
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;

	private float earthCircumferenceMeters = 40075f;
	private float earthAxialTiltRadians = 23.5f * Mathf.Deg2Rad;

	private void Update ( )
	{
		// Get the current UTC time and map location
		DateTime now = DateTime.UtcNow;
		Vector2 currentLocationCoordinates = mapLocationController.Locations[mapLocationController.CurrentLocationIndex];

		// Calculate the number of seconds that have elapsed within the day
		float currentDaySeconds = 0;
		currentDaySeconds += now.Hour * 3600f;
		currentDaySeconds += now.Minute * 60f;
		currentDaySeconds += now.Second;

		// Calculate the current longitude of the sun
		float sunLongitude = currentDaySeconds / 86400f * 360f;

		// Adjust the longitude of the map location to be 0 to 360 degrees instead of -180 to 180
		float locationLongitude = -currentLocationCoordinates.x;
		if (locationLongitude < 0)
		{
			locationLongitude += 360f;
		}

		float eclipticLongitude = Mathf.PI * 2f * now.DayOfYear / 365f;
		float sunDeclination = Mathf.Asin(Mathf.Sin(eclipticLongitude) * Mathf.Sin(earthAxialTiltRadians));
		float hourAngle = Mathf.Acos(-Mathf.Tan(currentLocationCoordinates.y) * Mathf.Tan(sunDeclination));

		Debug.Log(hourAngle);

		// NEED TO FIX MATH
		// Find the angle offset between the locations longitude and the suns longitude
		// 90 - locationLongitude should give the rotation of the skybox at 0 degrees longitude
		// Adding the sunLongitude should give the current position of the sun in the sky
		// But this does not work, something is wrong with the math
		float sunRotation = 90 - locationLongitude + sunLongitude;

		// Set the rotation of the directional light to match the sun
		transform.rotation = Quaternion.Euler(sunRotation, 0f, 0f);
	}
}
