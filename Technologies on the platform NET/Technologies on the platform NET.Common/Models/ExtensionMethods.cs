using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnologiesOnPlatformNET.Common.Models
{
    public static class ExtensionMethods
    {
        // метод розширення
        public static string ToShortString(this DotNetTechnology tech)
        {
            return $"{tech.Name} v{tech.Version}";
        }
    }
}