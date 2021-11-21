using System;

namespace gpm.core.Extensions
{
    public class ArgumentNullOrEmptyException : ArgumentNullException
    {
    //     public ArgumentNullOrEmptyException(string paramName, string message) : base(paramName: paramName, message: message)
    //     {
    //     }

        public static void ThrowIfNullOrEmpty(object? argument)
            => ArgumentNullException.ThrowIfNull(argument: argument);
        public static void ThrowIfNullOrEmpty(object? argument, string? paramName)
            => ArgumentNullException.ThrowIfNull(argument: argument, paramName: paramName);
    }


}
