using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnologiesOnPlatformNET.Common.Models
{
    public class AspNetCore : WebTechnology
    {
        public bool SupportsMinimalAPI { get; set; }

        // Делегат
        public delegate void InfoHandler(string message);

        // Метод з делегатом
        public void ShowDetails(InfoHandler handler)
        {
            handler?.Invoke($"ASP.NET Core - Minimal API: {SupportsMinimalAPI}");
        }
    }
}