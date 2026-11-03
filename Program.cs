using BankRaceCondition.Models;
using BankRaceCondition.Services;

const int workers = 10;
const int operations = 1000;
const decimal amount = 10;

var account = new BankAccount();
var simulator = new BankSimulator();

decimal expected = workers * operations * amount;

Console.WriteLine("=== BANK RACE CONDITION ===");
Console.WriteLine($"Начальный баланс: {account.Balance}");
Console.WriteLine($"Количество задач: {workers}");
Console.WriteLine($"Операций на задачу: {operations}");
Console.WriteLine($"Сумма пополнения: {amount}");
Console.WriteLine($"Ожидаемый баланс: {expected}");

await simulator.RunUnsafeAsync(
    account,
    workers,
    operations,
    amount);

Console.WriteLine();
Console.WriteLine($"Фактический баланс: {account.Balance}");
Console.WriteLine($"Разница: {expected - account.Balance}");

Console.WriteLine();
Console.WriteLine("=== BANK LOCK ===");

var safeAccount = new BankAccount();

await simulator.RunSafeAsync(
    safeAccount,
    workers,
    operations,
    amount);

Console.WriteLine();
Console.WriteLine($"Фактический баланс: {safeAccount.Balance}");
Console.WriteLine($"Разница: {expected - safeAccount.Balance}");

