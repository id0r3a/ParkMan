using ParkMan;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ParkingData<T> where T : IIdentifiable<string>
{
    [JsonProperty("ParkedVehicles")]
    public List<Parking<T>> ParkedVehicles { get; set; }
}
