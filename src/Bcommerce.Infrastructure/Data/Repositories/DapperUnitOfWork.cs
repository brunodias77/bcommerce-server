using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class DapperUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private NpgsqlTransaction? _transaction;
    private bool _disposed;

    public DapperUnitOfWork(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        _connection = new NpgsqlConnection(connectionString);
    }

    // ✅ Corrigido para interface genérica
    public IDbConnection Connection => _connection;

    public IDbTransaction Transaction =>
        _transaction ?? throw new InvalidOperationException("Transação não foi iniciada. Chame Begin().");
    
    // ✅ [NOVA PROPRIEDADE]
    // Informa se há uma transação ativa
    public bool HasActiveTransaction => _transaction != null; // ← Adicionado

    public async Task Begin()
    {
        // ✅ [MELHORIA]
        // Evita iniciar uma nova transação se já houver uma ativa
        if (_transaction != null) return;
        
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        _transaction = await _connection.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        if (_transaction is null)
            throw new InvalidOperationException("Não é possível fazer commit sem uma transação ativa.");

        await _transaction.CommitAsync();
        await CleanupAsync();
    }

    public async Task Rollback()
    {
        if (_transaction is null)
            throw new InvalidOperationException("Não é possível fazer rollback sem uma transação ativa.");

        await _transaction.RollbackAsync();
        await CleanupAsync();
    }

    private async Task CleanupAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }

        await _connection.DisposeAsync();
        _disposed = true;
    }
}








// using System;
// using System.Data;
// using System.Threading.Tasks;
// using Npgsql; // Necessário para NpgsqlConnection e NpgsqlTransaction
//
// namespace bcommerce_server.Infra.Repositories // Mantendo o namespace da interface
// {
//     public class NpgsqlUnitOfWork : IUnitOfWork
//     {
//         private readonly NpgsqlConnection _connection;
//         private NpgsqlTransaction? _transaction;
//         private bool _disposed = false;
//         private readonly string _connectionString; // Opcional, se a conexão não for passada diretamente
//
//         /// <summary>
//         /// Cria uma nova instância de NpgsqlUnitOfWork.
//         /// </summary>
//         /// <param name="connectionString">A string de conexão para o banco de dados PostgreSQL.</param>
//         public NpgsqlUnitOfWork(string connectionString)
//         {
//             if (string.IsNullOrWhiteSpace(connectionString))
//                 throw new ArgumentNullException(nameof(connectionString));
//
//             _connectionString = connectionString;
//             // A conexão é criada aqui, mas aberta apenas quando necessário (em Begin ou por uso direto)
//             _connection = new NpgsqlConnection(_connectionString);
//         }
//
//         /// <summary>
//         /// Conexão atual com o banco de dados.
//         /// A conexão será aberta se estiver fechada quando uma transação for iniciada.
//         /// </summary>
//         public IDbConnection Connection => _connection;
//
//         /// <summary>
//         /// Transação atual em execução.
//         /// Lança uma exceção se nenhuma transação estiver ativa.
//         /// </summary>
//         public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("Nenhuma transação ativa. Chame Begin() primeiro.");
//
//         /// <summary>
//         /// Indica se existe uma transação ativa no momento.
//         /// </summary>
//         public bool HasActiveTransaction => _transaction != null;
//
//         /// <summary>
//         /// Inicia uma nova transação.
//         /// </summary>
//         public async Task Begin()
//         {
//             if (_disposed)
//                 throw new ObjectDisposedException(nameof(NpgsqlUnitOfWork));
//             if (HasActiveTransaction)
//                 throw new InvalidOperationException("Uma transação já está ativa.");
//
//             // Abre a conexão se estiver fechada ou quebrada
//             if (_connection.State != ConnectionState.Open)
//             {
//                 try
//                 {
//                     await _connection.OpenAsync();
//                 }
//                 catch (Exception ex)
//                 {
//                     // Poderia envolver a exceção em uma mais específica de UoW se desejado
//                     throw new InvalidOperationException("Falha ao abrir a conexão com o banco de dados.", ex);
//                 }
//             }
//             
//             _transaction = (NpgsqlTransaction)await _connection.BeginTransactionAsync();
//         }
//
//         /// <summary>
//         /// Confirma a transação atual.
//         /// </summary>
//         public async Task Commit()
//         {
//             if (_disposed)
//                 throw new ObjectDisposedException(nameof(NpgsqlUnitOfWork));
//             if (!HasActiveTransaction)
//                 throw new InvalidOperationException("Nenhuma transação ativa para comitar.");
//
//             try
//             {
//                 await _transaction!.CommitAsync();
//             }
//             catch
//             {
//                 // Em caso de falha no commit, a transação ainda pode estar ativa ou em estado indeterminado.
//                 // Reverter pode ser uma opção, mas o SGBD geralmente já a invalida.
//                 // Deixar o DisposeAsync lidar com o dispose da transação.
//                 throw; // Re-lança a exceção original
//             }
//             finally
//             {
//                 // Limpa a referência à transação após o commit (bem-sucedido ou não, se o estado for final)
//                 // Se CommitAsync lança, a transação pode não estar mais utilizável.
//                 // NpgsqlTransaction é disposed implicitamente em Commit/Rollback pelo driver.
//                 if (_transaction != null && (_transaction.IsCompleted || _transaction.Connection == null))
//                 {
//                      await _transaction.DisposeAsync(); // Garante que foi disposed
//                     _transaction = null;
//                 } else if (_transaction != null && _transaction.Connection != null && !_transaction.IsCompleted) {
//                     // Se o commit falhou mas a transação tecnicamente ainda está "ativa" (improvável para CommitAsync)
//                     // o DisposeAsync fará o rollback.
//                 }
//                 else // Caso já tenha sido disposed pelo CommitAsync
//                 {
//                     _transaction = null;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// Cancela a transação atual.
//         /// </summary>
//         public async Task Rollback()
//         {
//             if (_disposed)
//                 throw new ObjectDisposedException(nameof(NpgsqlUnitOfWork));
//             if (!HasActiveTransaction)
//                 throw new InvalidOperationException("Nenhuma transação ativa para reverter.");
//
//             try
//             {
//                 await _transaction!.RollbackAsync();
//             }
//             catch
//             {
//                 // Similar ao Commit, se o Rollback falhar, o estado da transação pode ser problemático.
//                 throw; // Re-lança a exceção original
//             }
//             finally
//             {
//                 // Limpa a referência à transação após o rollback
//                 // NpgsqlTransaction é disposed implicitamente em Commit/Rollback pelo driver.
//                 if (_transaction != null) // Se RollbackAsync lançou exceção antes de limpar
//                 {
//                     await _transaction.DisposeAsync(); // Garante que foi disposed
//                     _transaction = null;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// Libera os recursos utilizados pelo NpgsqlUnitOfWork.
//         /// Realiza rollback em transações ativas não finalizadas.
//         /// </summary>
//         public async ValueTask DisposeAsync()
//         {
//             if (_disposed)
//                 return;
//
//             // Realiza rollback se uma transação estiver ativa e não foi finalizada
//             if (HasActiveTransaction)
//             {
//                 try
//                 {
//                     await _transaction!.RollbackAsync();
//                 }
//                 catch (Exception ex)
//                 {
//                     // Logar a exceção aqui pode ser útil, pois o DisposeAsync não deve lançar exceções.
//                     Console.Error.WriteLine($"Erro durante o rollback automático no DisposeAsync: {ex.Message}");
//                 }
//             }
//
//             if (_transaction != null)
//             {
//                 await _transaction.DisposeAsync();
//                 _transaction = null;
//             }
//
//             if (_connection != null)
//             {
//                 // A conexão só é fechada se foi aberta. DisposeAsync do NpgsqlConnection cuida disso.
//                 await _connection.DisposeAsync();
//             }
//
//             _disposed = true;
//             GC.SuppressFinalize(this); // Se houvesse um finalizador (não necessário aqui)
//         }
//     }
// }