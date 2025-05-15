using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnologiesOnPlatformNET.Common.Models
{
    public class DotNetTechnology
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        // Статичне поле
        public static string Platform = ".NET";

        // Статичний конструктор
        static DotNetTechnology()
        {
            Console.WriteLine("DotNetTechnology class loaded");
        }

        // Конструктор
        public DotNetTechnology()
        {
            Id = Guid.NewGuid();
        }

        // Метод
        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Technology: {Name}, Version: {Version}");
        }
    }
}