using SimplifyAptify.DevToysConsole.Attributes;
using SimplifyAptify.DevToysConsole.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimplifyAptify.DevToysConsole.Toys.EntityDefinitions
{
    [ToyDetails(
        key: "SimplifyAptify.DevToysConsole.Toys.EntityDefinitions.EntityDefinitionGenerateDefinitions",
        label: "Entity Definitions: Generate Default Definition Records",
        useFramework: true
    )]
    internal class EntityDefinitionGenerateDefinitions : IToy
    {
        #region Properties

        private readonly AptifyService AptifyClient = AptifyService.Instance;
        private readonly List<string> Errors = new List<string>();

        #endregion


        public async Task Execute()
        {
            var rule = new Spectre.Console.Rule("[yellow]Entity Definitions - Generate Base Definitions[/]")
            {
                Justification = Justify.Left
            };

            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("");

            GenerateEntityDefinitions();

            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("");

            if (Errors.Count > 0)
            {
                AnsiConsole.MarkupLine("[red]ERRORS:[/]");
                AnsiConsole.MarkupLine(JsonSerializer.Serialize(Errors, new JsonSerializerOptions() { WriteIndented = true }));
            }
            else
            {
                AnsiConsole.MarkupLine("[green]COMPLETE[/]");
            }
        }

        private void GenerateEntityDefinitions()
        {
            var entities = new Dictionary<int, int>();

            AnsiConsole.Status().Start("Retrieving Entity Data", ctx => {
                ctx.SpinnerStyle(Style.Parse("green"));
                Thread.Sleep(100);

                using (var reader = AptifyClient.DataAction.ExecuteDataReader("EXEC spEntityDefinitionApp_sa 'EntityList'"))
                {
                    while (reader.Read())
                    {
                        entities.Add(
                            (int)reader.GetValue(reader.GetOrdinal("ID")),
                            (int)reader.GetValue(reader.GetOrdinal("DefinitionID"))
                        );
                    }
                }

                Thread.Sleep(100);
            });

            if (entities.Count > 0)
            {
                var entityInc = (float)100 / (float)entities.Count;

                AnsiConsole.Progress().Start(ctx => {
                    var entitiesTask = ctx.AddTask($"[green]Generating {entities.Count} Entity Definitions[/]");

                    foreach (var entity in entities)
                    {
                        try
                        {
                            var ge = AptifyClient.Application.GetEntityObject("EntityDefinition_sa", entity.Value);
                            if (ge.RecordID <= 0)
                            {
                                ge.SetValue("EntityID", entity.Key);
                            }

                            if (!ge.Save(false))
                            {
                                throw new DataException(ge.LastError);
                            }
                        }
                        catch (Exception ex)
                        {
                            Errors.Add($"Definition Save Error - DefinitionID: {entity.Value} - EntityID: {entity.Key} - Error: {ex.Message}");
                        }

                        entitiesTask.Increment(entityInc);
                    }

                    entitiesTask.Value(100.00);
                });
            }
        }
    }
}