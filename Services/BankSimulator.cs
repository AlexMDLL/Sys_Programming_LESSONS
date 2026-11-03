using BankRaceCondition.Models;
namespace BankRaceCondition.Services;
public sealed class BankSimulator
{
    public async Task RunUnsafeAsync(
        BankAccount account,
        int workers,
        int operationsPerWorker,
        decimal amount)
    {
        Task[] tasks = Enumerable
            .Range(0, workers)
            .Select(_ => Task.Run(() => { for (int i = 0; i < operationsPerWorker; i++) { account.DepositUnsafe(amount); } }))
            .ToArray();

        // ждём, пока все задачи сдохнут вместе с операцией
        await Task.WhenAll(tasks);
    }

    public async Task RunSafeAsync(
        BankAccount account,
        int workers,
        int operationsPerWorker,
        decimal amount)
    {
        Task[] tasks = Enumerable
            .Range(0, workers)
            .Select(_ => Task.Run(() => { for (int i = 0; i < operationsPerWorker; i++) { account.Deposit(amount); } }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    public async Task<int> RunWithdrawalsAsync(
        BankAccount account,
        int workers,
        int operationsPerWorker,
        decimal amount)
    {
        int successfulOperations = 0;

        // баланс проверить и списать одним действием, поэтому lock,
        // а счётчику успешных списаний хватит и одного Interlocked.Incrementа, ну чтоб не втыкал а то че он
        Task[] tasks = Enumerable
            .Range(0, workers)
            .Select(_ => Task.Run(() => { for (int i = 0; i < operationsPerWorker; i++) { if (account.Withdraw(amount)) { Interlocked.Increment(ref successfulOperations); } } }))
            .ToArray();

        await Task.WhenAll(tasks);
        return successfulOperations;
    }
}
