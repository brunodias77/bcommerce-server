using Bcommerce.Domain.Exceptions;

namespace Bcommerce.Domain.Validations.Handlers;

public class Notification : IValidationHandler
{
    private readonly List<Error> _errors;

    private Notification(List<Error> errors)
    {
        _errors = errors;
    }

    public static Notification Create() => new(new List<Error>());

    public static Notification Create(Exception exception) =>
        Create(new Error(exception.Message ?? "Unknown error"));

    public static Notification Create(Error error)
    {
        var notification = new Notification(new List<Error>());
        return (Notification)notification.Append(error);
    }

    public IValidationHandler Append(Error error)
    {
        _errors.Add(error);
        return this;
    }

    public IValidationHandler Append(IValidationHandler handler)
    {
        _errors.AddRange(handler.GetErrors());
        return this;
    }

    public T? Validate<T>(IValidation<T> validation)
    {
        try
        {
            return validation.Validate();
        }
        catch (DomainException ex)
        {
            _errors.AddRange(ex.Errors);
        }
        catch (Exception ex)
        {
            _errors.Add(new Error(ex.Message));
        }

        return default;
    }

    public IReadOnlyList<Error> GetErrors() => _errors;

    public bool HasError() => _errors.Count > 0;

    public Error? FirstError() => _errors.Count > 0 ? _errors[0] : null;
}