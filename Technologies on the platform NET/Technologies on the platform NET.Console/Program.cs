using TechnologiesOnPlatformNET.Common.Models;
using TechnologiesOnPlatformNET.Common.Services;

class Program
{
    static void Main(string[] args)
    {
        var service = new CrudService<DotNetTechnology>();

        var asp = new AspNetCore { Name = "ASP.NET Core", Version = "8.0", FrontendFramework = "Blazor", IsCloudReady = true, SupportsMinimalAPI = true };
        var win = new WinForms { Name = "WinForms", Version = "4.8", GUIFramework = "GDI+", CrossPlatform = false, HasDesignerSupport = true };

        service.Create(asp);
        service.Create(win);

        Console.WriteLine("Added Technologies:");
        foreach (var tech in service.ReadAll())
        {
            Console.WriteLine(tech.ToShortString());
        }

        // Серіалізація
        service.Save("data.json");

        Console.WriteLine("\nSaved to file. Now clearing service and loading again...\n");

        var newService = new CrudService<DotNetTechnology>();
        newService.Load("data.json");

        Console.WriteLine("Loaded Technologies:");
        foreach (var tech in newService.ReadAll())
        {
            Console.WriteLine(tech.ToShortString());
        }
    }
}