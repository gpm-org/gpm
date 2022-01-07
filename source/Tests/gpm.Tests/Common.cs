using System;
using System.Runtime.CompilerServices;

namespace gpm_tests
{
    public static class Common
    {
        public static void LogBeginOfTest([CallerMemberName] string methodName = "")
            => Console.WriteLine("\n=== {0} ===", methodName);
    }
}
