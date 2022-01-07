using System;

namespace gpm.core.Exceptions
{
    public sealed class ArgumentNullOrEmptyException : ArgumentNullException
    {
        public ArgumentNullOrEmptyException(string? paramName, string? message) : base(paramName, message)
        {

        }

        public static void ThrowIfNullOrEmpty(object? argument)
            => ThrowIfNull(argument: argument);
        public static void ThrowIfNullOrEmpty(object? argument, string? paramName)
            => ThrowIfNull(argument: argument, paramName: paramName);
    }
}
