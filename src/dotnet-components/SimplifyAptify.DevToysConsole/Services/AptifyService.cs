using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.LoginServices;
using SimplifyAptify.DevToysConsole.Attributes;
using SimplifyAptify.DevToysConsole.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SimplifyAptify.DevToysConsole.Services
{
    internal sealed class AptifyService
    {
        #region Instance

        private static Lazy<AptifyService> _instance;
        private static bool _initialized = false;

        public static AptifyService Instance
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException("AptifyService must be initialized before use.");
                }

                return _instance.Value;
            }
        }

        public static void Initialize(AppSettings settings)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("AptifyService has already been initialized.");
            }

            _instance = new Lazy<AptifyService>(() => new AptifyService(settings));
            _initialized = true;
        }

        #endregion


        #region Properties & Constructor

        private List<AptifyServer> Servers { get; set; }
        public AptifyServer SelectedServer { get; set; }

        private bool isFrameworkLoggedIn = false;
        private bool isSoaLoggedIn = false;

        private UserCredentials _credentials = null;
        private AptifyApplication _application = null;
        private DataAction _dataAction = null;

        public HttpClient AptifySoaClient { get; private set; } = null;

        public AptifyApplication Application
        {
            get
            {
                if (_application == null)
                {
                    _application = new AptifyApplication(_credentials);
                }

                return _application;
            }
        }

        public DataAction DataAction
        {
            get
            {
                if (_dataAction == null)
                {
                    _dataAction = new DataAction(_credentials);
                }

                return _dataAction;
            }
        }

        private AptifyService(AppSettings settings)
        {
            Servers = settings.AptifyServers?.OrderBy(s => s.Name).ToList() ?? new List<AptifyServer>();
        }

        #endregion


        public async Task Login(ToyDetailsAttribute toyDetails)
        {
            var executeLogin = true;

            // Re-login?
            if ((toyDetails.UseFramework && isFrameworkLoggedIn) || (toyDetails.UseSoa && isSoaLoggedIn))
            {
                var use = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Already Logged In to: {_credentials.Server}")
                        .AddChoices(
                            "Use Existing",
                            "Login to Different Server"
                        )
                );

                executeLogin = use.Equals("Login to Different Server", StringComparison.OrdinalIgnoreCase);
            }


            // Login to Aptify
            if (executeLogin && (toyDetails.UseFramework || toyDetails.UseSoa))
            {
                SetHeader();

                // Select Server
                var serverOptions = new SelectionPrompt<AptifyServer>()
                    .Title("Connect To:")
                    .AddChoices(Servers);

                serverOptions.AddChoice(new AptifyServer() { Name = "Other" });

                SelectedServer = AnsiConsole.Prompt(serverOptions);

                if (SelectedServer.Name.Equals("Other", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.WriteLine("Other Server Details:");

                    SelectedServer.Name = AnsiConsole.Prompt(new TextPrompt<string>("Name: "));
                    SelectedServer.Server = AnsiConsole.Prompt(new TextPrompt<string>("Server IP/Name: "));
                    SelectedServer.Soa = AnsiConsole.Prompt(new TextPrompt<string>("Soa Url (ex: https://aptify.orgname.org/AptifyServicesAPI): "));

                    var existing = Servers.FirstOrDefault(s => s.Name.Equals(SelectedServer.Name, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.Server = SelectedServer.Server;
                        existing.Soa = SelectedServer.Soa;
                    }
                    else
                    {
                        Servers.Add(SelectedServer);
                    }
                }

                SetHeader();

                if (AptifySoaClient == null)
                {
                    AptifySoaClient = new HttpClient();
                }


                // User Login details
                var loginCompleted = false;
                var attempting = string.Empty;

                do
                {
                    attempting = string.Empty;
                    var (username, password, isTrusted) = LoginPrompt(toyDetails);

                    var frameworkLoginSuccess = !toyDetails.UseFramework;
                    var soaLoginSuccess = !toyDetails.UseSoa;

                    try
                    {
                        // Framework Login
                        if (!frameworkLoginSuccess)
                        {
                            attempting = "framework";

                            AnsiConsole.Status().Start("Logging into Aptify DB", ctx => {
                                ctx.SpinnerStyle(Style.Parse("green"));
                                Thread.Sleep(100);

                                var login = new AptifyLogin()
                                {
                                    Trusted = isTrusted,
                                    Server = SelectedServer.Server,
                                    EntitiesDatabase = "Aptify",
                                    UsersDatabase = "Aptify"
                                };

                                if (!isTrusted)
                                {
                                    login.SQLLogin = username;
                                    login.set_Password(true, password);
                                }

                                _credentials = login.Login();

                                var one = DataAction.ExecuteScalar("SELECT TOP 1 1 FROM vwPersons");
                                if (one == null || !one.ToString().Equals("1"))
                                {
                                    throw new AptifyDataException("Aptify DB: Invalid user credential, please try again.");
                                }

                                frameworkLoginSuccess = true;
                                isFrameworkLoggedIn = true;
                                Thread.Sleep(100);
                            });
                        }


                        // Soa Login
                        if (!soaLoginSuccess)
                        {
                            attempting = "soa";

                            await AnsiConsole.Status().StartAsync("Logging into Aptify Soa", async ctx => {
                                ctx.SpinnerStyle(Style.Parse("green"));
                                AnsiConsole.WriteLine("Retrieving CSRF Tokens");
                                Thread.Sleep(100);

                                var soaServicesUrl = SelectedServer.Soa.Trim().TrimEnd('/');
                                if (!soaServicesUrl.EndsWith("/Services"))
                                {
                                    soaServicesUrl = $"{soaServicesUrl}/Services";
                                }

                                SelectedServer.SoaServicesApiUrl = soaServicesUrl;

                                var userInforResponse = await AptifySoaClient.GetAsync($"{SelectedServer.SoaServicesApiUrl}/UserInformation");
                                var userInforResponseContent = await userInforResponse.Content.ReadAsStringAsync();
                                var userInfo = JsonSerializer.Deserialize<AptifySoaUserInformationResponse>(userInforResponseContent);

                                if (!string.IsNullOrWhiteSpace(userInforResponseContent))
                                {
                                    if (userInforResponse.Headers.TryGetValues("CSRFDefenseInDepthToken", out IEnumerable<string> depth))
                                    {
                                        var token = depth.FirstOrDefault() ?? string.Empty;
                                        if (!string.IsNullOrWhiteSpace(token))
                                        {
                                            AptifySoaClient.DefaultRequestHeaders.Add("CSRFDefenseInDepthToken", token);
                                        }
                                    }

                                    if (userInforResponse.Headers.TryGetValues("__RequestVerificationToken", out IEnumerable<string> reqVer))
                                    {
                                        var token = reqVer.FirstOrDefault() ?? string.Empty;
                                        if (!string.IsNullOrWhiteSpace(token))
                                        {
                                            AptifySoaClient.DefaultRequestHeaders.Add("__RequestVerificationToken", token);
                                        }

                                    }

                                    var loginParams = new Dictionary<string, string>
                                    {
                                        { "username", username },
                                        { "password", password }
                                    };

                                    var postContent = new FormUrlEncodedContent(loginParams);
                                    var authProvider = isTrusted ? "DomainWithContainer" : "SQLNeedsInputWithContainer";

                                    AnsiConsole.MarkupLine("Execute Login Endpoint");
                                    Thread.Sleep(100);

                                    var loginAttempt = await AptifySoaClient.PostAsync(
                                        $"{SelectedServer.SoaServicesApiUrl}/Authentication/Login/{authProvider}",
                                        postContent
                                    );

                                    var loginAttemptContent = await loginAttempt.Content.ReadAsStringAsync();
                                    var loginInfo = JsonSerializer.Deserialize<AptifySoaUserInformationDetails>(loginAttemptContent);

                                    if (loginAttempt.IsSuccessStatusCode)
                                    {
                                        AptifySoaClient.DefaultRequestHeaders.Add("AptifyAuthorization", $"{authProvider} {loginInfo.TokenId}");

                                        var authResponse = await AptifySoaClient.GetAsync($"{SelectedServer.SoaServicesApiUrl}/UserInformation");
                                        var authResponseContent = await authResponse.Content.ReadAsStringAsync();
                                        var authUserInfo = JsonSerializer.Deserialize<AptifySoaUserInformationResponse>(authResponseContent);

                                        if (string.IsNullOrWhiteSpace(userInforResponseContent) || !authUserInfo.Result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                                        {
                                            throw new WebException("Aptify Soa: Invalid user credential, please try again.");
                                        }

                                        soaLoginSuccess = true;
                                        isSoaLoggedIn = true;
                                    }
                                    else
                                    {
                                        throw new WebException(loginAttemptContent);
                                    }

                                    AptifySoaClient.DefaultRequestHeaders.Clear();
                                }
                                else
                                {
                                    throw new WebException($"CSRF Error: {userInfo.Error}");
                                }

                                Thread.Sleep(100);
                            });
                        }


                        // Complete?
                        loginCompleted = frameworkLoginSuccess && soaLoginSuccess;
                    }
                    catch (Exception ex)
                    {
                        SetHeader();

                        if (attempting.Equals("framework", StringComparison.OrdinalIgnoreCase))
                        {
                            AnsiConsole.MarkupLine("[red]Login Failed for DB Server[/]");
                            AnsiConsole.MarkupLine("");
                        }

                        else if (attempting.Equals("soa", StringComparison.OrdinalIgnoreCase))
                        {
                            AnsiConsole.MarkupLine("[red]Login Failed for Soa[/]");
                            AnsiConsole.MarkupLine("");
                        }

                        AnsiConsole.WriteException(ex, ExceptionFormats.NoStackTrace);
                        AnsiConsole.MarkupLine("");
                        AnsiConsole.MarkupLine("");

                        loginCompleted = false;
                    }
                } while (!loginCompleted);

                AnsiConsole.MarkupLine("Login Complete");
            }
        }


        #region Utils

        private void SetHeader()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[yellow]Aptify Application Setup & Login[/]");
            AnsiConsole.MarkupLine("");

            if (SelectedServer != null)
            {
                AnsiConsole.MarkupLine($"[yellow]Server: {SelectedServer}[/]");
                AnsiConsole.MarkupLine("");
            }
        }

        private static (string username, string password, bool isTrused) LoginPrompt(ToyDetailsAttribute toyDetails)
        {
            var username = string.Empty;
            var password = string.Empty;

            var userLoginType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Login Type:")
                    .AddChoices(
                        "Domain User (trusted)",
                        "SQL Login (untrusted)"
                    )
            );

            var isTrusted = userLoginType.StartsWith("Domain User", StringComparison.OrdinalIgnoreCase);
            if (!isTrusted || toyDetails.UseSoa)
            {
                if (isTrusted)
                {
                    AnsiConsole.WriteLine("Need creds for Soa:");
                }

                username = AnsiConsole.Prompt(new TextPrompt<string>("Username: "));

                password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Password: ").Secret()
                );
            }

            return (username, password, false);
        }

        #endregion


        #region Models

        internal class AptifyServer
        {
            public string Name { get; set; }
            public string Server { get; set; }
            public string Soa { get; set; }
            public string SoaServicesApiUrl { get; set; }

            public override string ToString()
            {
                return $"{Name} - Server: {Server} - Soa: {Soa}";
            }
        }

        private sealed class AptifySoaUserInformationResponse
        {
            [JsonPropertyName("result")]
            public string Result { get; set; } = string.Empty;


            [JsonPropertyName("userInfo")]
            public AptifySoaUserInformationDetails UserInfo { get; set; } = null;


            [JsonPropertyName("error")]
            public string Error { get; set; } = string.Empty;
        }

        private sealed class AptifySoaUserInformationDetails
        {
            public string TokenId { get; set; }
        }

        #endregion
    }
}
