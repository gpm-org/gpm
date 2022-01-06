using gpm.Core.Models;

namespace gpm.Core.Util.Builders
{
    public class BuilderContext
    {
        public BuilderContext(Package package)
        {
            Package = package;
        }

        public Package Package { get; }
    }
}
