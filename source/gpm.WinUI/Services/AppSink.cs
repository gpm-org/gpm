using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace gpmWinui.Services
{
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
                  IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new AppSink(formatProvider));
        }
    }
}
