using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;

public class Airplane : MonoBehaviour
{
    [SerializeField] private AirportSpawner airportSpawner;
    [SerializeField] private ArcGISLocationComponent locationComponent;
    [SerializeField, Min(0f)] private int altitude;
    [Space]
    [SerializeField] private Airport targetAirport;
    [SerializeField] private float progress;

    private void OnValidate()
    {
        airportSpawner = FindFirstObjectByType<AirportSpawner>();
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
}
