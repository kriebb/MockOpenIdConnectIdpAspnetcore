namespace ConcertApp.Ui.Tests.BoilerPlate;

internal sealed class NUnitLogger<T> : NUnitLogger, ILogger<T>
{
    public NUnitLogger(Func<ITestOutputHelper> testOutputHelper, LoggerExternalScopeProvider scopeProvider)
        : base(testOutputHelper, scopeProvider, typeof(T).FullName ?? "Unknown type")
    {
    }

    public NUnitLogger(Func<ITestOutputHelper> testOutputHelper) : this(testOutputHelper, new LoggerExternalScopeProvider())
    {
    }
   
}