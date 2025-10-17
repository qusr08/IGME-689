using UnityEngine;

public class AirportSpawner : MonoBehaviour
{
	[SerializeField] private Transform aiportObjectContainer;
	[SerializeField] private GameObject airportPrefab;

	private AirportDataLoader airportDataLoader;

	private void Awake()
	{
		airportDataLoader = new AirportDataLoader();
		airportDataLoader.LoadDataFromCSVFile();

		foreach (AirportData airportData in airportDataLoader.AirportDataList)
		{
			Airport airport = Instantiate(airportPrefab, aiportObjectContainer).GetComponent<Airport>();
			airport.Data = airportData;
		}
	}
}
