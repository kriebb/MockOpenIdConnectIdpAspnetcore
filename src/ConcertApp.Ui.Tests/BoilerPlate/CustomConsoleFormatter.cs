using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ConcertApp.Ui.Tests.BoilerPlate;

public class CustomConsoleFormatter() : ConsoleFormatter("custom")
{
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var builder = new StringBuilder();
        builder.Append($"{timestamp} [{logEntry.LogLevel}] {logEntry.Category} ");

        scopeProvider?.ForEachScope((state, stringBuilder) =>
        {
            stringBuilder.Append($" => {state}");
        }, builder);

        builder.Append($" : {logEntry.Formatter(logEntry.State, logEntry.Exception)}");

        textWriter.WriteLine(builder.ToString());
    }
}