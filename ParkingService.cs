using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using ParkMan;

public class ParkingService<T> where T : IIdentifiable<string>
{
    public List<Parking<T>> Parkings { get; set; }
    private readonly string filePath = "ParkedVehicles.json"; // Filen där parkeringarna lagras

    public ParkingService()
    {
        Parkings = new List<Parking<T>>(); // Initiera med tom lista
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
                var data = JsonConvert.DeserializeObject<ParkingData<T>>(json); // Deserialize till RootObject
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
    public void SaveData(string filePath, ParkingData<T> myDataBase)
    {
        myDataBase.ParkedVehicles = Parkings;
        var json = JsonConvert.SerializeObject(myDataBase, Formatting.Indented);
        File.WriteAllText(filePath, json); // Skriv den serialiserade JSON till filen
    }

    // Starta parkering
    public void StartParking(string zoneCode, T vehicle)
    {
        var parking = new Parking<T>(zoneCode.ToLower(), vehicle);
        parking.StartParking();
        Parkings.Add(parking);
        SaveData(filePath, new ParkingData<T> { ParkedVehicles = Parkings }); // Spara parkeringen i JSON-filen
        AnsiConsole.MarkupLine($"[green]Vehicle {vehicle.Id} parked at {parking.StartTime} in zone {zoneCode}.[/]");
    }

    // Check out
    public void CheckOutByRegNumber(string regNumber, string filePath)
    {
        var parking = Parkings.Find(p => p.Vehicle.Id.ToLower() == regNumber.ToLower());
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
                    string phoneNumber;
                    do
                    {
                        phoneNumber = AnsiConsole.Ask<string>("[cyan]Enter your Swish number:[/]");
                        if (phoneNumber.Length != 10 || !long.TryParse(phoneNumber, out _));
                        {
                            AnsiConsole.Clear();
                            AnsiConsole.MarkupLine("[red]Invalid Swish number. The number must start with '07' and contain exactly 10 digits.[/]");
                        }
                    }
                    while (!IsValidSwishNumber(phoneNumber));

                    AnsiConsole.MarkupLine($"[green]Thank you for your payment![/]");
                    AnsiConsole.MarkupLine("[white]We look forward to serving you again![/]");
                    break;

                case "card":
                    string cardNumber;
                    do
                    {
                        cardNumber = AnsiConsole.Ask<string>("[cyan]Enter your card number:[/]");
                        if (cardNumber.Length != 14 || !long.TryParse(cardNumber, out _))
                        {
                            AnsiConsole.Clear();
                            AnsiConsole.MarkupLine("[red]Invalid card number. It must always be a 14-digit number.[/]");
                        }
                    }
                    while (cardNumber.Length != 14 || !long.TryParse(cardNumber, out _));

                    string expiryDate;
                    do
                    {
                        expiryDate = AnsiConsole.Ask<string>("[cyan]Enter your card expiry date (MM/YY):[/]");
                        if (!ValidateExpiryDate(expiryDate))
                        {
                            AnsiConsole.Clear();
                        }
                    }
                    while (!ValidateExpiryDate(expiryDate));

                    string cvv;
                    do
                    {
                        cvv = AnsiConsole.Ask<string>("[cyan]Enter your card CVV (3-digit code):[/]");
                        if (cvv.Length != 3 || !int.TryParse(cvv, out _))
                        {
                            AnsiConsole.Clear();
                            AnsiConsole.MarkupLine("[red]Invalid CVV. It must be a 3-digit number.[/]");
                        }
                    }
                    while (cvv.Length != 3 || !int.TryParse(cvv, out _));

                    AnsiConsole.MarkupLine("[green]Thank you for your payment![/]");
                    AnsiConsole.MarkupLine("[white]We look forward to serving you again![/]");

                    break;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid payment method.[/]");
                    break;
            }
            SaveData(filePath, new ParkingData<T> { ParkedVehicles = Parkings }); // Uppdatera och spara parkeringen i JSON
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]No active parking found for vehicle {regNumber}.[/]");
        }
    }

    // Valideringsfunktion för utgångsdatum
    private bool ValidateExpiryDate(string expiryDate)
    {
        if (DateTime.TryParseExact(expiryDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            // Kontrollera att månaden är mellan 1 och 12
            if (parsedDate.Month < 1 || parsedDate.Month > 12)
            {
                AnsiConsole.MarkupLine("[red]Invalid expiry date. The month must be between 01 and 12.[/]");
                return false;
            }
            // Kontrollera att utgångsdatumet inte är tidigare än den nuvarande månaden
            return parsedDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }
        AnsiConsole.MarkupLine("[red]Invalid expiry date format. Please use this format MM/YY.[/]");
        return false;
    }

    // Valideringsfunktion för Swish-nummer
    private bool IsValidSwishNumber(string phoneNumber)
    {
        return phoneNumber.StartsWith("07") && phoneNumber.Length == 10 && long.TryParse(phoneNumber, out _);
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
                cost = (DateTime.Now - parking.StartTime).TotalMinutes * 0.001; // Beräkna kostnaden för pågående parkering
            }

            table.AddRow(parking.ZoneCode, parking.Vehicle.Id, parking.StartTime.ToString(), endTime, cost.ToString("F2"));
        }

        AnsiConsole.Write(table);
    }
}
