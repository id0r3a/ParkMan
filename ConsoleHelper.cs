using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NemoPark
{
    using Spectre.Console;

    public static class ConsoleHelper
    {
        public static void Pause()
        {
            AnsiConsole.MarkupLine("\n[blue]Press any key to continue...[/]");
            Console.ReadKey();
            Console.Clear();
        }
    }

}
