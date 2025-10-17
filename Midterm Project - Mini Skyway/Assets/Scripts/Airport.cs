using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;

public enum AirportType
{
	CIRCLE, TRIANGLE, SQUARE
}

public class Airport : MonoBehaviour
{
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private HPTransform hpTransform;
	[SerializeField, Min(1)] private int height;
	[SerializeField, Min(1)] private int size;
	[Space]
	[SerializeField] private AirportType _airportType;
	[SerializeField] private AirportData _data;

	public AirportType AirportType { get => _airportType; private set => _airportType = value; }
	public ArcGISPoint Coordinates { get => locationComponent.Position; set => locationComponent.Position = value; }
	public AirportData Data
	{
		get => _data;
		set
		{
			_data = value;
			Coordinates = new ArcGISPoint(_data.Longitude, _data.Latitude, 0f, Coordinates.SpatialReference);
		}
	}

	private void OnValidate ( )
	{
		locationComponent = GetComponent<ArcGISLocationComponent>( );
		if (Coordinates != null)
		{
			Coordinates = new ArcGISPoint(Coordinates.X, Coordinates.Y, 0f, Coordinates.SpatialReference);
            locationComponent.Rotation = new ArcGISRotation(0f, 90f, 0f);
        }

		hpTransform = GetComponent<HPTransform>( );
		hpTransform.LocalScale = new float3(size, height, size);
	}

	private void OnMouseDown ( )
	{
		Debug.Log("CLICK");
	}
}
