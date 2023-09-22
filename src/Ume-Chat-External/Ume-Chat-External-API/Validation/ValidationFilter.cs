using FluentValidation;

namespace Ume_Chat_External_API.Validation;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.SingleOrDefault(x => x?.GetType() == typeof(T)) is not T validatable)
            return Results.BadRequest();

        var validationResult = await validator.ValidateAsync(validatable);

        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors.ToResponse());

        return await next(context);
    }
}