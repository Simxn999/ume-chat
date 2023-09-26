using System.Diagnostics;
using FluentValidation.Results;

namespace Ume_Chat_External_API.Validation;

public class ValidationFailureResponse
{
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}

public static class ValidationFailureMapper
{
    public static ValidationFailureResponse ToResponse(this IEnumerable<ValidationFailure> failures)
    {
        return new ValidationFailureResponse { Errors = failures.Select(f => f.ErrorMessage) };
    }
}
