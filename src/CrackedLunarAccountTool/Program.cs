using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Linq;
using Console = Colorful.Console;
using CrackedLunarAccountTool.Helpers;

namespace CrackedLunarAccountTool
{
    internal class Program
    {
        // Beautiful color palette
        private static readonly Color Primary = Color.FromArgb(102, 204, 255);   // Sky Blue
        private static readonly Color Secondary = Color.FromArgb(255, 178, 102);  // Peach
        private static readonly Color Success = Color.FromArgb(102, 255, 178);    // Mint Green
        private static readonly Color Warning = Color.FromArgb(255, 204, 102);    // Golden Yellow
        private static readonly Color Error = Color.FromArgb(255, 102, 153);      // Coral Pink
        private static readonly Color Info = Color.FromArgb(178, 153, 255);       // Lavender
        private static readonly Color Accent = Color.FromArgb(255, 153, 204);     // Light Pink
        private static readonly Color Background = Color.FromArgb(30, 30, 40);    // Dark Navy

        public static readonly string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string lunarAcccountsPath = Path.Combine(userFolder, ".lunarclient/settings/game/accounts.json");
        public static readonly string ver = "v1.0";
        public static readonly string lunarClientPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Lunar Client",
            "Lunar Client.exe"
        );

        static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static void Main(string[] args)
        {
            // Set console appearance
            Console.BackgroundColor = Background;
            Console.ForegroundColor = Color.WhiteSmoke;
            Console.Clear();

            // Check and request admin privileges
            if (!IsAdministrator())
            {
                ConsoleHelpers.PrintLine("INFO", "Administrator privileges required. Requesting elevation...", Info);

                try
                {
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    var startInfo = new ProcessStartInfo(exeName)
                    {
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    Thread.Sleep(1500);
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.PrintLine("ERROR", $"Admin elevation failed: {ex.Message}", Error);
                    ConsoleHelpers.PrintLine("INFO", "Program will exit in 3 seconds...", Info);
                    Thread.Sleep(3000);
                }
                return;
            }

            // Run program with admin privileges
            RunProgram();
        }

        private static void RunProgram()
        {
            AccountManager.LoadJson();

            bool continueProgram = true;
            while (continueProgram)
            {
                Console.Clear();
                Console.Title = $"LC Account Tool {ver} | by drako";

                SetConsoleSize(80, 25);
                PrintMenu();

                string choice = Console.ReadLine();
                try
                {
                    switch (int.Parse(choice))
                    {
                        case 1:
                            CreateAccountPrompt();
                            break;
                        case 2:
                            RemoveAccountsMenu();
                            break;
                        case 3:
                            AccountManager.ViewInstalledAccounts();
                            ConsoleHelpers.PrintLine("INFO", "Press any key to continue...", Info);
                            Console.ReadKey();
                            break;
                        case 4:
                            ConsoleHelpers.PrintLine("SUCCESS", "Exiting the program. Goodbye!", Success);
                            continueProgram = false;
                            break;
                        default:
                            ConsoleHelpers.PrintLine("ERROR", "Invalid choice. Please select 1-4.", Error);
                            Thread.Sleep(1200);
                            break;
                    }
                }
                catch (Exception e)
                {
                    ConsoleHelpers.PrintLine("ERROR", $"An error occurred: {e.Message}", Error);
                    Thread.Sleep(1500);
                }
            }

            AccountManager.SaveJson();
        }

        private static void SetConsoleSize(int width, int height)
        {
            int maxWidth = Console.LargestWindowWidth;
            int maxHeight = Console.LargestWindowHeight;

            int consoleWidth = Math.Min(width, maxWidth);
            int consoleHeight = Math.Min(height, maxHeight);

            Console.SetWindowSize(consoleWidth, consoleHeight);
            Console.SetBufferSize(consoleWidth, consoleHeight);
        }

        private static void PrintMenu()
        {
            Console.WriteLine();
            ConsoleHelpers.PrintLine("+", $"LUNAR CLIENT ACCOUNT TOOL {ver}", Primary);
            ConsoleHelpers.PrintLine("+", "by drako", Secondary);
            Console.WriteLine();

            ConsoleHelpers.PrintLine("1", "Create Account", Primary);
            ConsoleHelpers.PrintLine("2", "Remove Accounts", Primary);
            ConsoleHelpers.PrintLine("3", "View Installed Accounts", Primary);
            ConsoleHelpers.PrintLine("4", "Exit Program", Primary);

            Console.WriteLine();
            ConsoleHelpers.Print("INPUT", "Select an option: ", Accent);
        }

        public static void RemoveAccountsMenu()
        {
            Console.Clear();
            ConsoleHelpers.PrintLine("QUERY", "REMOVE ACCOUNTS", Secondary);
            Console.WriteLine();

            ConsoleHelpers.PrintLine("1", "Remove All Accounts", Primary);
            ConsoleHelpers.PrintLine("2", "Remove Cracked Accounts", Primary);
            ConsoleHelpers.PrintLine("3", "Remove Premium Accounts", Primary);
            ConsoleHelpers.PrintLine("0", "Return to Main Menu", Primary);

            Console.WriteLine();
            ConsoleHelpers.Print("INPUT", "Select an option: ", Accent);

            string choice = Console.ReadLine();

            // Close Lunar Client before proceeding
            if (!CloseLunarClientLauncher())
            {
                ConsoleHelpers.PrintLine("WARNING", "Modifications might not save properly!", Warning);
                Thread.Sleep(1500);
            }

            switch (int.Parse(choice))
            {
                case 1:
                    AccountManager.RemoveAllAccounts();
                    ConsoleHelpers.PrintLine("SUCCESS", "✓ All accounts removed successfully!", Success);
                    break;
                case 2:
                    AccountManager.RemoveCrackedAccounts();
                    ConsoleHelpers.PrintLine("SUCCESS", "✓ Cracked accounts removed successfully!", Success);
                    break;
                case 3:
                    AccountManager.RemovePremiumAccounts();
                    ConsoleHelpers.PrintLine("SUCCESS", "✓ Premium accounts removed successfully!", Success);
                    break;
                case 0:
                    return;
                default:
                    ConsoleHelpers.PrintLine("ERROR", "Invalid selection!", Error);
                    break;
            }

            Thread.Sleep(1500);
            AccountManager.SaveJson();
        }

        private static void CreateAccountPrompt()
        {
            Console.Clear();
            ConsoleHelpers.PrintLine("QUERY", "CREATE ACCOUNT", Secondary);
            Console.WriteLine();

            ConsoleHelpers.Print("INPUT", "Enter username: ", Accent);
            string usernameAdd = Console.ReadLine();

            if (!Validate.IsValidMinecraftUsername(usernameAdd))
            {
                ConsoleHelpers.PrintLine("WARNING", "⚠ This username might cause issues on servers", Warning);
            }

            // Close Lunar Client before proceeding
            if (!CloseLunarClientLauncher())
            {
                ConsoleHelpers.PrintLine("WARNING", "⚠ Account might not save properly!", Warning);
                Thread.Sleep(1500);
            }

            while (true)
            {
                ConsoleHelpers.Print("INPUT", "Enter UUID: ", Accent);
                string customuuidAdd = Console.ReadLine();

                if (!Validate.IsValidUUID(customuuidAdd))
                {
                    ConsoleHelpers.PrintLine("ERROR", "✗ Invalid UUID format!", Error);
                    ConsoleHelpers.Print("QUERY", "Try again? (y/n): ", Info);

                    string retry = Console.ReadLine()?.Trim().ToLower();
                    if (retry == "n")
                    {
                        ConsoleHelpers.PrintLine("INFO", "Returning to menu...", Info);
                        Thread.Sleep(1000);
                        return;
                    }
                }
                else
                {
                    AccountManager.CreateAccount(usernameAdd, customuuidAdd);
                    AccountManager.SaveJson();
                    ConsoleHelpers.PrintLine("SUCCESS", "✓ Account created successfully!", Success);
                    Thread.Sleep(1500);
                    break;
                }
            }
        }

        private static bool CloseLunarClientLauncher()
        {
            try
            {
                // Find Lunar Client processes
                var launcherProcesses = Process.GetProcessesByName("Lunar Client")
                    .Where(p => {
                        try { return !p.HasExited; }
                        catch { return false; }
                    })
                    .ToList();

                if (launcherProcesses.Count == 0)
                {
                    return true; // Already closed
                }

                ConsoleHelpers.PrintLine("WARNING", "LUNAR CLIENT DETECTED", Warning);
                ConsoleHelpers.PrintLine("WARNING", "Lunar Client must be closed to modify accounts", Warning);
                ConsoleHelpers.Print("QUERY", "Close Lunar Client now? (y/n): ", Info);

                string response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y")
                {
                    ConsoleHelpers.PrintLine("INFO", "Lunar Client remains running", Info);
                    Thread.Sleep(1000);
                    return false;
                }

                bool allClosed = true;
                foreach (Process process in launcherProcesses)
                {
                    try
                    {
                        // Try graceful shutdown first
                        process.CloseMainWindow();

                        // Wait for exit with timeout
                        if (!process.WaitForExit(1500))
                        {
                            // Force kill if not closed
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelpers.PrintLine("ERROR", $"✗ Close error: {ex.Message}", Error);
                        allClosed = false;
                    }
                }

                if (allClosed)
                {
                    ConsoleHelpers.PrintLine("SUCCESS", "✓ Lunar Client closed successfully!", Success);
                    Thread.Sleep(1000);
                }

                return allClosed;
            }
            catch (Exception ex)
            {
                ConsoleHelpers.PrintLine("ERROR", $"✗ Process error: {ex.Message}", Error);
                Thread.Sleep(1500);
                return false;
            }
        }
    }
}
