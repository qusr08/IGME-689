using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AirportDataLoader
{
	// https://hub.arcgis.com/datasets/esri-de-content::world-airports/explore?location=0.066830%2C0.049351%2C2.15&showTable=true
	private const string WORLD_AIRPORT_CSV_PATH = "Assets/Data/Airports28062017_189278238873247918.csv";

	private enum AirportCSVIndex
	{
		OBJECT_ID, ID, AIRPORT_ID, TYPE, NAME, LATITUDE, LONGITUDE, ELEVATION, CONTINENT, ISO_COUNTRY, ISO_REGION, MUNICIPALITY, SCHEDULED_SERVICE, GPS_CODE, IATA_CODE, LOCAL_CODE, WEBSITE, WIKIPEDIA, KEYWORDS, DESCRIPTION, FREQUENCY, RUNWAY_LENGTH, RUNWAY_WIDTH, RUNWAY_SURFACE, RUNWAY_LIGHTED, RUNWAY_CLOSED, X, Y
	}

	public readonly List<AirportData> AirportDataList;

	public AirportDataLoader()
	{
		AirportDataList = new List<AirportData>();
	}

	public void LoadDataFromCSVFile()
	{
		Debug.Log("Loading Aiport Data...");

		string[] worldAiportData = File.ReadAllLines(WORLD_AIRPORT_CSV_PATH);

		for (int i = 1; i < worldAiportData.Length; i++)
		{
			// Get the airport data split up by indices from the CSV file
			char[] airportDataIndexString = worldAiportData[i].ToCharArray();
			string[] airportDataIndex = new string[Enum.GetNames(typeof(AirportCSVIndex)).Length];
			string currentData = "";
			int currentIndex = 0;
			bool inQuotes = false;

			for (int j = 0; j < airportDataIndexString.Length; j++)
			{
				char currentChar = airportDataIndexString[j];

				// Some indices of the dataset have quotes in them with commas inside
				// Need to do this to make sure it is splitting the data up at the right commas
				if (currentChar == '"')
				{
					inQuotes = !inQuotes;
				}
				else if (!inQuotes && currentChar == ',')
				{
					airportDataIndex[currentIndex] = currentData;
					currentIndex++;
					currentData = "";
				}
				else
				{
					currentData += currentChar;
				}
			}

			// Only get the biggest airports that are currently in service
			string airportType = airportDataIndex[(int)AirportCSVIndex.TYPE].Trim();
			string airportHasService = airportDataIndex[(int)AirportCSVIndex.SCHEDULED_SERVICE].Trim();
			if (airportType != "large_airport" || airportHasService != "yes")
			{
				continue;
			}

			// Add the airport data to the list
			string name = airportDataIndex[(int)AirportCSVIndex.AIRPORT_ID].Trim();
			float longitude = GetFloatFromData(airportDataIndex[(int)AirportCSVIndex.LONGITUDE]);
			float latitude = GetFloatFromData(airportDataIndex[(int)AirportCSVIndex.LATITUDE]);
			AirportDataList.Add(new AirportData(name, longitude, latitude));
		}

		Debug.Log($"Total Aiports Loaded: {AirportDataList.Count}");
	}

	private float GetFloatFromData(string dataIndex)
	{
		if (float.TryParse(dataIndex.Trim(), out float value))
		{
			return value;
		}

		return 0;
	}
}
