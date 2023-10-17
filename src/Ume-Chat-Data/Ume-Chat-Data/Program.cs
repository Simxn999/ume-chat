using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ume_Chat_Data.Clients;
using Ume_Chat_KeyVaultProvider;
using Ume_Chat_Utilities;
using Ume_Chat_Utilities.Logger;

ILogger logger;

try
{
    Console.Title = "Data Synchronization";
    Console.WriteLine("Initializing...");

    var host = new HostBuilder().ConfigureAppConfiguration((_, builder) =>
                                 {
                                     builder.AddEnvironmentVariables();
                                     builder.SetBasePath(Environment.CurrentDirectory);
                                     builder.AddJsonFile("appsettings.json");

                                     var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                                     if (env == "Development")
                                         builder.AddJsonFile("appsettings.Development.json", true);

                                     builder.AddAzureKeyVaultWithReferenceSupport();

                                     var appConfigConnectionString = builder.Build()["DATASYNC_APP_CONFIGURATION_CONNECTION_STRING"];

                                     ArgumentNullException.ThrowIfNull(appConfigConnectionString);

                                     builder.AddAzureAppConfiguration(o => o.Connect(appConfigConnectionString));

                                     builder.AddVariables(true);
                                 })
                                .ConfigureLogging((_, logging) => { logging.AddProvider(new UmeLoggerProvider()); })
                                .Build();

    logger = host.Services.GetRequiredService<ILogger<Program>>();
    Variables.AddLogger(logger);

    Console.Clear();
    await Run();
}
catch (Exception e)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(e.Message);
    Console.ResetColor();
}

Console.WriteLine("\nPress any key to continue...");
Console.ReadKey(true);

return;

async Task Run()
{
    var dataClient = await DataClient.CreateAsync(logger);
    await dataClient.SynchronizeAsync();
}
