using System;
using Microsoft.Extensions.Logging;

namespace gpm.core.Services
{
    public class MicrosoftLoggerService : ILoggerService
    {
        private readonly ILogger<MicrosoftLoggerService> _logger;

        public MicrosoftLoggerService(ILogger<MicrosoftLoggerService> logger)
        {
            _logger = logger;
        }

        public void Log(string message, Logtype type = Logtype.Debug)
        {
            switch (type)
            {
                case Logtype.Trace:
                    _logger.LogTrace("{Message}", message);
                    break;
                case Logtype.Debug:
                    _logger.LogTrace("{Message}", message);
                    break;

                case Logtype.Information:
                    _logger.LogDebug("{Message}", message);
                    break;
                case Logtype.Success:
                    _logger.LogInformation("{Message}", message);
                    break;

                case Logtype.Warning:
                    _logger.LogWarning("{Message}", message);
                    break;
                case Logtype.Error:
                    _logger.LogError("{Message}", message);
                    break;
                case Logtype.Critical:
                    _logger.LogCritical("{Message}", message);
                    break;
                case Logtype.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Trace(string message) => Log(message, Logtype.Trace);

        public void Debug(string message) => Log(message, Logtype.Debug);


        public void Information(string message) => Log(message, Logtype.Information);

        public void Success(string message)  => Log(message, Logtype.Success);


        public void Warning(string message) => Log(message, Logtype.Warning);

        public void Error(string message)  => Log(message, Logtype.Error);
        public void Error(Exception exception)
        {
            var msg =
                $"========================\r\n" +
                $"{exception}" +
                $"\r\n========================";
            Error(msg);
        }

        public void Critical(string message)  => Log(message, Logtype.Critical);
    }
}
