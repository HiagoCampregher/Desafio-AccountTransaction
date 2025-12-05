using PagueVeloz.Application.DTOs;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Domain.Exceptions;
using PagueVeloz.Domain.Interfaces;
using Polly;
using Polly.Retry;
using InvalidOperationException = PagueVeloz.Domain.Exceptions.InvalidOperationException;

namespace PagueVeloz.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AsyncRetryPolicy _retryPolicy;

    public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;

        _retryPolicy = Policy.Handle<Exception>(ex => ex.Message.Contains("Concorrência entre requisições - Tentativa de retry"))
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(20 * Math.Pow(2, retryAttempt)));
    }

    public async Task<CreateTransactionResponse> ProcessTransactionAsync(CreateTransactionRequest request, CancellationToken ct)
    {
        if (await _transactionRepository.ExistsByReferenceIdAsync(request.ReferenceId, ct))
        {
            throw new InvalidOperationException($"Transação duplicada detectada: {request.ReferenceId}");
        }

        var opType = Enum.Parse<OperationTypeTransaction>(request.Operation, true);
        var transaction = new Transaction(
            Guid.NewGuid(),
            opType,
            request.Amount,
            request.Currency,
            request.ReferenceId,
            request.Metadata
        );

        Account? account = null;

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                account = await _accountRepository.GetByAccountNumberAsync(request.AccountId, ct)
                          ?? throw new InvalidOperationException("Conta não encontrada.");

                //transaction.SetAccountId(account.Id);

                switch (transaction.Operation)
                {
                    case OperationTypeTransaction.Credit:
                        account.Credit(transaction.Amount);
                        break;

                    case OperationTypeTransaction.Debit:
                        account.Debit(transaction.Amount);
                        break;

                    case OperationTypeTransaction.Reserve:
                        account.Reserve(transaction.Amount);
                        break;

                    case OperationTypeTransaction.Capture:
                        account.Capture(transaction.Amount);
                        break;

                    case OperationTypeTransaction.Reversal:
                        account.Credit(transaction.Amount);
                        break;

                    case OperationTypeTransaction.Transfer:
                        await ProcessTransferAsync(account, request, ct);
                        break;

                    default:
                        throw new NotImplementedException($"Operação {transaction.Operation} não suportada.");
                }

                transaction.SetStatusSucess();

                await _transactionRepository.AddAsync(transaction, ct);
                _accountRepository.Update(account);

                await _unitOfWork.CommitAsync(ct); // Pode lançar ConcurrencyException
            });
        }
        catch (DomainException ex)
        {
            transaction.SetStatusFailed(ex.Message);
            await SaveFailedTransactionAsync(transaction, ct);
        }
        catch (Exception ex)
        {
            transaction.SetStatusFailed("Erro interno no processamento.");
            await SaveFailedTransactionAsync(transaction, ct);
        }

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

    private async Task ProcessTransferAsync(Account sourceAccount, CreateTransactionRequest request, CancellationToken ct)
    {
        if (request.Metadata == null || !request.Metadata.TryGetValue("target_account_id", out var targetAccountIdObj))
        {
            throw new InvalidOperationException("Conta de destino não informada nos metadados para transferência.");
        }

        string targetAccountId = targetAccountIdObj.ToString()!;

        if (sourceAccount.AccountNumber == targetAccountId)
            throw new InvalidOperationException("Não é possível transferir para a mesma conta.");

        var targetAccount = await _accountRepository.GetByAccountNumberAsync(targetAccountId, ct)
                            ?? throw new InvalidOperationException("Conta de destino não encontrada.");

        sourceAccount.Debit(request.Amount);
        targetAccount.Credit(request.Amount);

        _accountRepository.Update(targetAccount);
    }

    private async Task SaveFailedTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        try
        {
            await _transactionRepository.AddAsync(transaction, ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch (Exception saveEx)
        {
        }
    }
}