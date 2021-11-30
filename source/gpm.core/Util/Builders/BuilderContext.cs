using gpm.core.Models;

namespace gpm.core.Util.Builders
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
