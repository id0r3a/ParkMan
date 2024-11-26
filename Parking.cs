using System;

public class Parking
{
    public string ZoneCode { get; set; }
    public Vehicle Vehicle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; } // Nullable för att hantera pågående parkeringar
    public double Cost { get; set; }

    public Parking(string zoneCode, Vehicle vehicle)
    {
        ZoneCode = zoneCode.ToLower(); // Normalisera till små bokstäver
        Vehicle = vehicle;
        StartTime = DateTime.Now; // Sätt starttid till nu när parkeringen startas
    }

    public void StartParking()
    {
        StartTime = DateTime.Now; // Använd nuvarande tid som starttid
    }

    public void EndParking()
    {
        EndTime = DateTime.Now; // Sätt sluttid till nu när parkeringen slutar
        Cost = (EndTime.Value - StartTime).TotalMinutes * 0.1; // Exempel på kostnad: 0.5 SEK per minut
    }
}
