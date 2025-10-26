using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlaneColor
{
	Red, Green, Blue, Yellow, Pink
}

public class Airplane : MonoBehaviour
{
	[SerializeField] private AirportManager airportManager;
	[SerializeField] private UIManager uiManager;
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private MeshRenderer[] meshRenderers;
	[SerializeField] private PassengerContainer _passengerContainer;
	[SerializeField, Min(0f)] private int altitude;
	[SerializeField] private float speed;
	[Space]
	[SerializeField] private List<Airport> _airportPath;
	[SerializeField] private int currentPathIndex;
	[SerializeField] private Airport toAirport;
	[SerializeField] private Airport fromAirport;
	[SerializeField] private PlaneColor _planeColor;
	[SerializeField] private bool _isDocked;

	private float progress;
	private float progressSpeed;
	private Material airplaneMaterial;

	public ArcGISPoint Coordinates { get => locationComponent.Position; private set => locationComponent.Position = value; }
	public ArcGISRotation Rotation { get => locationComponent.Rotation; private set => locationComponent.Rotation = value; }
	public PlaneColor Color
	{
		get => _planeColor;
		set
		{
			_planeColor = value;
			airplaneMaterial.color = uiManager.PlaneColorDictionary[_planeColor];
			PassengerContainer.LocalMaterial.color = uiManager.PlaneColorDictionary[_planeColor];
		}
	}
	public bool IsDocked
	{
		get => _isDocked;
		set
		{
			_isDocked = value;
			UndockFlag = false;

			if (toAirport == null)
			{
				return;
			}

			if (_isDocked)
			{
				toAirport.DockedAirplanes.Add(this);
			}
			else
			{
				toAirport.DockedAirplanes.Remove(this);
			}
		}
	}
	public PassengerContainer PassengerContainer => _passengerContainer;
	public List<Airport> AirportPath { get => _airportPath; private set => _airportPath = value; }
	public List<GameObject> FlightPathObjects { get; private set; }
	public bool IsVisible
	{
		get => meshRenderers[0].enabled;
		set
		{
			foreach (MeshRenderer meshRenderer in meshRenderers)
			{
				meshRenderer.enabled = value;
			}
		}
	}
	public bool UndockFlag { get; set; }

	private void Awake()
	{
		airportManager = FindFirstObjectByType<AirportManager>();
		uiManager = FindFirstObjectByType<UIManager>();
		AirportPath = new List<Airport>();
		FlightPathObjects = new List<GameObject>();

		airplaneMaterial = new Material(meshRenderers[0].material);
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.material = airplaneMaterial;
		}

		progress = 1f;
		IsDocked = false;
		UndockFlag = false;
		currentPathIndex = 0;
		IsVisible = false;
	}

	private void Update()
	{
		if (AirportPath.Count < 2 || !IsVisible || uiManager.State != UIState.GAME)
		{
			return;
		}

		if (IsDocked)
		{
			if (!_passengerContainer.IsAtCapacity && toAirport.PassengerContainer.CurrentPassengers > 0 && !UndockFlag)
			{
				return;
			}

			IsDocked = false;
			FlyToNextAirport();
		}

		ProgressFlight(Time.deltaTime);
	}

	private void OnDestroy()
	{
		ClearAiportPath();
	}

	private void ProgressFlight(float deltaTime)
	{
		progress += Time.deltaTime * progressSpeed;

		if (progress >= 1f)
		{
			DropOffPassengers();
			IsDocked = true;
		}

		InterpolateBetweenAirports(progress);
	}

	private void InterpolateBetweenAirports(float t)
	{
		if (fromAirport == toAirport)
		{
			Coordinates = new ArcGISPoint(fromAirport.Coordinates.X, fromAirport.Coordinates.Y, altitude, Coordinates.SpatialReference);
			Rotation = new ArcGISRotation(0f, 180f, 0f);
			return;
		}

		ArcGISPoint interCoord = airportManager.IntermediateCoordinate(fromAirport.Coordinates, toAirport.Coordinates, t);
		//float interAltitude = (-1 * Mathf.Pow(2 * t - 1, 4) + 1) * altitude; // -4 * t * (t - 1) * altitude;

		//Coordinates = new ArcGISPoint(interCoord.X, interCoord.Y, interAltitude, Coordinates.SpatialReference);
		Coordinates = new ArcGISPoint(interCoord.X, interCoord.Y, altitude, Coordinates.SpatialReference);
		Rotation = new ArcGISRotation(airportManager.Bearing(interCoord, toAirport.Coordinates), 180f, 0f);
	}

	private void FlyToAirport(Airport airport)
	{
		fromAirport = toAirport;
		toAirport = airport;
		progress = 0f;
		progressSpeed = speed / airportManager.HaversineDistance(fromAirport.Coordinates, toAirport.Coordinates);
	}

	private void FlyToNextAirport()
	{
		currentPathIndex = (currentPathIndex + 1) % AirportPath.Count;
		FlyToAirport(AirportPath[currentPathIndex]);
	}

	private void FlyToRandomAirport()
	{
		FlyToAirport(airportManager.GetRandomAirport(excludedAirport: fromAirport));
	}

	private void DropOffPassengers()
	{
		if (toAirport == null)
		{
			return;
		}

		uiManager.CurrentTrips += _passengerContainer.RemovePassengersOfType(toAirport.Type);
	}

	public void AddAirportToPath(Airport airport)
	{
		if (AirportPath.Contains(airport))
		{
			Debug.Log("NONE");
			return;
		}

		AirportPath.Add(airport);
		airport.ConnectedAirplanes.Add(this);

		if (AirportPath.Count == 1)
		{
			fromAirport = AirportPath[0];
			toAirport = AirportPath[0];
			InterpolateBetweenAirports(0f);
			IsVisible = true;
		}

		if (AirportPath.Count > 1)
		{
			for (int i = FlightPathObjects.Count - 1; i >= 0; --i)
			{
				Destroy(FlightPathObjects[i]);
			}
			FlightPathObjects.Clear();

			for (int i = 0; i < AirportPath.Count; i++)
			{
				FlightPathObjects.Add(airportManager.SpawnNewFlightPath(AirportPath[i], AirportPath[(i + 1) % AirportPath.Count], Color));
			}
		}
	}

	public void ClearAiportPath()
	{
		for (int i = FlightPathObjects.Count - 1; i >= 0; --i)
		{
			Destroy(FlightPathObjects[i]);
		}
		FlightPathObjects.Clear();

		for (int i = AirportPath.Count - 1; i >= 0; --i)
		{
			AirportPath[i].ConnectedAirplanes.Remove(this);
		}
		AirportPath.Clear();

		PassengerContainer.RemoveAllPassengers();

		IsVisible = false;
	}

	public bool PickUpPassenger(ShapeType shapeType)
	{
		if (AirportPath.Where(a => a.Type == shapeType).Any())
		{
			PassengerContainer.AddPassenger(shapeType);
			return true;
		}

		return false;
	}
}
