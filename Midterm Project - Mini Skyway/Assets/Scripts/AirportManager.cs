using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AirportManager : MonoBehaviour
{
	public const int EARTH_RADIUS_METERS = 6378100;

	[SerializeField] private UIManager uiManager;
	[SerializeField] private Transform airportContainer;
	[SerializeField] private Transform airplaneContainer;
	[SerializeField] private Transform flightPathContainer;
	[SerializeField] private Transform indicatorContainer;
	[SerializeField] private GameObject airportPrefab;
	[SerializeField] private GameObject airplanePrefab;
	[SerializeField] private GameObject flightPathPrefab;
	[SerializeField] private GameObject airportIndicatorPrefab;
	[SerializeField] private ArcGISMapComponent mapComponent;

	private List<int> availableDataIndices;
	private List<int> usedDataIndices;
	private AirportDataLoader airportDataLoader;

	public List<Airport> AirportList { get; private set; }
	public List<Airplane> AirplaneList { get; private set; }

	private void Awake()
	{
		AirportList = new List<Airport>();
		AirplaneList = new List<Airplane>();
		availableDataIndices = new List<int>();
		usedDataIndices = new List<int>();
		uiManager = FindFirstObjectByType<UIManager>();

		airportDataLoader = new AirportDataLoader();
		airportDataLoader.LoadDataFromCSVFile();

		for (int i = 0; i < airportDataLoader.AirportDataCount; i++)
		{
			availableDataIndices.Add(i);
		}
	}

	private void Update()
	{
		while (Mathf.FloorToInt((uiManager.CurrentTime / 60f) + 3.75f) > AirportList.Count)
		{
			SpawnNewAirport();
		}
	}

	public void StartGame()
	{
		for (int i = 0; i < AirportList.Count; i++)
		{
			Destroy(AirportList[i].gameObject);
		}
		AirportList.Clear();

		for (int i = 0; i < AirplaneList.Count; i++)
		{
			AirplaneList[i].ClearAiportPath();
			Destroy(AirplaneList[i].gameObject);
		}
		AirplaneList.Clear();

		for (int i = 0; i < 3; i++)
		{
			SpawnNewAirport();
		}

		for (int i = 0; i < Enum.GetValues(typeof(PlaneColor)).Length; i++)
		{
			Airplane airplane = SpawnNewAirplane();
		}
	}

	private void SpawnNewAirport()
	{
		int dataIndex = availableDataIndices[Random.Range(0, availableDataIndices.Count)];
		availableDataIndices.Remove(dataIndex);
		usedDataIndices.Add(dataIndex);

		Airport airport = Instantiate(airportPrefab, airportContainer).GetComponent<Airport>();
		airport.Data = airportDataLoader.AirportDataList[dataIndex];
		airport.Type = (ShapeType)(AirportList.Count % 3) + 1;
		AirportList.Add(airport);

		AirportIndicator indicator = Instantiate(airportIndicatorPrefab, indicatorContainer).GetComponent<AirportIndicator>();
		indicator.Target = airport;
	}

	private Airplane SpawnNewAirplane()
	{
		Airplane airplane = Instantiate(airplanePrefab, airplaneContainer).GetComponent<Airplane>();
		airplane.Color = (PlaneColor)AirplaneList.Count;
		AirplaneList.Add(airplane);
		//airplane.AirportPath.Add(GetRandomAirport(excludedAirport: airplane.AirportPath[0]));
		return airplane;
	}

	public GameObject SpawnNewFlightPath(Airport start, Airport end, PlaneColor planeColor)
	{
		// Spawn the line renderer object
		LineRenderer flightPath = Instantiate(flightPathPrefab, flightPathContainer).GetComponent<LineRenderer>();
		int flightPathSegmentDistance = 10000;

		// Calculate the distance (in meters) from a start geo coordinate to an end geo coordinate
		// Divide this up into segments to see how many points to add to the line renderer
		int pointCount = (int)HaversineDistance(start.Coordinates, end.Coordinates) / flightPathSegmentDistance;

		ArcGISPoint interCoord;
		float t;
		Vector3[] positions = new Vector3[pointCount + 1];
		for (int i = 0; i <= pointCount; i++)
		{
			// Get a value between 0 and 1
			t = (float)i / pointCount;

			// Get an intermediate coordinate between the start and end geo coordinates, then convert that geo position to a unity world position
			// The unity world position is then added to the position list
			interCoord = IntermediateCoordinate(start.Coordinates, end.Coordinates, t);
			interCoord = new ArcGISPoint(interCoord.X, interCoord.Y, flightPath.startWidth / 2f, interCoord.SpatialReference);
			positions[i] = mapComponent.View.GeographicToWorld(interCoord).ToVector3();
		}

		flightPath.positionCount = positions.Length;
		flightPath.SetPositions(positions);

		Material material = new Material(flightPath.material);
		Color.RGBToHSV(uiManager.PlaneColorDictionary[planeColor], out float h, out float s, out float v);
		material.color = Color.HSVToRGB(h, s, v * 0.5f);
		flightPath.material = material;

		return flightPath.gameObject;
	}

	public Airport GetRandomAirport(Airport excludedAirport = null)
	{
		List<Airport> availableAirports = AirportList.Where(a => a != excludedAirport).ToList();
		return availableAirports[Random.Range(0, availableAirports.Count)];
	}

	// https://www.movable-type.co.uk/scripts/latlong.html

	public float HaversineDistance(ArcGISPoint fromCoord, ArcGISPoint toCoord)
	{
		Vector2 fromCoordRad = new Vector2((float)fromCoord.X, (float)fromCoord.Y) * Mathf.Deg2Rad;
		Vector2 toCoordRad = new Vector2((float)toCoord.X, (float)toCoord.Y) * Mathf.Deg2Rad;

		float sdlat = Mathf.Sin((toCoordRad.y - fromCoordRad.y) / 2);
		float sdlon = Mathf.Sin((toCoordRad.x - fromCoordRad.x) / 2);
		float q = sdlat * sdlat + Mathf.Cos(fromCoordRad.y) * Mathf.Cos(toCoordRad.y) * sdlon * sdlon;
		return 2 * EARTH_RADIUS_METERS * Mathf.Asin(Mathf.Sqrt(q));
	}

	public float Bearing(ArcGISPoint fromCoord, ArcGISPoint toCoord)
	{
		Vector2 fromCoordRad = new Vector2((float)fromCoord.X, (float)fromCoord.Y) * Mathf.Deg2Rad;
		Vector2 toCoordRad = new Vector2((float)toCoord.X, (float)toCoord.Y) * Mathf.Deg2Rad;

		float x = Mathf.Sin(toCoordRad.x - fromCoordRad.x) * Mathf.Cos(toCoordRad.y);
		float y = Mathf.Cos(fromCoordRad.y) * Mathf.Sin(toCoordRad.y) - Mathf.Sin(fromCoordRad.y) * Mathf.Cos(toCoordRad.y) * Mathf.Cos(toCoordRad.x - fromCoordRad.x);

		float bearing = Mathf.Atan2(x, y) * Mathf.Rad2Deg + 90f;
		return (bearing + 360f) % 360f;
	}

	public ArcGISPoint IntermediateCoordinate(ArcGISPoint fromCoord, ArcGISPoint toCoord, float t)
	{
		Vector2 fromCoordRad = new Vector2((float)fromCoord.X, (float)fromCoord.Y) * Mathf.Deg2Rad;
		Vector2 toCoordRad = new Vector2((float)toCoord.X, (float)toCoord.Y) * Mathf.Deg2Rad;
		float angDist = HaversineDistance(fromCoord, toCoord) / EARTH_RADIUS_METERS;

		float a = Mathf.Sin((1 - t) * angDist) / Mathf.Sin(angDist);
		float b = Mathf.Sin(t * angDist) / Mathf.Sin(angDist);
		float x = a * Mathf.Cos(fromCoordRad.y) * Mathf.Cos(fromCoordRad.x) + b * Mathf.Cos(toCoordRad.y) * Mathf.Cos(toCoordRad.x);
		float y = a * Mathf.Cos(fromCoordRad.y) * Mathf.Sin(fromCoordRad.x) + b * Mathf.Cos(toCoordRad.y) * Mathf.Sin(toCoordRad.x);
		float z = a * Mathf.Sin(fromCoordRad.y) + b * Mathf.Sin(toCoordRad.y);

		float lat = Mathf.Atan2(z, Mathf.Sqrt(x * x + y * y)) * Mathf.Rad2Deg;
		float lon = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
		return new ArcGISPoint(lon, lat, 0f, fromCoord.SpatialReference);
	}
}
