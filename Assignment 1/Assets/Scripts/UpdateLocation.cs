using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpdateLocation : MonoBehaviour
{
	[SerializeField] private List<Vector2> locations;
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;
	[SerializeField] private ArcGISCameraComponent arcGISCameraComponent;

	private int currentLocationIndex;
	private List<CustomGeoObject> customGeoObjects;

	private void Awake ( )
	{
		// Get a list of all the custom geo objects in the scene
		customGeoObjects = FindObjectsByType<CustomGeoObject>(FindObjectsSortMode.None).ToList( );
	}

	private void Start ( )
	{
		// Set the map to the first location
		currentLocationIndex = 0;
		SetLocationFromIndex(currentLocationIndex);
	}

	private void Update ( )
	{
		// When the user presses M, switch to the next location
		if (Input.GetKeyDown(KeyCode.M))
		{
			GoToNextLocation( );
		}
	}

	private void GoToNextLocation ( )
	{
		// Make sure the current location index loops back to the start after the last location has been visited
		currentLocationIndex = (currentLocationIndex + 1) % locations.Count;
		SetLocationFromIndex(currentLocationIndex);
	}

	private void SetLocationFromIndex (int index)
	{
		// Update the map origin
		arcGISMapComponent.OriginPosition = new ArcGISPoint(locations[index].x, locations[index].y, 0, ArcGISSpatialReference.WGS84( ));
		arcGISMapComponent.UpdateHPRoot( );

		// Update the camera position
		ArcGISLocationComponent cameraLocation = arcGISCameraComponent.GetComponent<ArcGISLocationComponent>( );
		cameraLocation.Position = new ArcGISPoint(locations[index].x, locations[index].y, cameraLocation.Position.Z, ArcGISSpatialReference.WGS84( ));

		// Update all 3D objects in the scene
		foreach (CustomGeoObject obj in customGeoObjects)
		{
			obj.PrintCoordinates( );
		}
	}
}
