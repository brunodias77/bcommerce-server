namespace Bcommerce.Domain.Validations;

public abstract class Validator
{
    private readonly IValidationHandler _handler;

    protected Validator(IValidationHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// Executa as regras de validação concretas implementadas pelas subclasses.
    /// </summary>
    public abstract void Validate();

    /// <summary>
    /// Fornece acesso ao manipulador de validação para subclasses.
    /// </summary>
    protected IValidationHandler ValidationHandler => _handler;
}