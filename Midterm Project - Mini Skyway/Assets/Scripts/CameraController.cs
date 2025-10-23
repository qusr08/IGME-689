using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private int altitude;

	// If mouse cursor is on left side of screen, rotate camera left
	// Right side of screen rotates right

	private Airplane targetAirplane;

	private void OnValidate()
	{
		locationComponent = GetComponent<ArcGISLocationComponent>();
	}

	private void Awake()
	{
		OnValidate();
	}

	private void Update()
	{
		if (targetAirplane == null)
		{
			targetAirplane = FindFirstObjectByType<Airplane>();
		}
		else
		{
			locationComponent.Position = new ArcGISPoint(targetAirplane.Coordinates.X, targetAirplane.Coordinates.Y, altitude, locationComponent.Position.SpatialReference);
		}
	}
}
