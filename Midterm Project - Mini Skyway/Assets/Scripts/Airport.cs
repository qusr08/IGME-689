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

	public AirportType AirportType { get => _airportType; private set => _airportType = value; }

	private void OnValidate ( )
	{
		locationComponent = GetComponent<ArcGISLocationComponent>( );
		ArcGISPoint location = locationComponent.Position;
		locationComponent.Position = new ArcGISPoint(location.X, location.Y, 0f, location.SpatialReference);
		locationComponent.Rotation = new ArcGISRotation(0f, 90f, 0f);

		hpTransform = GetComponent<HPTransform>( );
		hpTransform.LocalScale = new float3(size, height, size);
	}

	private void OnMouseDown ( )
	{
		Debug.Log("CLICK");
	}
}
