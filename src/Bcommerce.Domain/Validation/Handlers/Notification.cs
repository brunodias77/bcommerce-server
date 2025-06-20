using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcommerce.Domain.Exceptions;

namespace Bcommerce.Domain.Validation.Handlers;


public class Notification : IValidationHandler
{
    private readonly List<Error> _errors;

    private Notification(List<Error> errors) => _errors = errors;
    
    public static Notification Create() => new(new List<Error>());

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
}