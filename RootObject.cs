using Newtonsoft.Json;
using System.Collections.Generic;

public class RootObject
{
    [JsonProperty("ParkedVehicles")]
    public List<Parking> ParkedVehicles { get; set; }
}
