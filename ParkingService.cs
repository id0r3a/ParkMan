using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

public class ParkingService
{
    public List<Parking> Parkings { get; set; }
    private readonly string filePath = "ParkedVehicles.json"; // Filen där parkeringarna lagras

    public ParkingService()
    {
        Parkings = new List<Parking>(); // Initiera med tom lista
        LoadParkings();
    }

    // Ladda parkeringar från JSON-fil
    public void LoadParkings()
    {
        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<RootObject>(json); // Deserialize till RootObject
                if (data?.ParkedVehicles != null)
                {
                    Parkings.AddRange(data.ParkedVehicles);
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]No parkings found in the file.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error loading parkings: {ex.Message}[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]File not found.[/]");
        }
    }

    // Spara parkeringar till JSON-fil
    public void SaveData(string filePath, RootObject myDataBase)
    {
        myDataBase.ParkedVehicles = Parkings;
        var json = JsonConvert.SerializeObject(myDataBase, Formatting.Indented);
        File.WriteAllText(filePath, json); // Skriv den serialiserade JSON till filen
    }

    // Starta parkering
    public void StartParking(string zoneCode, string regNumber)
    {
        var vehicle = new Vehicle(regNumber.ToLower());
        var parking = new Parking(zoneCode.ToLower(), vehicle);
        parking.StartParking();
        Parkings.Add(parking);
        SaveData(filePath, new RootObject { ParkedVehicles = Parkings }); // Spara parkeringen i JSON-filen
        AnsiConsole.MarkupLine($"[green]Vehicle {regNumber} parked at {parking.StartTime} in zone {zoneCode}.[/]");
    }

    // Check out
    public void CheckOutByRegNumber(string regNumber, string filePath)
    {
        var parking = Parkings.Find(p => p.Vehicle.RegNumber.ToLower() == regNumber.ToLower());
        if (parking != null)
        {
            if (parking.EndTime != null)
            {
                AnsiConsole.MarkupLine($"[red]Vehicle {regNumber} has already been checked out at {parking.EndTime}.[/]");
                return;
            }

            parking.EndParking();
            AnsiConsole.MarkupLine($"[green]Parking for vehicle {regNumber} ended successfully.[/]");
            AnsiConsole.MarkupLine($"[yellow]Total cost: {parking.Cost} SEK.[/]");
            var paymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]How would you like to pay?[/]")
                    .AddChoices("Swish", "Card")
            );
            switch (paymentMethod.ToLower())
            {
                case "swish":
                    var phoneNumber = AnsiConsole.Ask<string>("[cyan]Enter your Swish number:[/]");
                    AnsiConsole.MarkupLine($"[green]Payment completed via Swish using phone number {phoneNumber}.[/]");
                    break;
                case "card":
                    var cardNumber = AnsiConsole.Ask<string>("[cyan]Enter your card number:[/]");
                    var expiryDate = AnsiConsole.Ask<string>("[cyan]Enter your card expiry date (MM/YY):[/]");
                    var cvv = AnsiConsole.Ask<string>("[cyan]Enter your card CVV (3-digit code):[/]");
                    AnsiConsole.MarkupLine("[green]Payment completed via card.[/]");
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Invalid payment method.[/]");
                    break;
            }
            SaveData(filePath, new RootObject { ParkedVehicles = Parkings }); // Uppdatera och spara parkeringen i JSON
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]No active parking found for vehicle {regNumber}.[/]");
        }
    }

    // Visa alla parkeringar
    public void ShowParkings()
    {
        if (Parkings.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No parkings found.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("[bold cyan]Zone Code[/]");
        table.AddColumn("Registration Number");
        table.AddColumn("Start Time");
        table.AddColumn("End Time");
        table.AddColumn("[bold red]Cost (SEK)[/]");

        foreach (var parking in Parkings)
        {
            string endTime = parking.EndTime.HasValue ? parking.EndTime.Value.ToString() : "Ongoing";
            double cost = parking.Cost;

            // Hantera pågående parkering och beräkna kostnaden baserat på den aktuella tiden
            if (!parking.EndTime.HasValue)
            {
                cost = (DateTime.Now - parking.StartTime).TotalMinutes * 0.01; // Beräkna kostnaden för pågående parkering
            }

            table.AddRow(parking.ZoneCode, parking.Vehicle.RegNumber, parking.StartTime.ToString(), endTime, cost.ToString("F2"));
        }

        AnsiConsole.Write(table);
    }
}
