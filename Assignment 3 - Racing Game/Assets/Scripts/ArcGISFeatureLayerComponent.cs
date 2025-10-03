// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Splines;

public class ArcGISFeatureLayerComponent : MonoBehaviour
{
    [System.Serializable]
    public struct QueryLink
    {
        public string Link;
        public string[] RequestHeaders;
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
        public List<string> PropertyNames = new List<string>();
        public List<string> Data = new List<string>();
    }

    [System.Serializable]
    public class FeatureQueryData
    {
        public GeometryData Geometry = new GeometryData();
        public PropertyData Properties = new PropertyData();
    }

    private List<FeatureQueryData> Features = new List<FeatureQueryData>();
    private FeatureData featureInfo;
    [SerializeField] private GameObject featurePrefab;
    private JToken[] jFeatures;

    public List<GameObject> FeatureItems = new List<GameObject>();
    public QueryLink WebLink;
    [SerializeField] private List<int> exclusiveFeatureIDs;
    [SerializeField] private SplineContainer splineContainer;
    private ArcGISMapComponent mapComponent;

    [Space]
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private Transform checkpointParent;
    [SerializeField] private float maxCheckpointDistance;
    private List<Checkpoint> checkpointList;

    private void Start()
    {
        checkpointList = new List<Checkpoint>();
        mapComponent = FindFirstObjectByType<ArcGISMapComponent>();
        StartCoroutine(nameof(GetFeatures));
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            foreach (string header in WebLink.RequestHeaders)
            {
                if (!link.ToLower().Contains(header))
                {
                    link += header;
                }
            }

            WebLink.Link = link;
        }
    }

    public IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(WebLink.Link);
        yield return Request.SendWebRequest();

        if (Request.result == UnityWebRequest.Result.Success)
        {
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
        }
        else
        {
            Debug.Log(Request.result);
        }
    }

    private void CreateGameObjectsFromResponse(string response)
    {
        // Deserialize the JSON response from the query.
        JObject jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();
        CreateFeatures();
    }

    private void CreateFeatures()
    {
        foreach (JToken feature in jFeatures)
        {
            // Only add features with the specified IDs
            if (!exclusiveFeatureIDs.Contains((int)feature.SelectToken("id")))
            {
                continue;
            }

            // Get coordinates in the Feature Service
            JToken[] coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();

            Vector2 lastCheckpointCoordinate = Vector2.zero;
            foreach (JToken coordinate in coordinates)
            {
                // Get the current coordinate of the next checkpoint
                bool checkpointsTooFar = true;
                coordinate.ToArray();
                Vector2 checkpointCoordinate = new Vector2((float)Convert.ToDouble(coordinate[0]), (float)Convert.ToDouble(coordinate[1]));

                // If there are less than 2 checkpoints currently placed, then do not check to see if the checkpoints are too far
                if (splineContainer.Splines[0].Count <= 1)
                {
                    CreateCheckpoint(checkpointCoordinate);
                    lastCheckpointCoordinate = checkpointCoordinate;

                    continue;
                }

                // Keep creating intermediate checkpoints if the distance between them are too far
                // This will make sure the track sits on the surface of the terrain better
                int counter = 0;
                while (checkpointsTooFar && counter++ < 20)
                {
                    // Get the direction of the new checkpoint
                    Vector2 checkpointDirection = lastCheckpointCoordinate - checkpointCoordinate;

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

                if (counter == 20)
                {
                    Debug.Log("Counter HIT");
                }
            }
        }
    }

    private void CreateCheckpoint(Vector2 position)
    {
        Checkpoint checkpoint = Instantiate(checkpointPrefab, checkpointParent).GetComponent<Checkpoint>();
        checkpoint.Initialize(splineContainer, mapComponent, new ArcGISPoint(position.x, position.y, new ArcGISSpatialReference(4326)));
        checkpointList.Add(checkpoint);
    }
}
