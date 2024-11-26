using System;
using System.Text.Json;
using System.Collections.Generic;
using Spectre.Console;
using Figgle;

namespace NemoPark
{
    public class Program
    {
        static void Main(string[] args)
        {
            string dataJSONFilePath = "ParkedVehicles.json";
            string allDataAsJSONType = File.ReadAllText(dataJSONFilePath);

            RootObject myDataBase = JsonSerializer.Deserialize<RootObject>(allDataAsJSONType)!;

            ParkingService parkingService = new ParkingService();
            parkingService.Parkings = myDataBase.ParkedVehicles ?? new List<Parking>();

            bool keepRunning = true;

            // Skriv ut appens namn i ASCII-konst
            var title = FiggleFonts.Standard.Render("Park Man");
            AnsiConsole.Write(new Markup("[bold yellow]" + title + "[/]"));

            while (keepRunning)
            {
                //AnsiConsole.MarkupLine("[bold cyan]Welcome to the Parking Management System![/]");
                //AnsiConsole.MarkupLine("[cyan]Please choose an option:[/]");
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold green]Choose your action:[/]")
                        .AddChoices("Park", "Check out", "Show all parkings", "Exit")
                );

                switch (choice)
                {
                    case "Park":
                        var zoneCode = AnsiConsole.Ask<string>("[cyan]Enter zone code:[/]").ToLower();
                        var regNumber = AnsiConsole.Ask<string>("[cyan]Enter vehicle registration number:[/]").ToLower();
                        parkingService.StartParking(zoneCode, regNumber);
                        parkingService.SaveData(dataJSONFilePath, new RootObject { ParkedVehicles = parkingService.Parkings });
                        Pause();
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
                        Pause();
                        break;

                    case "Show all parkings":
                        parkingService.ShowParkings();
                        Pause();
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

        static void Pause()
        {
            AnsiConsole.MarkupLine("\n[blue]Press any key to continue...[/]");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
