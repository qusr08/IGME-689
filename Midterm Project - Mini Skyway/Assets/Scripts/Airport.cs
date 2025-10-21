using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AirportType
{
	CIRCLE, TRIANGLE, SQUARE
}

public class Airport : MonoBehaviour
{
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private GameObject squareObject;
	[SerializeField] private GameObject circleObject;
	[SerializeField] private GameObject triangleObject;
	[SerializeField, Min(1)] private int size;
	[Space]
	[SerializeField] private AirportType _airportType;
	[SerializeField] private AirportData _data;

	public AirportType AirportType {
		get => _airportType;
		set
		{
			_airportType = value;
			squareObject.SetActive(_airportType == AirportType.SQUARE);
			circleObject.SetActive(_airportType == AirportType.CIRCLE);
			triangleObject.SetActive(_airportType == AirportType.TRIANGLE);
		}
	}
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
	}

    private void Start()
    {
        AirportType = (AirportType)Random.Range(0, 3);
    }

    private void OnMouseDown ( )
	{
		Debug.Log("CLICK");
	}
}
