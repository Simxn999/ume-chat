using FluentValidation;
using Ume_Chat_External_API;
using Ume_Chat_External_API.Validation;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.API.Request;
using Ume_Chat_KeyVaultProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IValidator<List<RequestMessage>>, RequestMessagesValidator>();

builder.Configuration.AddAzureKeyVaultWithReferenceSupport();

var app = builder.Build();

Variables.Initialize(app.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("api/chat", async (HttpContext context, List<RequestMessage> messages, bool? stream = false) =>
                            await DataManager.ChatAsync(context, messages, stream ?? false))
   .AddEndpointFilter<ValidationFilter<List<RequestMessage>>>();

app.Run();