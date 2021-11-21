using System;

namespace gpm.core.Exceptions
{
    public class ArgumentNullOrEmptyException : ArgumentNullException
    {
        public static void ThrowIfNullOrEmpty(object? argument)
            => ThrowIfNull(argument: argument);
        public static void ThrowIfNullOrEmpty(object? argument, string? paramName)
            => ThrowIfNull(argument: argument, paramName: paramName);
    }
}
