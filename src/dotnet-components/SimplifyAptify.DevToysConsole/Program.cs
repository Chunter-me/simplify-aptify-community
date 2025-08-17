using SimplifyAptify.DevToysConsole.Attributes;
using SimplifyAptify.DevToysConsole.Models;
using SimplifyAptify.DevToysConsole.Services;
using SimplifyAptify.DevToysConsole.Toys;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SimplifyAptify.DevToysConsole
{
    static class Program
    {
        #region Properties

        private static string AppSettingsFile => $"{Directory.GetCurrentDirectory()}\\appsettings.json";
        internal static AppSettings AppSettings { get; private set; } = null;

        internal class Toy
        {
            public Type Type { get; set; }
            public ToyDetailsAttribute Details { get; set; }
            public override string ToString()
            {
                return Details.Label;
            }
        }

        private static Toy[] Toys { get; set; }

        private static SelectionPrompt<Toy> ToySelectionPrompt { get; set; }

        #endregion

        static async Task Main(string[] args)
        {
            // Because we connect to Dev/Test instances with Self-Signed Certificates
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                MainConsoleHeader();


                // App Settings
                if (File.Exists(AppSettingsFile))
                {
                    var appSettingsContent = File.ReadAllText(AppSettingsFile);
                    if (!string.IsNullOrWhiteSpace(appSettingsContent))
                    {
                        AppSettings = JsonSerializer.Deserialize<AppSettings>(appSettingsContent);
                    }
                }

                if (AppSettings == null)
                {
                    throw new ArgumentException("Unable to load Setting from appsettings.json. Make sure it's in the execution folder.");
                }


                // Get Toys and Setup Selection List
                GetToys();


                // Initialize Aptify
                AptifyService.Initialize(AppSettings);


                // Show Menu
                await ToySelectionMenu();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine("");
                AnsiConsole.WriteException(ex, ExceptionFormats.NoStackTrace);
                Console.ReadLine();
            }
        }

        private static void MainConsoleHeader()
        {
            Console.Clear();

            var rule = new Rule("[yellow]Simplify Aptify - Dev Toys Console[/]")
            {
                Justification = Justify.Left
            };

            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();
        }

        private static void GetToys()
        {
            Toys = typeof(IToy).Assembly.GetTypes().Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(IToy).IsAssignableFrom(t) &&
                t.GetCustomAttributes(typeof(ToyDetailsAttribute), inherit: false).Any()
            ).Select(t => {
                return new Toy()
                {
                    Type = t,
                    Details = t.GetCustomAttributes(typeof(ToyDetailsAttribute), inherit: false).FirstOrDefault() as ToyDetailsAttribute
                };
            }).ToArray();

            if (Toys.Length <= 0)
            {
                throw new InvalidOperationException("No toys found. Make sure you have implemented IToy in your toy classes and decorated them with [ToyDetails] attribute.");
            }

            ToySelectionPrompt = new SelectionPrompt<Toy>()
                .Title("Which toy are we playing with?")
                .EnableSearch()
                .PageSize(10)
                .MoreChoicesText("[gray](Move up and down to reveal more toys)[/]")
                .AddChoices(Toys);
        }

        private static async Task ToySelectionMenu()
        {
            // Prompt for Toy Selection
            var toy = AnsiConsole.Prompt(ToySelectionPrompt);

            // Aptify Setup/Login
            await AptifyService.Instance.Login(toy.Details);

            AnsiConsole.Clear();

            try
            {
                // Execute Toy
                var ctor = toy.Type.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    var toyInstance = (IToy)Activator.CreateInstance(toy.Type);
                    await toyInstance.Execute();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]An error occurred while executing the toy.[/]");
                AnsiConsole.WriteException(ex);
                AnsiConsole.MarkupLine("");
            }

            await PlayWithAnotherToy();
        }

        internal static async Task PlayWithAnotherToy()
        {
            AnsiConsole.MarkupLine("");

            var anotherPrompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Would you like to play with another toy?")
                .AddChoices("No", "Yes")
            );

            if (anotherPrompt.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                MainConsoleHeader();
                await ToySelectionMenu();
            }
        }
    }
}
