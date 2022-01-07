using System;
using System.Runtime.CompilerServices;

namespace gpm.Tests
{
    public static class Common
    {
        public static void LogBeginOfTest([CallerMemberName] string methodName = "")
            => Console.WriteLine("\n=== {0} ===", methodName);
    }
}
