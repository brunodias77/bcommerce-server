namespace Bcommerce.Domain.Validation;

public interface IValidation<T>
{ 
    T Validate();
}
