using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;

public class Airplane : MonoBehaviour
{
	[SerializeField] private AirportManager airportManager;
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField, Min(0f)] private int altitude;
	[Space]
	[SerializeField] private Airport toAirport;
	[SerializeField] private Airport fromAirport;
	[SerializeField] private float speed;

	private float progress;
	private float progressSpeed;
	private float fromBearing;
	private float toBearing;

	public ArcGISPoint Coordinates => locationComponent.Position;

	private void OnValidate()
	{
		airportManager = FindFirstObjectByType<AirportManager>();
		locationComponent = GetComponent<ArcGISLocationComponent>();

		ArcGISPoint location = locationComponent.Position;
		if (location != null)
		{
			locationComponent.Position = new ArcGISPoint(location.X, location.Y, altitude, location.SpatialReference);
			locationComponent.Rotation = new ArcGISRotation(0f, 180f, 0f);
		}
	}

	private void Awake()
	{
		OnValidate();
	}

	private void Start()
	{
		progress = 1f;
	}

	private void Update()
	{
		ProgressFlight(Time.deltaTime);
	}

	private void ProgressFlight(float deltaTime)
	{
		progress += Time.deltaTime * progressSpeed;

		if (progress >= 1f)
		{
			fromAirport = toAirport ? toAirport : airportManager.GetRandomAirport();
			FlyToRandomAirport();
			progress = 0f;
			progressSpeed = speed / airportManager.HaversineDistance(fromAirport.Coordinates, toAirport.Coordinates);
		}

		InterpolateBetweenAirports(progress);
	}

	private void InterpolateBetweenAirports(float t)
	{
		ArcGISPoint interCoord = airportManager.IntermediateCoordinate(fromAirport.Coordinates, toAirport.Coordinates, t);
		//float interAltitude = -4 * t * (t - 1) * altitude;
		float interAltitude = (-1 * Mathf.Pow(2 * t - 1, 6) + 1) * altitude;
		locationComponent.Position = new ArcGISPoint(interCoord.X, interCoord.Y, interAltitude, locationComponent.Position.SpatialReference);
		locationComponent.Rotation = new ArcGISRotation(airportManager.Bearing(interCoord, toAirport.Coordinates), 180f, 0f);
	}

	private void FlyToAirport(Airport airport)
	{
		toAirport = airport;
	}

	private void FlyToRandomAirport()
	{
		FlyToAirport(airportManager.GetRandomAirport(excludedAirport: fromAirport));
	}
}
