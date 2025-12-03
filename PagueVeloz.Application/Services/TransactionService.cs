using PagueVeloz.Application.DTOs;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Domain.Exceptions;
using PagueVeloz.Domain.Interfaces;
using Polly;
using Polly.Retry;
using InvalidOperationException = PagueVeloz.Domain.Exceptions.InvalidOperationException;

namespace PagueVeloz.Application.Services;

public interface ITransactionService
{
    Task<CreateTransactionResponse> ProcessTransactionAsync(CreateTransactionRequest request, CancellationToken ct);
}

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AsyncRetryPolicy _retryPolicy;

    public TransactionService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;

        // Configuração do Retry conforme requisito de "Resiliência" [cite: 66, 129]
        // Se der erro de Concorrência (Lock Otimista), tenta 3 vezes esperando 
        // exponencialmente (ex: 20ms, 40ms, 80ms)
        _retryPolicy = Policy
            .Handle<Exception>(ex => ex.Message.Contains("Conflito de concorrência"))
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(20 * Math.Pow(2, retryAttempt)));
    }

    public async Task<CreateTransactionResponse> ProcessTransactionAsync(CreateTransactionRequest request, CancellationToken ct)
    {
        // 1. Idempotência: Verifica se já processamos essa transação 
        if (await _transactionRepository.ExistsByReferenceIdAsync(request.ReferenceId, ct))
        {
            throw new InvalidOperationException($"Transação duplicada detectada: {request.ReferenceId}");
        }

        // Criamos a entidade de transação (Auditoria)
        var transaction = new Transaction(
            Guid.Empty, // Será preenchido ao carregar a conta
            Enum.Parse<OperationTypeTransaction>(request.Operation, true),
            request.Amount,
            request.Currency,
            request.ReferenceId,
            request.Metadata
        );

        Account? account = null;

        try
        {
            // O código dentro do ExecuteAsync será retentado se der conflito de concorrência
            await _retryPolicy.ExecuteAsync(async () =>
            {
                // 2. Carrega a conta
                // Nota: Buscamos pelo AccountNumber (string) que vem no JSON
                account = await _accountRepository.GetByAccountNumberAsync(request.AccountId, ct)
                          ?? throw new InvalidOperationException("Conta não encontrada.");

                // Atualizamos o ID da conta na transação para vincular corretamente
                // (Aqui teríamos que ajustar a entidade Transaction para permitir setar AccountId ou passá-lo no construtor depois de buscar a conta)
                // Para simplificar, assuma que a lógica de negócio roda na conta:

                switch (transaction.Operation)
                {
                    case OperationTypeTransaction.Credit:
                        account.Credit(transaction.Amount);
                        break;
                    case OperationTypeTransaction.Debit:
                        account.Debit(transaction.Amount);
                        break;
                    // Implementar outros cases (Reserve, Capture, etc.)
                    default:
                        throw new NotImplementedException($"Operação {transaction.Operation} não suportada ainda.");
                }

                transaction.SetStatusSucess();

                // 3. Persistência Atômica [cite: 63]
                // Se salvar Account e Transaction falhar por concorrência, o Polly pega aqui
                await _transactionRepository.AddAsync(transaction, ct);
                _accountRepository.Update(account);
                await _unitOfWork.CommitAsync(ct);
            });
        }
        catch (DomainException ex) // Erros de Negócio (Saldo insuficiente, bloqueio)
        {
            // Requisito: Retornar falha mas registrar a tentativa? 
            // O PDF diz: Status "failed" na saída[cite: 72].
            // Então não devemos explodir a Exception para a API, e sim retornar o DTO com erro.
            transaction.SetStatusFailed(ex.Message);

            // Opcional: Salvar a transação falhada no banco para histórico
            // await _transactionRepository.AddAsync(transaction, ct);
            // await _unitOfWork.CommitAsync(ct);
        }
        catch (Exception ex) // Erros técnicos
        {
            // Logar erro crítico
            throw;
        }

        // 4. Montar Resposta 
        return new CreateTransactionResponse(
            transaction.Id.ToString(),
            transaction.Status.ToString().ToLower(),
            account?.Balance ?? 0,
            account?.ReservedBalance ?? 0,
            account?.AvailableBalance ?? 0,
            transaction.Timestamp,
            transaction.ErrorMessage
        );
    }
}