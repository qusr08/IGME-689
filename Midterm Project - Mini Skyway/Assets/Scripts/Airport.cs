using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Airport : MonoBehaviour
{
	[SerializeField] private UIManager uiManager;
	[SerializeField] private ArcGISLocationComponent locationComponent;
	[SerializeField] private Shape mainShape;
	[SerializeField] private PassengerContainer _passengerContainer;
	[SerializeField, Range(0f, 10f)] private float passengerLoadDelay;
	[SerializeField, Range(0f, 10f)] private float gameOverTime;
	[Space]
	[SerializeField] private AirportData _data;
	[SerializeField] private AirportIndicator _indicator;
	[SerializeField] public List<Airplane> DockedAirplanes;
	[SerializeField] public List<Airplane> ConnectedAirplanes;

	public AirportIndicator Indicator { get => _indicator; set => _indicator = value; }
	public ShapeType Type { get => mainShape.Type; set => mainShape.Type = value; }
	public bool IsRendererVisible => mainShape.IsRendererVisible;
	public PassengerContainer PassengerContainer { get => _passengerContainer; private set => _passengerContainer = value; }
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

	private float passengerLoadTimer;
	private float gameOverTimer;
	private float passengerSpawnTimer;
	private float passengerSpawnDelay;

	private void Awake()
	{
		DockedAirplanes = new List<Airplane>();
		ConnectedAirplanes = new List<Airplane>();
		uiManager = FindFirstObjectByType<UIManager>();

		passengerSpawnDelay = 0f;
		//passengerSpawnTimer = Random.Range(25f, 20f) / Mathf.Sqrt(uiManager.CurrentTime + 1f);
		passengerSpawnTimer = Random.Range(9f, 10f) * Mathf.Exp((-uiManager.CurrentTime + 1f) / 100);
	}

	private void Update()
	{
		if (uiManager.State != UIState.GAME)
		{
			return;
		}

		if (DockedAirplanes.Count > 0)
		{
			passengerLoadTimer += Time.deltaTime;
			if (passengerLoadTimer >= passengerLoadDelay)
			{
				for (int i = DockedAirplanes.Count - 1; i >= 0; i--)
				{
					if (PassengerContainer.CurrentPassengers == 0)
					{
						break;
					}

					for (int j = 0; j < PassengerContainer.CurrentPassengers; j++)
					{
						ShapeType shapeType = PassengerContainer.PassengerTypeList[j];

						if (DockedAirplanes[i].PickUpPassenger(shapeType))
						{
							PassengerContainer.RemovePassengerAtIndex(j);
							break;
						}
						else if (j == PassengerContainer.CurrentPassengers - 1)
						{
							DockedAirplanes[i].UndockFlag = true;
						}
					}
				}

				passengerLoadTimer -= passengerLoadDelay;
			}
		}
		else
		{
			passengerLoadTimer = 0f;
		}

		if (PassengerContainer.IsAtCapacity)
		{
			gameOverTimer += Time.deltaTime;
			if (gameOverTimer >= gameOverTime)
			{
				uiManager.State = UIState.GAME_OVER;
			}
		}
		else
		{
			gameOverTimer = 0f;
		}

		passengerSpawnDelay += Time.deltaTime;
		if (passengerSpawnDelay >= passengerSpawnTimer)
		{
			passengerSpawnDelay -= passengerSpawnTimer;
			passengerSpawnTimer = Random.Range(9f, 10f) * Mathf.Exp((-uiManager.CurrentTime + 1f) / 100);
			PassengerContainer.AddRandomPassenger(excludedType: Type);
		}
	}
}
