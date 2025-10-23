using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AirportType
{
    CIRCLE, TRIANGLE, SQUARE
}

public class Airport : MonoBehaviour
{
    [SerializeField] private ArcGISLocationComponent locationComponent;
    [SerializeField] private AirportMeshDictionary airportMeshDictionary;
    [SerializeField, Min(1)] private int size;
    [Space]
    [SerializeField] private AirportType _type;
    [SerializeField] private AirportData _data;
    [SerializeField] private AirportIndicator _indicator;

    public AirportType Type
    {
        get => _type;
        set
        {
            _type = value;

            // Update active airport mesh
            int airportTypeCount = Enum.GetValues(typeof(AirportType)).Length;
            for (int i = 0; i < airportTypeCount; i++)
            {
                airportMeshDictionary[(AirportType)i].enabled = (_type == (AirportType)i);
            }
        }
    }

    public AirportIndicator Indicator { get => _indicator; set => _indicator = value; }

    public bool IsRendererVisible => airportMeshDictionary[Type].isVisible;

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

    private void Awake()
    {
        locationComponent = GetComponent<ArcGISLocationComponent>();
    }

    private void Start()
    {
        Type = (AirportType)Random.Range(0, 3);

        Coordinates = new ArcGISPoint(Coordinates.X, Coordinates.Y, 0f, Coordinates.SpatialReference);
        Rotation = new ArcGISRotation(0f, 90f, 0f);
    }

    private void OnMouseDown()
    {
        Debug.Log("CLICK");
    }
}
