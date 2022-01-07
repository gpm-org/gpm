using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace gpm.WinUI.Services;

public class AppSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;

    public AppSink(IFormatProvider? formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);
    }
}

public static class MySinkExtensions
{
    public static LoggerConfiguration AppSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider? formatProvider = null) => loggerConfiguration.Sink(new AppSink(formatProvider));
}

