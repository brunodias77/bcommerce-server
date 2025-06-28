

namespace Bcommerce.Domain.Validation;

public interface IValidationHandler
{
        IValidationHandler Append(Error error);
        IValidationHandler Append(IValidationHandler handler);
        T? Validate<T>(IValidation<T> validation);
        IReadOnlyList<Error> GetErrors();
        bool HasError();
        Error? FirstError();
}
