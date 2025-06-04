namespace Bcommerce.Domain.Validations;

//// <summary>
/// Interface para tratamento de validações no domínio.
/// Permite acumular erros, validar operações e acessar os erros ocorridos.
/// </summary>
public interface IValidationHandler
{
    /// <summary>
    /// Adiciona um erro ao handler de validações.
    /// </summary>
    IValidationHandler Append(Error error);

    /// <summary>
    /// Adiciona todos os erros de outro handler a este.
    /// </summary>
    IValidationHandler Append(IValidationHandler handler);

    /// <summary>
    /// Executa uma validação e retorna o resultado.
    /// </summary>
    T Validate<T>(IValidation<T> validation);

    /// <summary>
    /// Retorna a lista de erros acumulados.
    /// </summary>
    IReadOnlyList<Error> GetErrors();

    /// <summary>
    /// Verifica se há algum erro registrado no handler.
    /// </summary>
    bool HasError() => GetErrors() is { Count: > 0 };

    /// <summary>
    /// Retorna o primeiro erro da lista, se houver.
    /// </summary>
    Error? FirstError() => GetErrors()?.Count > 0 ? GetErrors()[0] : null;
}

/// <summary>
/// Interface funcional representando uma operação de validação.
/// </summary>
/// <typeparam name="T">Tipo do valor de retorno da validação.</typeparam>
public interface IValidation<T>
{
    T Validate();
}
