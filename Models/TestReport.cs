namespace Variant14.Models;

// Итог ручных проверок: сколько прошло из скольких.
public sealed record TestReport(int Passed, int Total)
{
    public bool AllPassed => Passed == Total;
}
