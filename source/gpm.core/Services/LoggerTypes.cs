namespace gpm.core.Services
{
    public enum Logtype
    {
        Normal,
        Error,
        Important,
        Success,
        Warning
    }

    public enum SystemLogFlag
    {
        SLF_Default,
        SLF_Error,
        SLF_Warning,
        SLF_Info,
        SLF_Interpretable
    }

    public class LogStringEventArgs
    {
        #region Constructors

        public LogStringEventArgs(string message, Logtype logtype)
        {
            Message = message;
            Logtype = logtype;
        }

        #endregion Constructors

        #region Properties

        public Logtype Logtype { get; private set; }
        public string Message { get; private set; }

        #endregion Properties
    }
}
