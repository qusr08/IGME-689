using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public enum StateDataIndex
{
	GID, AREALAND, DIVISION, INTPTLAT, NAME, OBJECTID, AREAWATER, INTPTLON, OID, FUNCSTAT, CENTLON, STUSAB, STATE, STATENS, CENTLAT, BASENAME, MTFCC, REGION, LSADC, GEOID, ST_ASGEOJSON, GEO_POINT_2D
}

public enum TemperatureDataIndex
{
	INDEX, FIPS, YEAR, TEMP, TEMPC
}

public class StateGeometryGenerator : MonoBehaviour
{
	[SerializeField] private ArcGISMapComponent arcGISMapComponent;
	[SerializeField] private TextMeshProUGUI yearText;
	[SerializeField] private Slider yearSlider;
	[SerializeField] private TextMeshProUGUI medianTemperatureText;
	[SerializeField] private Slider medianTemperatureSlider;
	[SerializeField] private TextMeshProUGUI temperatureRangeText;
	[SerializeField] private Slider temperatureRangeSlider;
	[Space]
	[SerializeField] private Material stateMaterial;
	[SerializeField] private Gradient temperatureGradient;
	[SerializeField] private float stateMinAltitude;
	[SerializeField] private float stateMeshScale;

	private const int STATE_CSV_DATA_LENGTH = 22;
	private const int TEMPERATURE_CSV_DATA_LENGTH = 5;

	public Dictionary<int, StateData> StateDataList { get; private set; }
	public int MaxYear { get; private set; }
	public int MinYear { get; private set; }
	public float MinTemperature { get; private set; }
	public float MaxTemperature { get; private set; }

	private void Awake ( )
	{
		StateDataList = new Dictionary<int, StateData>( );
		MinYear = int.MaxValue;
		MaxYear = int.MinValue;
		MinTemperature = float.MaxValue;
		MaxTemperature = float.MinValue;
	}

	private void Start ( )
	{
		LoadStateBoundaryData( );
		LoadStateTemperatureData( );
		RemoveUnusedStateData( );

		yearSlider.minValue = MinYear;
		yearSlider.maxValue = MaxYear;
		yearSlider.onValueChanged.AddListener((v) => { UpdateStateMeshGroups( ); });

		medianTemperatureSlider.minValue = MinTemperature;
		medianTemperatureSlider.maxValue = MaxTemperature;
		medianTemperatureSlider.onValueChanged.AddListener((v) => { UpdateStateMeshGroups( ); });

		temperatureRangeSlider.minValue = 1;
		temperatureRangeSlider.maxValue = (MaxTemperature - MinTemperature) / 2f;
		temperatureRangeSlider.onValueChanged.AddListener((v) => { UpdateStateMeshGroups( ); });

		foreach (int stateFIPS in StateDataList.Keys)
		{
			GenerateStateMeshGroup(stateFIPS);
		}

		yearSlider.value = 2000;
		medianTemperatureSlider.value = 60f;
		temperatureRangeSlider.value = 2f;
	}

	private void LoadStateBoundaryData ( )
	{
		string[ ] csvBoundaryData = File.ReadAllText("Data/us-state-boundaries.csv").Split(";");

		int i = STATE_CSV_DATA_LENGTH;
		do
		{
			StateData stateData = new StateData( );
			stateData.Name = csvBoundaryData[i + (int) StateDataIndex.NAME].Trim( );
			stateData.FIPS = int.Parse(csvBoundaryData[i + (int) StateDataIndex.STATE]);

			// Load all geography data using JSON nodes
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

			StateDataList.Add(stateData.FIPS, stateData);
			i += STATE_CSV_DATA_LENGTH;
		}
		while (i < csvBoundaryData.Length);
	}

	private void LoadStateTemperatureData ( )
	{
		string[ ] csvTemperatureData = File.ReadAllText("Data/us-state-temperature.csv").Split(new char[ ] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);

		int i = TEMPERATURE_CSV_DATA_LENGTH;
		do
		{
			// Check to make sure the state FIPS is in the list
			int stateFIPS = int.Parse(csvTemperatureData[i + (int) TemperatureDataIndex.FIPS]);
			if (StateDataList.TryGetValue(stateFIPS, out StateData stateData))
			{
				// Add the data point to the state
				int year = int.Parse(csvTemperatureData[i + (int) TemperatureDataIndex.YEAR]);
				float temperature = float.Parse(csvTemperatureData[i + (int) TemperatureDataIndex.TEMP]);
				stateData.TemperatureByYear[year] = temperature;

				// Check to see if a new earliest/lowest/highest has been found
				if (year < stateData.EarliestYear)
				{
					stateData.EarliestYear = year;
				}

				if (year < MinYear)
				{
					MinYear = year;
				}

				if (year > MaxYear)
				{
					MaxYear = year;
				}

				if (temperature < MinTemperature)
				{
					MinTemperature = temperature;
				}

				if (temperature > MaxTemperature)
				{
					MaxTemperature = temperature;
				}
			}

			i += TEMPERATURE_CSV_DATA_LENGTH;
		} while (i < csvTemperatureData.Length);
	}

	private void RemoveUnusedStateData ( )
	{
		List<int> unusedStates = new List<int>( );

		// If no data points have been added, then the state is unused
		foreach (KeyValuePair<int, StateData> stateDataPair in StateDataList)
		{
			if (stateDataPair.Value.TemperatureByYear.Count == 0)
			{
				unusedStates.Add(stateDataPair.Key);
			}
		}

		foreach (int stateFIPS in unusedStates)
		{
			StateDataList.Remove(stateFIPS);
		}
	}

	private void GenerateStateMeshGroup (int stateFIPS)
	{
		StateData stateData = StateDataList[stateFIPS];

		// Create a new mesh group container object
		GameObject stateMeshGroup = new GameObject($"{stateData.Name} Mesh Group");
		stateMeshGroup.transform.SetParent(transform);

		List<Vector2> meshPoints = new List<Vector2>( );
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
				meshPoints.Add(new Vector2((float) unityCoordinate[0] / stateMeshScale, (float) unityCoordinate[2] / stateMeshScale));
			}

			// Create a gameobject to display the mesh
			GameObject meshObject = new GameObject($"Mesh {i}");

			// Create polygon collider component to generate a triangulated mesh
			PolygonCollider2D collider = meshObject.AddComponent<PolygonCollider2D>( );
			collider.SetPath(0, meshPoints.ToArray( ));
			Mesh mesh = collider.CreateMesh(true, true);
			collider.enabled = false;
			meshObject.AddComponent<MeshFilter>( ).mesh = mesh;

			// Set the material of the mesh renderer
			MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>( );
			meshRenderer.material = new Material(stateMaterial);
			stateData.MeshRenderers.Add(meshRenderer);

			// Set the position of the mesh so it aligns with the map
			meshObject.transform.SetPositionAndRotation(new Vector3(0f, stateMinAltitude, 0f), Quaternion.Euler(90f, 0f, 0f));
			meshObject.transform.localScale = new Vector3(stateMeshScale, stateMeshScale, 1f);
			meshObject.transform.SetParent(stateMeshGroup.transform);
		}
	}

	private void UpdateStateMeshGroups ( )
	{
		foreach (int stateFIPS in StateDataList.Keys)
		{
			UpdateStateMeshGroup(stateFIPS);
		}

		yearText.text = $"Year: {(int) yearSlider.value}";
		medianTemperatureText.text = $"Median Temperature: {medianTemperatureSlider.value:0.00}°F";
		temperatureRangeText.text = $"Temperature Range: ±{temperatureRangeSlider.value:0.00}°F";
	}

	public void UpdateStateMeshGroup (int stateFIPS)
	{
		StateData stateData = StateDataList[stateFIPS];
		float temperature = stateData.SafeGetTemperature((int) yearSlider.value) - medianTemperatureSlider.value;
		Color temperatureColor = temperatureGradient.Evaluate(Remap(temperature, -temperatureRangeSlider.value, temperatureRangeSlider.value, 0f, 1f));

		for (int i = 0; i < stateData.MeshRenderers.Count; i++)
		{
			stateData.MeshRenderers[i].material.color = temperatureColor;
		}
	}

	private float Remap (float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
}