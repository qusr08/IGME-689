using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using UnityEngine;

public class Airplane : MonoBehaviour
{
	[SerializeField] private AirportManager airportManager;
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private MeshRenderer[] meshRenderers;
	[SerializeField, Min(0f)] private int altitude;
	[SerializeField] private float speed;
	[Space]
	[SerializeField] private Airport toAirport;
	[SerializeField] private Airport fromAirport;

	private float progress;
	private float progressSpeed;
	private Material airplaneMaterial;

	public ArcGISPoint Coordinates { get => locationComponent.Position; private set => locationComponent.Position = value; }
	public ArcGISRotation Rotation { get => locationComponent.Rotation; private set => locationComponent.Rotation = value; }

	public Color Color { get => airplaneMaterial.color; set => airplaneMaterial.color = value; }

	private void Awake()
	{
		airportManager = FindFirstObjectByType<AirportManager>();
		locationComponent = GetComponent<ArcGISLocationComponent>();
		meshRenderers = GetComponentsInChildren<MeshRenderer>();

		airplaneMaterial = new Material(meshRenderers[0].material);
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.material = airplaneMaterial;
		}

		progress = 1f;

		Color = new Color(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f));
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

		Coordinates = new ArcGISPoint(interCoord.X, interCoord.Y, interAltitude, Coordinates.SpatialReference);
		Rotation = new ArcGISRotation(airportManager.Bearing(interCoord, toAirport.Coordinates), 180f, 0f);
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
