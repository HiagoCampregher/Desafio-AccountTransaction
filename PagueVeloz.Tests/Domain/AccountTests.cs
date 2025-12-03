using FluentAssertions;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Exceptions;

namespace PagueVeloz.Tests.Domain;

public class AccountTests
{
    [Fact(DisplayName = "Deve realizar débito quando saldo + limite for suficiente")]
    public void Debit_WithSufficientFunds_ShouldUpdateBalance()
    {
        // Arrange (Cenário)
        var account = new Account("CLI-001", "ACC-TEST", 0); // Limite 0
        account.Credit(100); // Saldo 100

        // Act (Ação)
        account.Debit(40);

        // Assert (Validação)
        account.Balance.Should().Be(60);
    }

    [Fact(DisplayName = "Deve lançar erro ao tentar debitar valor maior que Saldo + Limite")]
    public void Debit_WithInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        var limit = 100;
        var account = new Account("CLI-001", "ACC-TEST", limit);
        // Saldo 0 + Limite 100 = Poder de compra 100

        // Act
        // Tentamos debitar 150 (50 acima do limite)
        Action action = () => account.Debit(150);

        // Assert
        action.Should().Throw<DomainException>() // Ou InsufficientFundsException se você criou
            .WithMessage("*insuficiente*"); // Verifica se a mensagem contém "insuficiente"
    }

    [Fact(DisplayName = "Não deve aceitar valor negativo no Crédito")]
    public void Credit_NegativeAmount_ShouldThrowException()
    {
        var account = new Account("CLI-001", "ACC-TEST", 0);
        Action action = () => account.Credit(-50);
        action.Should().Throw<DomainException>();
    }
}