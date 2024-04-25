namespace ConcertApp.Ui.Tests.BoilerPlate;

internal sealed class NUnitLoggerProvider : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider _scopeProvider = new();
    private readonly Func<ITestOutputHelper> _testOutputHelper;

    public NUnitLoggerProvider(Func<ITestOutputHelper> testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new NUnitLogger(_testOutputHelper, _scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}