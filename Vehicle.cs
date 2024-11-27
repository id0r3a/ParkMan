using NemoPark;
using Newtonsoft.Json;

public class Vehicle : IIdentifiable<string>
{
    [JsonProperty("RegNumber")]
    public string RegNumber { get; set; }

    // Parameterlös konstruktor behövs för deserialisering
    public Vehicle()
    {
    }

    public Vehicle(string regNumber)
    {
        RegNumber = regNumber.ToLower(); // Normalisera till små bokstäver
    }

    // Implementera IIdentifiable
    [JsonIgnore]
    public string Id
    {
        get => RegNumber;
        set => RegNumber = value;
    }
}
