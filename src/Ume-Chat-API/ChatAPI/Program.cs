using ChatAPI;
using ChatAPI.Validation;
using FluentValidation;
using KeyVaultProvider;
using Models.API.ChatAPI;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IValidator<List<RequestMessage>>, RequestMessagesValidator>();

builder.Configuration.AddAzureKeyVaultWithReferenceSupport();
builder.Configuration.AddVariables();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("api/chat",
            async (HttpContext context, List<RequestMessage> messages, bool? stream = false) =>
            {
                return await DataManager.ChatAsync(context, messages, stream ?? false);
            })
   .AddEndpointFilter<ValidationFilter<List<RequestMessage>>>();

app.Run();
