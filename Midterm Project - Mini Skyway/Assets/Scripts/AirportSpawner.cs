using UnityEngine;

public class AirportSpawner : MonoBehaviour
{
	private AirportDataLoader airportDataLoader;

	private void Awake()
	{
		airportDataLoader = new AirportDataLoader();
		airportDataLoader.LoadDataFromCSVFile();
	}
}
