using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnologiesOnPlatformNET.Common.Models
{
    // Наслідування
    public class WebTechnology : DotNetTechnology
    {
        public string FrontendFramework { get; set; }
        public bool IsCloudReady { get; set; }

        // Подія
        public event Action<string> OnDeployed;

        // Метод
        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine($"Frontend: {FrontendFramework}, Cloud-ready: {IsCloudReady}");
        }

        public void Deploy()
        {
            OnDeployed?.Invoke($"{Name} deployed.");
        }
    }
}
