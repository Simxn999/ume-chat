using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ume_Chat_External_General;
using Ume_Chat_KeyVaultProvider;

var host = new HostBuilder() 
          .ConfigureFunctionsWorkerDefaults(builder => builder.Services.AddLogging())
          .ConfigureAppConfiguration((context, builder) =>
                                     {
                                         builder.AddEnvironmentVariables();
                                         builder.SetBasePath(Environment.CurrentDirectory);
                                         builder.AddJsonFile("appsettings.json");

                                         if (context.HostingEnvironment.IsDevelopment())
                                             builder.AddJsonFile("appsettings.Development.json", true);

                                         builder.AddAzureKeyVaultWithReferenceSupport();
                                     })
          .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
Variables.Initialize(config);

host.Run();