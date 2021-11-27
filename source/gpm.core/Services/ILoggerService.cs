using System;

namespace gpm.core.Services
{
    public interface ILoggerService
    {
        public void Log(string msg, Logtype type = Logtype.Debug);

        public void Information(string s);
        public void Success(string msg);
        public void Warning(string s);
        public void Error(string message);
        public void Error(Exception exception);
        void Debug(string message);
        void Trace(string message);
        void Critical(string message);
    }
}
