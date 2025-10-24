using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Airport : MonoBehaviour
{
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private Shape mainShape;
	[SerializeField] private PassengerContainer _passengerContainer;
	[Space]
	[SerializeField] private AirportData _data;
	[SerializeField] private AirportIndicator _indicator;

	public AirportIndicator Indicator { get => _indicator; set => _indicator = value; }

	public ShapeType Type { get => mainShape.Type; set => mainShape.Type = value; }

	public bool IsRendererVisible => mainShape.IsRendererVisible;

	public PassengerContainer PassengerContainer { get => _passengerContainer; private set => _passengerContainer = value; }

	public AirportData Data
	{
		get => _data;
		set
		{
			_data = value;
			Coordinates = new ArcGISPoint(_data.Longitude, _data.Latitude, 0f, Coordinates.SpatialReference);
		}
	}

	public ArcGISPoint Coordinates { get => locationComponent.Position; set => locationComponent.Position = value; }

	public ArcGISRotation Rotation { get => locationComponent.Rotation; private set => locationComponent.Rotation = value; }

	private void Update()
	{
		if (Random.Range(0f, 1f) < 0.005f)
		{
			PassengerContainer.AddRandomPassenger(excludedType: Type);
		}
	}
}
