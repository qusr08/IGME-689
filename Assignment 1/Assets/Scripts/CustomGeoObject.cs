using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.Map;
using Esri.GameEngine.View;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;

public class CustomGeoObject : MonoBehaviour
{
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;

	public void PrintCoordinates ( )
	{
		// Convert this object's unity coordinates to universe coordinates
		// Then convert them to geographic coordinates
		double3 universeCoordinates = arcGISMapComponent.GetComponent<HPRoot>( ).InverseTransformPoint(new double3(transform.position));
		ArcGISPoint geoCoordinates = arcGISMapComponent.View.WorldToGeographic(universeCoordinates);
		Debug.Log($"{name} is at ({geoCoordinates.X}, {geoCoordinates.Y})");
	}
}
