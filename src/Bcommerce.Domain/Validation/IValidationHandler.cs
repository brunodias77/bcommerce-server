

namespace Bcommerce.Domain.Validation;

public interface IValidationHandler
{
        IValidationHandler Append(Error error);
        IValidationHandler Append(IValidationHandler handler);
        T? Validate<T>(IValidation<T> validation);
        IReadOnlyList<Error> GetErrors();
        bool HasError() => GetErrors().Count > 0;
        Error? FirstError() => GetErrors().Count > 0 ? GetErrors()[0] : null;
}
