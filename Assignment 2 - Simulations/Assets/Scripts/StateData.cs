using System;
using System.Collections.Generic;
using UnityEngine;

public class StateData
{
	public string Name { get; set; }
	public int FIPS { get; set; }
	public List<List<Vector2>> GeoData { get; set; }
	public Dictionary<int, float> TemperatureByYear { get; set; }
	public int EarliestYear { get; set; }
	public List<MeshRenderer> MeshRenderers { get; set; }

	public StateData ( )
	{
		Name = "NA";
		FIPS = -1;
		GeoData = new List<List<Vector2>>( );
		TemperatureByYear = new Dictionary<int, float>( );
		EarliestYear = int.MaxValue;
		MeshRenderers = new List<MeshRenderer>( );
	}

	public float SafeGetTemperature (int year)
	{
		while (!TemperatureByYear.ContainsKey(year) && year > EarliestYear)
		{
			year--;
		}

		return TemperatureByYear[Math.Max(EarliestYear, year)];
	}
}
