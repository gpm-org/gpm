using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace gpm.Core.Util
{
    /// <summary>
    /// A scoped stopwatch that will log the elapsed time automatically when exiting scope.
    /// </summary>
    public sealed class ScopedStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _caller;

        public ScopedStopwatch([CallerMemberName] string name = "")
        {
            _caller = name;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"[{_caller}] took {elapsed.ToString()} ms.");
        }
    }
}
