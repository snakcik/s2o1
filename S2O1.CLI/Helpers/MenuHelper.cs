using System;
using System.Collections.Generic;

namespace S2O1.CLI.Helpers
{
    public static class MenuHelper
    {
        public static int ShowMenu(string title, List<string> options)
        {
            Console.WriteLine(title);
            int currentSelection = 0;

            ConsoleKey key;
            do
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == currentSelection)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentSelection > 0) currentSelection--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentSelection < options.Count - 1) currentSelection++;
                        break;
                }

                // Rewrite menu (simple clear lines approach or clear screen)
                // Clearing screen might be too flashy.
                // Let's just clear console
                // For simplicity, we can just Console.Clear() but that clears the Logo.
                // Better: SetCursorPosition.
                
                Console.SetCursorPosition(0, Console.CursorTop - options.Count);

            } while (key != ConsoleKey.Enter);
            
            // Move cursor down after selection
            Console.SetCursorPosition(0, Console.CursorTop + options.Count);
            return currentSelection;
        }
    }
}
