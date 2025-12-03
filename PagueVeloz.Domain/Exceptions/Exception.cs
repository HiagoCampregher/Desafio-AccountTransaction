namespace PagueVeloz.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class InsufficientFundsException() : DomainException("Saldo insuficiente para realizar esta operação.");

public class AccountBlockedException() : DomainException("Conta bloqueada ou inativa.");

public class InvalidOperationException(string message) : DomainException(message);