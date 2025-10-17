public class AirportData
{
	public string Name { get; private set; }
	public float Longitude { get; private set; }
	public float Latitude { get; private set; }

	public AirportData (string name, float longitude, float latitude)
    {
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
    }
}
