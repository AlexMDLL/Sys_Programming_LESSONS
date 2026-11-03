namespace BankRaceCondition.Models;

public sealed class BankAccount
{
    // один и тот же объект для всех критов
    private readonly object _sync = new();
    private decimal _balance;

    public decimal Balance { get { lock (_sync) { return _balance; } } } // читаем под lock, чтобы не словить ратник (половину)

    public void DepositUnsafe(decimal amount)
    {
        if (amount <= 0) { throw new ArgumentOutOfRangeException(nameof(amount)); }
        // считали баланс, но пока считали сосед, сменил пароль от вайфая, поток мог его уже поменять
        decimal current = _balance;
        // искусственно растягиваем самогонки, чтобы её было видно
        Thread.SpinWait(1000);
        _balance = current + amount;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) { throw new ArgumentOutOfRangeException(nameof(amount)); }

        lock (_sync) {  _balance += amount; }
    }
}
