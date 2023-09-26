using FluentValidation;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API.Validation;

/// <summary>
///     Validates a list of RequestMessage.
/// </summary>
public class RequestMessagesValidator : AbstractValidator<List<RequestMessage>>
{
    public RequestMessagesValidator()
    {
        RuleForEach(x => x).SetValidator(new RequestMessageValidator());
    }
}

/// <summary>
///     Validates a RequestMessage.
/// </summary>
public class RequestMessageValidator : AbstractValidator<RequestMessage>
{
    public RequestMessageValidator()
    {
        // Validate role
        RuleFor(x => x.Role.ToLower())
           .Must(x => x is "assistant" or "user")
           .WithMessage("Role must be either 'assistant' or 'user'!");

        // Validate message
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message must be set!");
    }
}
