using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AirportManager : MonoBehaviour
{
	public const int EARTH_RADIUS_METERS = 6378100;

	[SerializeField] private Transform airportContainer;
	[SerializeField] private Transform airplaneContainer;
	[SerializeField] private GameObject airportPrefab;
	[SerializeField] private GameObject airplanePrefab;

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

		airportDataLoader = new AirportDataLoader();
		airportDataLoader.LoadDataFromCSVFile();

		for (int i = 0; i < airportDataLoader.AirportDataCount; i++)
		{
			availableDataIndices.Add(i);
		}
	}

	private void Start()
	{
		for (int i = 0; i < 50; i++)
		{
			SpawnNewAirport();
		}

		for (int i = 0; i < 200; i++)
		{
			SpawnNewAirplane();
		}
	}

	private void SpawnNewAirport()
	{
		int dataIndex = availableDataIndices[Random.Range(0, availableDataIndices.Count)];
		availableDataIndices.Remove(dataIndex);
		usedDataIndices.Add(dataIndex);

		Airport airport = Instantiate(airportPrefab, airportContainer).GetComponent<Airport>();
		airport.Data = airportDataLoader.AirportDataList[dataIndex];
		AirportList.Add(airport);
	}

	private void SpawnNewAirplane()
	{
		Airplane airplane = Instantiate(airplanePrefab, airplaneContainer).GetComponent<Airplane>();
		AirplaneList.Add(airplane);
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
