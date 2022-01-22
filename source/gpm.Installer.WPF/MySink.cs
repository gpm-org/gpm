using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using DynamicData;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace gpm.Installer.WPF;

public class MySink : ILogEventSink
{
    //private readonly IFormatProvider? _formatProvider;

    private readonly SourceList<LogEvent> _logEvents = new();

    public MySink()
    {
        //_formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        //var message = logEvent.RenderMessage(_formatProvider);
        //Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);

        _logEvents.Add(logEvent);
    }

    public IObservable<IChangeSet<LogEvent>> Connect() => _logEvents.Connect();

}


public static class MySinkExtensions
{
    public static LoggerConfiguration MySink(
              this LoggerSinkConfiguration loggerConfiguration) => loggerConfiguration.Sink(Ioc.Default.GetRequiredService<MySink>());
}
