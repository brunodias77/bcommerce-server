using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Exceptions;

public class DomainException : Exception
{
    public IReadOnlyList<Error> Errors { get; }

    public DomainException(string message) : base(message)
    {
        Errors = new List<Error> { new Error(message) };
    }

    public DomainException(IReadOnlyList<Error> errors) : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    public static void ThrowWhen(bool condition, string message)
    {
        if (condition)
        {
            throw new DomainException(message);
        }
    }
    
    private static string BuildErrorMessage(IReadOnlyList<Error> errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return "Um erro de domínio ocorreu.";
        }
        return "Erro de domínio: " + string.Join(", ", errors.Select(e => e.Message));
    }
}