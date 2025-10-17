using System;

[Serializable]
public class AirportData
{
    public string Name;
    public float Longitude;
    public float Latitude;

	public AirportData (string name, float longitude, float latitude)
    {
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
    }
}
