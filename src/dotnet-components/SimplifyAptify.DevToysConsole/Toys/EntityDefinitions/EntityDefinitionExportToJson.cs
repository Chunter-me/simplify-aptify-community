using Newtonsoft.Json.Linq;
using SimplifyAptify.DevToysConsole.Attributes;
using SimplifyAptify.DevToysConsole.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SimplifyAptify.DevToysConsole.Toys.EntityDefinitions
{
    [ToyDetails(
        key: "SimplifyAptify.DevToysConsole.Toys.EntityDefinitions.EntityDefinitionExportToJson",
        label: "Entity Definitions: Export to Json for Development",
        useFramework: true,
        useSoa: true
    )]
    internal class EntityDefinitionExportToJson : IToy
    {
        #region Properties

        private static string SaveToFolderPath { get; set; }
        private readonly AptifyService AptifyClient = AptifyService.Instance;

        #endregion


        #region Content Results

        private sealed class ContentResult
        {
            public string AppContent { get; set; }
            public int? RecordId { get; set; }
            public string LookupKey { get; set; }
            public string Json { get; set; }
            public string SoaUrl { get; set; }
            public string ErrorMessage { get; set; }

            public ContentResult(string content, int? recordId, string lookupKey, string json = "")
            {
                AppContent = content;
                RecordId = recordId;
                LookupKey = lookupKey;
                Json = json;
            }

            public string ToQueryString()
            {
                var vals = new Dictionary<string, string>
                {
                    { "AppContent", AppContent }
                };

                if (RecordId != null && RecordId > 0)
                {
                    vals.Add("RecordID", RecordId.ToString());
                }

                if (!string.IsNullOrWhiteSpace(LookupKey))
                {
                    vals.Add("LookupKey", LookupKey);
                }

                return string.Join("&", vals.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            }

            public async Task ExportToFile()
            {
                var folderPath = $"{SaveToFolderPath}{AppContentFolder[AppContent]}";

                var fileName = string.Empty;
                if (AppContent == "EntityList")
                {
                    fileName = "entities-list.json";
                }

                else if (AppContent == "Article")
                {
                    fileName = $"{LookupKey}.json";
                }

                else if (AppContent == "Blocks")
                {
                    fileName = "blocks.json";
                }

                else if (AppContent == "Definition" || AppContent == "ViewDetails")
                {
                    fileName = $"{RecordId}.json";
                }

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var filePath = Path.Combine(folderPath, fileName);
                    await ExportFile(filePath, Json);
                }
            }
        }

        private static readonly Dictionary<string, string> AppContentFolder = new Dictionary<string, string>
        {
            { "EntityList", "" },
            { "Article", "\\Articles" },
            { "Blocks", "" },
            { "Definition", "\\Definitions" },
            { "ViewDetails", "\\ViewDetails" }
        };

        private readonly List<ContentResult> ToExport = new List<ContentResult>();
        private List<ContentResult> ToExportErrors = new List<ContentResult>();

        #endregion


        #region Export Entity Definitions to Json

        public async Task Execute()
        {
            SaveToFolderDirectory();

            await GetContentResults();

            await ExportContentResults();

            AnsiConsole.MarkupLine("[green]Export Complete![/]");
        }

        private async Task GetContentResults()
        {
            var entities = GetEntityIDs();

            if (entities.Length > 0)
            {
                var progressIncrement = (float)100 / (entities.Length + 5);

                await AnsiConsole.Progress()
                    .StartAsync(async ctx => {
                        var definitionsTask = ctx.AddTask("[green]Retrieving Data from Aptify Sao[/]");

                        definitionsTask.Increment(progressIncrement);
                        await Task.Delay(100);

                        var entityListTask = Task.Run(async () => {
                            await GetSdoContent("EntityList");
                            definitionsTask.Increment(progressIncrement);
                        });

                        var articlesTask = Task.Run(async () => {
                            await GetSdoContent("Article", lookupKey: "_xAllArticles");
                            definitionsTask.Increment(progressIncrement);
                        });

                        var blocksTask = Task.Run(async () => {
                            await GetSdoContent("Blocks");
                            definitionsTask.Increment(progressIncrement);
                        });

                        definitionsTask.Increment(progressIncrement);

                        var tasks = new List<Task>()
                        {
                            entityListTask,
                            articlesTask,
                            blocksTask
                        };

                        var definitionTasks = entities.Select(entityId => Task.Run(async () => {
                            await Task.WhenAll(
                                GetSdoContent("Definition", entityId),
                                GetSdoContent("ViewDetails", entityId, "EntityID")
                            );

                            definitionsTask.Increment(progressIncrement);
                        }));

                        tasks.AddRange(definitionTasks);

                        await Task.WhenAll(tasks);

                        definitionsTask.Value(100.00);
                    });

                AnsiConsole.MarkupLine("");
                AnsiConsole.MarkupLine($"Exporting {ToExport.Count} Objects - Errors: {ToExportErrors.Count}");
                AnsiConsole.MarkupLine("");

                // Export Errors
                if (ToExportErrors.Count > 0)
                {
                    var errorContent = System.Text.Json.JsonSerializer.Serialize(
                        ToExportErrors,
                        new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }
                    );

                    await ExportFile($"{SaveToFolderPath}\\errors.json", errorContent);
                }
            }
        }

        private async Task ExportContentResults()
        {
            if (ToExport.Count > 0)
            {
                var exportIncrement = (float)100 / (ToExport.Count);
                ToExportErrors = new List<ContentResult>();

                await AnsiConsole.Progress()
                    .StartAsync(async ctx => {
                        var exportTask = ctx.AddTask("[green]Exporting to Json Files[/]");

                        await Task.WhenAll(ToExport.Select(result => Task.Run(async () => {
                            try
                            {
                                await result.ExportToFile();
                            }
                            catch (Exception ex)
                            {
                                result.ErrorMessage = ex.Message;
                                ToExportErrors.Add(result);
                            }

                            exportTask.Increment(exportIncrement);
                        })));

                        exportTask.Value(100.00);
                    });
            }
        }

        #endregion


        #region Utils

        private void SetHeader()
        {
            AnsiConsole.Clear();

            var rule = new Rule("[yellow]Entity Definitions - Export to JSON[/]")
            {
                Justification = Justify.Left
            };

            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("");
        }

        private void SaveToFolderDirectory()
        {
            SetHeader();

            if (!string.IsNullOrWhiteSpace(SaveToFolderPath))
            {
                AnsiConsole.MarkupLine($"[red]Sorry: {SaveToFolderPath} doesn't exist or you do not have permissions to it.[/]");
                AnsiConsole.MarkupLine("");
            }

            AnsiConsole.MarkupLine("[yellow]Please specify the folder path to save the exported files.[/]");
            SaveToFolderPath = AnsiConsole.Prompt(new TextPrompt<string>("Path: "));

            if (!Directory.Exists(SaveToFolderPath))
            {
                SaveToFolderDirectory();
            }
        }

        private int[] GetEntityIDs()
        {
            var entities = new List<int>();

            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("[yellow]Retrieving Entity IDs[/]");

            AnsiConsole.Status().Start("Running Query:", ctx => {
                ctx.SpinnerStyle(Style.Parse("green"));
                Thread.Sleep(100);

                using (var reader = AptifyClient.DataAction.ExecuteDataReader("EXEC spEntityDefinitionApp_sa 'EntityList'"))
                {
                    while (reader.Read())
                    {
                        entities.Add(reader.GetInt32(0));
                    }
                }

                Thread.Sleep(100);
            });

            AnsiConsole.MarkupLine($"{entities.Count} Entities IDs Retrieved.");
            AnsiConsole.MarkupLine("");

            return entities.ToArray();
        }

        private async Task GetSdoContent(string appContent, int? recordId = null, string lookupKey = null, int attempt = 0)
        {
            await Task.Delay(500);

            var results = new ContentResult(appContent, recordId, lookupKey);

            try
            {
                results.SoaUrl = $"{AptifyClient.SelectedServer.SoaServicesApiUrl}/DataObjects/EntityDefinitionApp?{results.ToQueryString()}";

                var response = await AptifyClient.AptifySoaClient.GetAsync(results.SoaUrl);
                results.Json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (attempt <= 50)
                    {
                        attempt++;
                        await GetSdoContent(appContent, recordId, lookupKey, attempt);
                        return;
                    }

                    throw new WebException();
                }

                if (appContent.Equals("Article", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonObj = JObject.Parse(results.Json);
                    if (jsonObj["results"] is JArray resultsArray && resultsArray.Count > 0)
                    {
                        var articles = resultsArray[0];
                        var links = resultsArray[1];

                        foreach (var article in articles)
                        {
                            var articleId = article["ID"]?.Value<int>();
                            var articleLookupKey = article["LookupKey"]?.Value<string>();
                            var articleLinks = links.Where(filter => (filter["ArticleID"]?.Value<int>()) == articleId);

                            var articleSdo = new
                            {
                                dataObjectName = "EntityDefinitionApp",
                                results = new List<IEnumerable<JToken>>()
                                {
                                    new JToken[] { article },
                                    articleLinks
                                }
                            };

                            var articleJson = Newtonsoft.Json.JsonConvert.SerializeObject(articleSdo);
                            var articleContentResult = new ContentResult("Article", null, articleLookupKey, articleJson);
                            ToExport.Add(articleContentResult);
                        }
                    }
                }

                else if (appContent.Equals("ViewDetails", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonObj = JObject.Parse(results.Json);
                    if (jsonObj["results"] is JArray resultsArray && resultsArray.Count > 0)
                    {
                        var filters = resultsArray[0] as JArray;
                        var fields = resultsArray[1] as JArray;
                        var views = resultsArray[2] as JArray;

                        foreach (var item in views)
                        {
                            var viewId = item["ViewID"]?.Value<int>();
                            var viewFilters = filters.Where(filter => (filter["ViewID"]?.Value<int>()) == viewId);
                            var viewFields = fields.Where(field => (field["ViewID"]?.Value<int>()) == viewId);
                            var viewSql = views.Where(sql => (sql["ViewID"]?.Value<int>()) == viewId);

                            var viewSdo = new
                            {
                                dataObjectName = "EntityDefinitionApp",
                                results = new List<IEnumerable<JToken>>()
                                {
                                    viewFilters,
                                    viewFields,
                                    viewSql
                                }
                            };

                            var viewJson = Newtonsoft.Json.JsonConvert.SerializeObject(viewSdo);
                            var viewContentResult = new ContentResult("ViewDetails", viewId, null, viewJson);
                            ToExport.Add(viewContentResult);
                        }
                    }
                }

                else
                {
                    ToExport.Add(results);
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(results.Json))
                {
                    results.Json = ex.Message;
                }

                ToExportErrors.Add(results);
            }
        }

        private static async Task ExportFile(string filepath, string content)
        {
            // Create Directory if it does not exist
            var directoryPath = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create File if it does not exist
            if (!File.Exists(filepath))
            {
                File.Create(filepath).Close();
            }

            // Write to File
            using (var writer = new StreamWriter(filepath, false))
            {
                await writer.WriteAsync(content);
            }
        }

        #endregion
    }
}