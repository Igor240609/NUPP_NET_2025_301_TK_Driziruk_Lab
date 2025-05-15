using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnologiesOnPlatformNET.Common.Models
{
    public class DesktopTechnology : DotNetTechnology
    {
        public string GUIFramework { get; set; }
        public bool CrossPlatform { get; set; }
    }
}
