using Esri.ArcGISMapsSDK.Components;
using System;
using UnityEngine;

public class RealtimeSunController : MonoBehaviour
{
	[SerializeField] private MapLocationController mapLocationController;
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;

	private void Update ( )
	{
		// Get the current UTC time
		DateTime currentLocationTime = DateTime.UtcNow;

		// Calculate the number of seconds that have elapsed within the day
		float currentDaySeconds = 0;
		currentDaySeconds += currentLocationTime.Hour * 3600f;
		currentDaySeconds += currentLocationTime.Minute * 60f;
		currentDaySeconds += currentLocationTime.Second;

		// Calculate the current longitude of the sun
		float sunLongitude = currentDaySeconds / 86400f * 360f;
		float locationLongitude = -mapLocationController.Locations[mapLocationController.CurrentLocationIndex].x;
		if (locationLongitude < 0)
		{
			locationLongitude += 360f;
		}

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
