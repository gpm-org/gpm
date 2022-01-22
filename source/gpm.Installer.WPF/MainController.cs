using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpm.Installer.WPF;

internal class MainController
{
    public bool Restart { get; internal set; }
    public string RestartName { get; internal set; } = "";

    public string BaseDir { get; internal set; } = "";


}
