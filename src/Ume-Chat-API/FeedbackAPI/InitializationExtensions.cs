using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Ume_Chat_Data_Feedback;
using Ume_Chat_Data_Feedback.Clients;
using Ume_Chat_Utilities;

namespace Ume_Chat_API_Feedback;

/// <summary>
///     Feedback API Initialization extensions.
/// </summary>
public static class InitializationExtensions
{
    /// <summary>
    ///     Initializes feedback clients with dependency injection.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    public static void AddScopedClients(this IServiceCollection services)
    {
        services.AddScoped<FeedbackSubmissionsClient>();
        services.AddScoped<CategoriesClient>();
        services.AddScoped<StatusesClient>();
    }

    /// <summary>
    ///     Initializes feedback submissions database.
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    public static void AddFeedbackDatabase(this IServiceCollection services)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        var connectionString = Variables.Get("FEEDBACK_DATABASE_CONNECTION_STRING");

        services.AddDbContext<FeedbackContext>(options => { options.UseSqlServer(connectionString, b => b.MigrationsAssembly(assemblyName)); });
    }

    /// <summary>
    ///     Migrates database.
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void MigrateDatabase(this WebApplication app)
    {
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<FeedbackContext>();
        context.Database.Migrate();
    }

    /// <summary>
    ///     Initializes Swagger UI for development.
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void InitializeSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.DocumentTitle = "Feedback API";
        });
    }
}
