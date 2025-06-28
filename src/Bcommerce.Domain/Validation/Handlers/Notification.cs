using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcommerce.Domain.Exceptions;

namespace Bcommerce.Domain.Validation.Handlers;


// <summary>
/// Implementação do IValidationHandler que acumula erros em uma lista.
/// Segue o Notification Pattern, separando a validação do fluxo de controle.
/// </summary>
public class Notification : IValidationHandler
{
    private readonly List<Error> _errors;

    private Notification(List<Error> errors) => _errors = errors;
    
    /// <summary>
    /// Cria uma nova instância vazia de Notification.
    /// </summary>
    public static Notification Create() => new(new List<Error>());
    
    /// <summary>
    /// Anexa um erro à notificação.
    /// </summary>
    public Notification Append(Error error)
    {
        _errors.Add(error);
        return this;
    }
    
    // Implementação explícita para satisfazer a interface, mantendo a API da classe limpa.
    IValidationHandler IValidationHandler.Append(Error error)
    {
        return this.Append(error);
    }
    
    IValidationHandler IValidationHandler.Append(IValidationHandler handler)
    {
        _errors.AddRange(handler.GetErrors());
        return this;
    }

    /// <summary>
    /// Executa uma validação e captura quaisquer exceções de domínio, adicionando-as à lista de erros.
    /// </summary>
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

    /// <summary>
    /// Obtém a lista de todos os erros acumulados.
    /// </summary>
    public IReadOnlyList<Error> GetErrors() => _errors;
    
    /// <summary>
    /// Verifica se algum erro foi adicionado à notificação.
    /// </summary>
    public bool HasError() => _errors.Any();

    /// <summary>
    /// Retorna o primeiro erro da lista, ou nulo se não houver erros.
    /// </summary>
    public Error? FirstError() => _errors.FirstOrDefault();
}

// /// <summary>
// /// Implementação do IValidationHandler que acumula erros em uma lista.
// /// Segue o Notification Pattern, separando a validação do fluxo de controle.
// /// </summary>
// public class Notification : IValidationHandler
// {
//     private readonly List<Error> _errors;
//
//     private Notification(List<Error> errors) => _errors = errors;
//     
//     /// <summary>
//     /// Cria uma nova instância vazia de Notification.
//     /// </summary>
//     public static Notification Create() => new(new List<Error>());
//     
//     /// <summary>
//     /// Anexa um erro à notificação. Este método público retorna a própria instância de Notification
//     /// para permitir um encadeamento fluente sem a necessidade de conversão (cast).
//     /// </summary>
//     /// <param name="error">O erro a ser adicionado.</param>
//     /// <returns>A própria instância de <see cref="Notification"/>.</returns>
//     public Notification Append(Error error)
//     {
//         _errors.Add(error);
//         return this;
//     }
//
//     // CORREÇÃO: Esta é a implementação explícita do método exigido pela interface IValidationHandler.
//     // Ela "esconde" o método da interface e redireciona a chamada para o método público acima.
//     // Isso resolve o conflito de assinaturas e satisfaz o contrato da interface.
//     IValidationHandler IValidationHandler.Append(Error error)
//     {
//         return this.Append(error);
//     }
//
//     // Também aplicando o mesmo padrão para o outro método Append para consistência.
//     IValidationHandler IValidationHandler.Append(IValidationHandler handler)
//     {
//         _errors.AddRange(handler.GetErrors());
//         return this;
//     }
//
//     /// <summary>
//     /// Executa uma validação e captura quaisquer exceções de domínio, adicionando-as à lista de erros.
//     /// </summary>
//     public T? Validate<T>(IValidation<T> validation)
//     {
//         try
//         {
//             return validation.Validate();
//         }
//         catch (DomainException ex)
//         {
//             _errors.AddRange(ex.Errors);
//         }
//         catch (Exception ex)
//         {
//             _errors.Add(new Error(ex.Message));
//         }
//
//         return default;
//     }
//
//     /// <summary>
//     /// Obtém a lista de todos os erros acumulados.
//     /// </summary>
//     public IReadOnlyList<Error> GetErrors() => _errors;
//     
//     /// <summary>
//     /// Verifica se algum erro foi adicionado à notificação.
//     /// </summary>
//     public bool HasError() => _errors.Any(); 
//     
//     public Error? FirstError() => _errors.FirstOrDefault();
// }