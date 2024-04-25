using System.Text;

namespace ConcertApp.Ui.Tests.BoilerPlate;

internal class NUnitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider;
    private readonly Func<ITestOutputHelper> _testOutputHelper;

    public NUnitLogger(Func<ITestOutputHelper> testOutputHelper, LoggerExternalScopeProvider scopeProvider,
        string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
    }

    public NUnitLogger(Func<ITestOutputHelper> testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _scopeProvider = new LoggerExternalScopeProvider();
        _categoryName = "INFO";
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return _scopeProvider.Push(state);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var sb = new StringBuilder();
        sb.Append(GetLogLevelString(logLevel))
            .Append(" [").Append(_categoryName).Append("] ")
            .Append(formatter(state, exception));

        sb.Append('\n').Append(exception);

        // Append scopes
        _scopeProvider.ForEachScope((scope, state) =>
        {
            state.Append("\n => ");
            state.Append(scope);
        }, sb);

        _testOutputHelper?.Invoke()?.WriteLine(sb.ToString());
    }

    public static ILogger CreateLogger(Func<ITestOutputHelper> testOutputHelper)
    {
        return new NUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");
    }

    public static ILogger<T> CreateLogger<T>(Func<ITestOutputHelper> testOutputHelper)
    {
        return new NUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}