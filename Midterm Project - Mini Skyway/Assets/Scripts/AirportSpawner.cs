using System.Collections.Generic;
using UnityEngine;

public class AirportSpawner : MonoBehaviour
{
    [SerializeField] private Transform aiportObjectContainer;
    [SerializeField] private GameObject airportPrefab;

    private List<Airport> airportList;
    private List<int> availableDataIndices;
    private List<int> usedDataIndices;

    private AirportDataLoader airportDataLoader;

    private void Awake()
    {
        airportList = new List<Airport>();
        availableDataIndices = new List<int>();
        usedDataIndices = new List<int>();

        airportDataLoader = new AirportDataLoader();
        airportDataLoader.LoadDataFromCSVFile();

        for (int i = 0; i < airportDataLoader.AirportDataCount; i++)
        {
            availableDataIndices.Add(i);
        }
    }

    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnNewRandomAirport();
        }
    }

    private void SpawnNewRandomAirport()
    {
        int dataIndex = availableDataIndices[Random.Range(0, availableDataIndices.Count)];
        availableDataIndices.Remove(dataIndex);
        usedDataIndices.Add(dataIndex);

        Airport airport = Instantiate(airportPrefab, aiportObjectContainer).GetComponent<Airport>();
        airport.Data = airportDataLoader.AirportDataList[dataIndex];
        airportList.Add(airport);
    }
}
