using System;
using System.Text.Json;
using System.Collections.Generic;
using Spectre.Console;
using Figgle;
using ParkMan;

namespace ParkMan
{
    public class Program
    {
        static void Main(string[] args)
        {
            string dataJSONFilePath = "ParkedVehicles.json";
            ParkingData<Vehicle>? myDataBase = null;

            try
            {
                string allDataAsJSONType = File.ReadAllText(dataJSONFilePath);
                myDataBase = JsonSerializer.Deserialize<ParkingData<Vehicle>>(allDataAsJSONType);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to read or deserialize JSON file: {ex.Message}[/]");
            }

            var parkingService = new ParkingService<Vehicle>();
            parkingService.Parkings = myDataBase?.ParkedVehicles ?? new List<Parking<Vehicle>>();

            bool keepRunning = true;

            // Skriv ut appens namn i ASCII-konst
            var title = FiggleFonts.Standard.Render("Park Man");
            AnsiConsole.Write(new Markup("[bold yellow]" + title + "[/]"));

            while (keepRunning)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold green]Choose an action:[/]")
                        .AddChoices("Park", "Check out", "Show all parkings", "Exit")
                );

                switch (choice)
                {
                    case "Park":
                        var zoneCode = AnsiConsole.Ask<string>("[cyan]Enter zone code:[/]").ToLower();
                        var regNumber = AnsiConsole.Ask<string>("[cyan]Enter vehicle registration number:[/]").ToLower();
                        var vehicle = new Vehicle(regNumber);
                        parkingService.StartParking(zoneCode, vehicle);
                        parkingService.SaveData(dataJSONFilePath, new ParkingData<Vehicle> { ParkedVehicles = parkingService.Parkings });
                        ConsoleHelper.Pause();
                        break;

                    case "Check out":
                        var endRegNumber = AnsiConsole.Ask<string>("[cyan]Enter vehicle registration number to check out:[/]").ToLower();
                        var confirm = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]Are you sure you want to check out?[/]")
                                .AddChoices("Yes", "No")
                        );
                        if (confirm == "Yes")
                        {
                            parkingService.CheckOutByRegNumber(endRegNumber, dataJSONFilePath);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Check out cancelled.[/]");
                        }
                        ConsoleHelper.Pause();
                        break;

                    case "Show all parkings":
                        parkingService.ShowParkings();
                        ConsoleHelper.Pause();
                        break;

                    case "Exit":
                        AnsiConsole.MarkupLine("[bold red]Program closes[/]");
                        keepRunning = false;
                        break;

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid choice, please try again.[/]");
                        break;
                }
            }
        }
    }
}
