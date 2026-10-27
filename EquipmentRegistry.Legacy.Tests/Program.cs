using EquipmentRegistry.Legacy;

var repository = new EquipmentRepository();
var service = new InventoryService(repository);

var failures = new List<string>();
var passed = 0;

void Check(string name, bool condition)
{
    Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    Console.ResetColor();

    if (condition) passed++;
    else failures.Add(name);
}

// 1. Дефект: поиск был регистрозависимым.
Check("поиск без учёта регистра находит Laptop Dell",
    service.SearchByName("laptop").Any(x => x.Id == 101));

// 2. Дефект: фильтр использовал != и возвращал записи с чужими статусами.
Check("фильтр InRepair возвращает только записи InRepair",
    service.GetByStatus(EquipmentStatus.InRepair).All(x => x.Status == EquipmentStatus.InRepair));

// 3. Граничный сценарий: оборудование 2010 года полностью самортизировано.
var oldEquipment = new Equipment
{
    Id = 999, Name = "Old test device", PurchasePrice = 100m,
    PurchaseDate = new DateOnly(2010, 1, 1), Status = EquipmentStatus.InUse
};
var value = service.CalculateBookValue(oldEquipment, new DateOnly(2026, 10, 27));
Check("балансовая стоимость не отрицательна", value == 0m);

var totalChecks = passed + failures.Count;

Console.WriteLine();

if (failures.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Итого: 0 проверок не пройдено из {totalChecks}.");
    Console.WriteLine("Все проверки пройдены.");
    Console.ResetColor();
}
else if (passed > 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Итого: {failures.Count} проверок не пройдено из {totalChecks}. Есть проблемы.");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Итого: {failures.Count} проверок не пройдено из {totalChecks}.");
    Console.ResetColor();
}
