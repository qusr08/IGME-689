using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
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
		CreateStateGeometry( );
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
				foreach (JSONNode coordinatePairNode in coordinateGroupNode)
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

	private void CreateStateGeometry ( )
	{

		UpdateStateGeometry( );
	}

	private void UpdateStateGeometry ( )
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