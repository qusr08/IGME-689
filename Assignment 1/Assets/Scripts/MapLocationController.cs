using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapLocationController : MonoBehaviour
{
	[SerializeField] private List<Vector2> _locations;
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;
	[SerializeField] private ArcGISCameraComponent arcGISCameraComponent;

	// Locations are set up as (Longitude, Latitude)
	public List<Vector2> Locations => _locations;
	public int CurrentLocationIndex { get; private set; }

	private List<GeographicObject> geographicObjects;

	private void Awake ( )
	{
		// Get a list of all the custom geo objects in the scene
		geographicObjects = FindObjectsByType<GeographicObject>(FindObjectsSortMode.None).ToList( );
	}

	private void Start ( )
	{
		// Set the map to the first location
		CurrentLocationIndex = 0;
		SetLocationFromIndex(CurrentLocationIndex);
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
		CurrentLocationIndex = (CurrentLocationIndex + 1) % _locations.Count;
		SetLocationFromIndex(CurrentLocationIndex);
	}

	private void SetLocationFromIndex (int index)
	{
		// Update the map origin
		arcGISMapComponent.OriginPosition = new ArcGISPoint(_locations[index].x, _locations[index].y, 0, ArcGISSpatialReference.WGS84( ));
		arcGISMapComponent.UpdateHPRoot( );

		// Update the camera position
		ArcGISLocationComponent cameraLocation = arcGISCameraComponent.GetComponent<ArcGISLocationComponent>( );
		cameraLocation.Position = new ArcGISPoint(_locations[index].x, _locations[index].y, cameraLocation.Position.Z, ArcGISSpatialReference.WGS84( ));

		// Update all 3D objects in the scene
		foreach (GeographicObject obj in geographicObjects)
		{
			obj.PrintCoordinates( );
		}
	}
}
