using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private int minAltitude;
	[SerializeField] private int maxAltitude;
	[SerializeField, Range(0f, 1f)] private float smoothing;

	private Airplane targetAirplane;

	private int altitude;
	private Vector2 coordPoint;
	private Vector2 toCoordPoint;
	private Vector2 velocity;

	public ArcGISPoint Coordinates { get => locationComponent.Position; private set => locationComponent.Position = value; }

	private void Awake()
	{
		locationComponent = GetComponent<ArcGISLocationComponent>();
		altitude = (maxAltitude + minAltitude) / 2;
	}

	private void Update()
	{
		if (targetAirplane == null)
		{
			targetAirplane = FindFirstObjectByType<Airplane>();
			toCoordPoint = new Vector2((float)targetAirplane.Coordinates.X, (float)targetAirplane.Coordinates.Y);
			coordPoint = toCoordPoint;
		}
		else
		{
			toCoordPoint = new Vector2((float) targetAirplane.Coordinates.X, (float) targetAirplane.Coordinates.Y);
			coordPoint = Vector2.SmoothDamp(coordPoint, toCoordPoint, ref velocity, smoothing);
			Coordinates = new ArcGISPoint(coordPoint.x, coordPoint.y, altitude, Coordinates.SpatialReference);
		}
	}
}
