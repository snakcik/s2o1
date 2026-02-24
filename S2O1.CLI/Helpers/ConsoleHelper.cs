using System;

namespace S2O1.CLI.Helpers
{
    public static class ConsoleHelper
    {
        public static void PrintLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
 ██████╗ ███████╗ ██╗ ██████╗ 
 ╚════██╗██╔════╝███║██╔═══██╗
  █████╔╝███████╗╚██║██║   ██║
 ██╔═══╝ ╚════██║ ██║██║   ██║
 ███████╗███████║ ██║╚██████╔╝
 ╚══════╝╚══════╝ ╚═╝ ╚═════╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("2S1O - Warehouse Management System [v1.0.0]\n");
            Console.ResetColor();
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] {message}");
            Console.ResetColor();
        }

        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] {message}");
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}");
            Console.ResetColor();
        }

        public static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO] {message}");
            Console.ResetColor();
        }

        public static DateTime LastActivity { get; private set; } = DateTime.Now;

        public static void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }

        public static void StartInactivityMonitor()
        {
            // Removed for Docker interactivity
        }

        public static string ReadLine()
        {
            UpdateActivity();
            // We can't easily track keystrokes in ReadLine without re-implementing it.
            // So we rely on the user completing the input within the timeout.
            // Or we could implement a Reader with timeout, but standard ReadLine is safer for compatibility.
            // Risk: If user types for > 1 min without Enter, they get kicked. Acceptable for CLI.
            
            // To allow "activity" to reset the timer, we ultimately need a non-blocking read.
            // But for now, let's just reset on entry and exit.
            string input = Console.ReadLine();
            UpdateActivity();
            return input;
        }

        public static string ReadPassword()
        {
            UpdateActivity();
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                UpdateActivity(); // Update on every keypress
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            UpdateActivity();
            return password;
        }
        public static async Task ShowSpinner(string message, Func<Task> action)
        {
            Console.Write($"{message} ");
            var spinner = new[] { '/', '-', '\\', '|' };
            var spinnerPos = 0;
            var isRunning = true;

            var task = Task.Run(async () =>
            {
                while (isRunning)
                {
                    Console.Write(spinner[spinnerPos]);
                    spinnerPos = (spinnerPos + 1) % spinner.Length;
                    await Task.Delay(100);
                    Console.Write("\b"); // Erase last char
                }
            });

            try
            {
                await action();
            }
            finally
            {
                isRunning = false;
                await task;
                Console.WriteLine(" Done!");
            }
        }
    }
}
