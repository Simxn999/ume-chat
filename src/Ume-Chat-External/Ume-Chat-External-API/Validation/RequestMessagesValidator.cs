using FluentValidation;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API.Validation;

public class RequestMessagesValidator : AbstractValidator<List<RequestMessage>>
{
    public RequestMessagesValidator()
    {
        RuleForEach(x => x).SetValidator(new RequestMessageValidator());
    }
}

public class RequestMessageValidator : AbstractValidator<RequestMessage>
{
    public RequestMessageValidator()
    {
        RuleFor(x => x.Role.ToLower())
           .Must(x => x is "assistant" or "user")
           .WithMessage("Role must be either 'assistant' or 'user'!");

        RuleFor(x => x.Message)
           .NotEmpty()
           .WithMessage("Message must be set!");
    }
}