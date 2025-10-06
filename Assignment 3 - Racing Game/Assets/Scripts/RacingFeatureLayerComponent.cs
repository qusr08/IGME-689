// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Splines;

public class RacingFeatureLayerComponent : MonoBehaviour
{
	[System.Serializable]
	public struct QueryLink
	{
		public string Link;
		public string[ ] RequestHeaders;
	}

	[System.Serializable]
	public class GeometryData
	{
		public double Latitude;
		public double Longitude;
	}

	[System.Serializable]
	public class PropertyData
	{
		public List<string> PropertyNames = new List<string>( );
		public List<string> Data = new List<string>( );
	}

	[System.Serializable]
	public class FeatureQueryData
	{
		public GeometryData Geometry = new GeometryData( );
		public PropertyData Properties = new PropertyData( );
	}

	private List<FeatureQueryData> Features = new List<FeatureQueryData>( );
	private FeatureData featureInfo;
	[SerializeField] private GameObject featurePrefab;
	private JToken[ ] jFeatures;

	public List<GameObject> FeatureItems = new List<GameObject>( );
	public QueryLink WebLink;
	[SerializeField] private List<int> exclusiveFeatureIDs;
	public SplineContainer SplineContainer;
	private ArcGISMapComponent mapComponent;

	[Space]
	[SerializeField] private GameObject checkpointPrefab;
	[SerializeField] private Transform checkpointParent;
	[SerializeField] private float maxCheckpointDistance;
	[SerializeField] private float minCheckpointDistance;
	private List<Checkpoint> checkpointList;
	private List<Vector2> checkpointPositions;

	public int CheckpointCount => checkpointList.Count;

	private void Start ( )
	{
		checkpointList = new List<Checkpoint>( );
		checkpointPositions = new List<Vector2>( );
		mapComponent = FindFirstObjectByType<ArcGISMapComponent>( );
		StartCoroutine(nameof(GetFeatures));
	}

	public void CreateLink (string link)
	{
		if (link != null)
		{
			foreach (string header in WebLink.RequestHeaders)
			{
				if (!link.ToLower( ).Contains(header))
				{
					link += header;
				}
			}

			WebLink.Link = link;
		}
	}

	public IEnumerator GetFeatures ( )
	{
		// To learn more about the Feature Layer rest API and all the things that are possible checkout
		// https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

		UnityWebRequest Request = UnityWebRequest.Get(WebLink.Link);
		yield return Request.SendWebRequest( );

		if (Request.result == UnityWebRequest.Result.Success)
		{
			CreateGameObjectsFromResponse(Request.downloadHandler.text);
		}
		else
		{
			Debug.Log(Request.result);
		}
	}

	private void CreateGameObjectsFromResponse (string response)
	{
		// Deserialize the JSON response from the query.
		JObject jObject = JObject.Parse(response);
		jFeatures = jObject.SelectToken("features").ToArray( );
		CreateFeatures( );
	}

	private void CreateFeatures ( )
	{
		foreach (JToken feature in jFeatures)
		{
			// Only add features with the specified IDs
			if (!exclusiveFeatureIDs.Contains((int) feature.SelectToken("id")))
			{
				continue;
			}

			// Get coordinates in the Feature Service
			JToken[ ] coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray( );

			Vector2 lastCheckpointCoordinate = Vector2.zero;
			foreach (JToken coordinate in coordinates)
			{
				// Get the current coordinate of the next checkpoint
				bool checkpointsTooFar = true;
				coordinate.ToArray( );
				Vector2 checkpointCoordinate = new Vector2((float) Convert.ToDouble(coordinate[0]), (float) Convert.ToDouble(coordinate[1]));

				// If there are less than 2 checkpoints currently placed, then do not check to see if the checkpoints are too far
				if (SplineContainer.Splines[0].Count < 2)
				{
					CreateCheckpoint(checkpointCoordinate);
					lastCheckpointCoordinate = checkpointCoordinate;

					continue;
				}

				// Keep creating intermediate checkpoints if the distance between them are too far
				// This will make sure the track sits on the surface of the terrain better
				while (checkpointsTooFar)
				{
					// Get the direction of the new checkpoint
					Vector2 checkpointDirection = checkpointCoordinate - lastCheckpointCoordinate;

					// If the distance to the new checkpoint is larger than the max checkpoint distance, then increase the checkpoint position by the max distance
					// This will gradually place checkpoints along a line until the current checkpoint coordinate is close enough to the last checkpoint coordinate
					// This ensures that no checkpoints are too far apart
					if (checkpointDirection.magnitude <= maxCheckpointDistance)
					{
						checkpointsTooFar = false;
						lastCheckpointCoordinate = checkpointCoordinate;
					}
					else
					{
						lastCheckpointCoordinate += checkpointDirection.normalized * maxCheckpointDistance;
					}

					CreateCheckpoint(lastCheckpointCoordinate);
				}
			}
		}

		// Make sure the road is smooth
		SplineContainer.Splines[0].SetTangentMode(TangentMode.AutoSmooth);
	}

	private void CreateCheckpoint (Vector2 position)
	{
		// Make sure two checkpoints do not spawn on top of each other
		foreach (Vector2 checkpointPosition in checkpointPositions)
		{
			if ((checkpointPosition - position).magnitude < minCheckpointDistance)
			{
				return;
			}
		}

		Checkpoint checkpoint = Instantiate(checkpointPrefab, checkpointParent).GetComponent<Checkpoint>( );
		checkpoint.Initialize(this, SplineContainer, mapComponent, new ArcGISPoint(position.x, position.y, new ArcGISSpatialReference(4326)));
		checkpointList.Add(checkpoint);
		checkpointPositions.Add(position);
	}

	public Vector3 GetSplineKnotMidpoint (int index1, int index2)
	{
		int knotCount = SplineContainer.Splines[0].Count;

		if (knotCount < 2)
		{
			return Vector3.zero;
		}

		// The last position in the spline is the same as the start for the dataset being used
		// This gets around that and skips the last index
		index1 %= knotCount - 1;
		index2 %= knotCount - 1;

		Vector3 position1 = SplineContainer.Splines[0][index1].Position;
		Vector3 position2 = SplineContainer.Splines[0][index2].Position;
		return (position1 + position2) * 0.5f;
	}

	public Quaternion GetSplineKnotRotation (int index1, int index2)
	{
		int knotCount = SplineContainer.Splines[0].Count;

		if (knotCount < 2)
		{
			return Quaternion.identity;
		}

		// The last position in the spline is the same as the start for the dataset being used
		// This gets around that and skips the last index
		index1 %= knotCount - 1;
		index2 %= knotCount - 1;

		Vector3 position1 = SplineContainer.Splines[0][index1].Position;
		Vector3 position2 = SplineContainer.Splines[0][index2].Position;
		Vector3 direction = position1 - position2;
		float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
		return Quaternion.Euler(0f, angle, 0f);
	}
}
