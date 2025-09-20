using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public enum StateDataIndex
{
	GID, AREALAND, DIVISION, INTPTLAT, NAME, OBJECTID, AREAWATER, INTPTLON, OID, FUNCSTAT, CENTLON, STUSAB, STATE, STATENS, CENTLAT, BASENAME, MTFCC, REGION, LSADC, GEOID, ST_ASGEOJSON, GEO_POINT_2D
}

public enum RainfallDataIndex
{
	STATION_ID, STATE_CODE, STATION_LIST_NO, NETWORK_DIV_NO, ELEMENT_UNITS, DATE, HOUR, RAINFALL, STATE
}

public class GenerateStateGeometry : MonoBehaviour
{
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;
	[Space]
	[SerializeField] private Material stateMaterial;
	[SerializeField] private float stateMinAltitude;
	[SerializeField] private float stateMeshScale;

	private const int STATE_CSV_DATA_LENGTH = 22;
	private const int RAINFALL_CSV_DATA_LENGTH = 9;
	private Dictionary<string, StateData> stateDataList;

	private void Awake ( )
	{
		stateDataList = new Dictionary<string, StateData>( );
	}

	private void Start ( )
	{
		LoadStateData( );
		GenerateStateMeshGroups( );
		SetupUI( );
	}

	private void LoadStateData ( )
	{
		// Load boundary data
		string[ ] csvBoundaryData = File.ReadAllText("Data/us-state-boundaries.csv").Split(";");
		int i = STATE_CSV_DATA_LENGTH;
		do
		{
			StateData stateData = new StateData( );
			stateData.Name = csvBoundaryData[i + (int) StateDataIndex.NAME];
			JSONNode coordinateNode = JSONNode.Parse(csvBoundaryData[i + (int) StateDataIndex.ST_ASGEOJSON].Replace("\"\"", "\"")[1..^1])["coordinates"];
			foreach (JSONNode coordinateGroupNode in coordinateNode)
			{
				stateData.GeoData.Add(new List<Vector2>( ));

				JSONNode groupNode = coordinateGroupNode.Count == 1 ? coordinateGroupNode[0] : coordinateGroupNode;
				foreach (JSONNode coordinatePairNode in groupNode)
				{
					stateData.GeoData[^1].Add(new Vector2(coordinatePairNode[0], coordinatePairNode[1]));
				}
			}
			stateDataList.Add(stateData.Name, stateData);

			i += STATE_CSV_DATA_LENGTH;
		}
		while (i < csvBoundaryData.Length);

		// Load rainfall data
		string[ ] csvRainfallData = File.ReadAllText("Data/us-state-daily-rainfall.csv").Split(new char[ ] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		i = RAINFALL_CSV_DATA_LENGTH;
		do
		{
			// Check to make sure the state name is in the list
			string stateName = csvRainfallData[i + (int) RainfallDataIndex.STATE];
			if (stateDataList.TryGetValue(stateName, out StateData stateData))
			{
				// Get the number of days since the min value of the 
				string[ ] rainfallDateString = csvRainfallData[i + (int) RainfallDataIndex.DATE].Split("-");
				DateTime rainfallDate = new DateTime(int.Parse(rainfallDateString[0]), int.Parse(rainfallDateString[1]), int.Parse(rainfallDateString[2]));
				int rainfallDateDays = (int) (rainfallDate - DateTime.MinValue).TotalDays;
				int rainfallAmount = int.Parse(csvRainfallData[i + (int) RainfallDataIndex.RAINFALL]);

				if (rainfallDateDays < stateData.EarliestDaySinceMinValue)
				{
					stateData.EarliestDaySinceMinValue = rainfallDateDays;
				}

				stateData.RainfallByDate[rainfallDateDays] = rainfallAmount;
			}

			i += RAINFALL_CSV_DATA_LENGTH;
		} while (i < csvRainfallData.Length);
	}

	private void GenerateStateMeshGroups ( )
	{
		foreach (string stateName in stateDataList.Keys)
		{
			GenerateStateMeshGroup(stateDataList[stateName]);
		}

		UpdateStateMeshes( );
	}

	public GameObject GenerateStateMeshGroup (StateData stateData)
	{
		GameObject stateMeshGroup = new GameObject($"{stateData.Name} Mesh Group");
		stateMeshGroup.transform.SetParent(transform);

		List<Vector2> meshPoints = new List<Vector2>( );
		Material meshMaterial = new Material(stateMaterial);
		meshMaterial.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

		for (int i = 0; i < stateData.GeoData.Count; i++)
		{
			meshPoints.Clear( );

			// Add mesh points based on state shape
			for (int j = 0; j < stateData.GeoData[i].Count; j++)
			{
				// Calculate the unity coordinate of the geographic coordinate
				Vector2 geographicCoordinate = stateData.GeoData[i][j];
				ArcGISPoint geographicPoint = new ArcGISPoint(geographicCoordinate.x, geographicCoordinate.y, stateMinAltitude, ArcGISSpatialReference.WGS84( ));
				double3 universeCoordinate = arcGISMapComponent.View.GeographicToWorld(geographicPoint);
				double3 unityCoordinate = arcGISMapComponent.GetComponent<HPRoot>( ).TransformPoint(universeCoordinate);

				// Scale the mesh points down so the polygon collider component can generate a new mesh
				//testvertices.Add(new Vector3((float) unityCoordinate[0], (float) unityCoordinate[1], (float) unityCoordinate[2]));
				meshPoints.Add(new Vector2((float) unityCoordinate[0] / stateMeshScale, (float) unityCoordinate[2] / stateMeshScale));
			}

			// Create a gameobject to display the mesh
			GameObject meshObject = new GameObject($"Mesh {i}");

			// Create polygon collider component to generate a triangulated mesh
			PolygonCollider2D collider2D = meshObject.AddComponent<PolygonCollider2D>( );
			collider2D.SetPath(0, meshPoints.ToArray( ));
			Mesh mesh = collider2D.CreateMesh(true, true);
			collider2D.enabled = false;
			meshObject.AddComponent<MeshFilter>( ).mesh = mesh;

			// Set the material of the state
			meshObject.AddComponent<MeshRenderer>( ).material = meshMaterial;

			// Set the position of the mesh so it aligns with the map
			meshObject.transform.SetPositionAndRotation(new Vector3(0f, stateMinAltitude, 0f), Quaternion.Euler(90f, 0f, 0f));
			meshObject.transform.localScale = new Vector3(stateMeshScale, stateMeshScale, 1f);
			meshObject.transform.SetParent(stateMeshGroup.transform);
		}

		return stateMeshGroup;
	}

	private void UpdateStateMeshes ( )
	{

	}

	private void SetupUI ( )
	{

	}
}

public class StateData
{
	public string Name { get; set; }
	public List<List<Vector2>> GeoData { get; private set; }
	public Dictionary<int, int> RainfallByDate { get; private set; }
	public int EarliestDaySinceMinValue { get; set; }

	public StateData ( )
	{
		Name = "NA";
		GeoData = new List<List<Vector2>>( );
		RainfallByDate = new Dictionary<int, int>( );
		EarliestDaySinceMinValue = int.MaxValue;
	}

	public int GetRainfallAtDate (int daysSinceMinValue)
	{
		while (!RainfallByDate.ContainsKey(daysSinceMinValue) && daysSinceMinValue > EarliestDaySinceMinValue)
		{
			daysSinceMinValue--;
		}
		return RainfallByDate[daysSinceMinValue + 1];
	}
}