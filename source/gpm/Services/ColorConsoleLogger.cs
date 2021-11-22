//https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace gpm.Services
{
    public sealed class ColorConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new();
        private readonly IDisposable _onChangeToken;
        private ColorConsoleLoggerConfiguration _currentConfig;

        public ColorConsoleLoggerProvider(
            IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, _currentConfig));

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }


    public static class ColorConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddColorConsoleLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <ColorConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddColorConsoleLogger(
            this ILoggingBuilder builder,
            Action<ColorConsoleLoggerConfiguration> configure)
        {
            builder.AddColorConsoleLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }


    public class ColorConsoleLoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }


    public class ColorConsoleLogger : ILogger
    {
        private readonly ColorConsoleLoggerConfiguration _config;
        private readonly string _name;

        public ColorConsoleLogger(
            string name,
            ColorConsoleLoggerConfiguration config)
        {
            (_name, _config) = (name, config);
        }

#pragma warning disable CS8603 // Possible null reference return.
        public IDisposable BeginScope<TState>(TState state) => default;
#pragma warning restore CS8603 // Possible null reference return.

        public bool IsEnabled(LogLevel logLevel) =>
            _config.LogLevels.ContainsKey(logLevel);


        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logLevelStr = "";
            switch (logLevel)
            {
                case LogLevel.Trace:
                    logLevelStr = "Trace";
                    break;
                case LogLevel.Debug:
                    logLevelStr = "Debug";
                    break;


                case LogLevel.Information:
                    logLevelStr = "Success";
                    break;
                case LogLevel.Warning:
                    logLevelStr = "Information";
                    break;
                case LogLevel.Error:
                    logLevelStr = "Warning";
                    break;
                case LogLevel.Critical:
                    logLevelStr = "Error";
                    break;


                case LogLevel.None:
                    logLevelStr = "None";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }

            if (_config.EventId == 0 || _config.EventId == eventId.Id)
            {
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = _config.LogLevels[logLevel];


                //if (exception is not null)
                {
                    Console.WriteLine($"[{eventId.Id,2}: {logLevelStr,-12}] - {formatter(state, exception)}");
                }


                //Console.WriteLine($"     {_name} - {formatter(state, exception)}");
                Console.ForegroundColor = originalColor;
            }
        }
    }
}
