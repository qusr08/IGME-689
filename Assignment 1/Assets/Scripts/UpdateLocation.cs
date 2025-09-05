using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLocation : MonoBehaviour
{
	[SerializeField] private List<Vector2> locations;
	[SerializeField] private int currentLocationIndex;
	[SerializeField] private ArcGISMapComponent arcGISMap;
	[SerializeField] private ArcGISLocationComponent arcGISCameraLocation;

	private void Awake ( )
	{
		currentLocationIndex = 0;
		SetLocationFromIndex(currentLocationIndex);
	}

	private void Update ( )
	{
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			GoToNextLocation( );
		}
	}

	private void GoToNextLocation ( )
	{
		currentLocationIndex = (currentLocationIndex + 1) % locations.Count;
		SetLocationFromIndex(currentLocationIndex);
	}

	private void SetLocationFromIndex (int index)
	{
		ArcGISPoint point = new ArcGISPoint(locations[index].x, locations[index].y, ArcGISSpatialReference.WGS84());
		arcGISMap.OriginPosition = point;
		arcGISCameraLocation.Position = point;
	}
}
