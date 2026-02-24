using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using S2O1.Business;
using S2O1.DataAccess;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.CLI.Helpers;
using S2O1.CLI.Services;
using System;
using System.Threading.Tasks;

namespace S2O1.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup DI
            // We need to read connection string from Setup (Registry/File) or Environment.
            // Requirement: "Registery nin içine ... Installed = true".
            // Connection params should be stored securely or asked.
            // Lines 96-101: "ConnectDatabase... User: Pass...".
            
            // For now, let's setup Host with dummy connection string or builder.
            // Actually, we might need to REBUILD the provider if connection changes (ConnectDatabase).
            // So we might use a wrapper for ConnectionString or DbContextFactory.
            
            // Let's start clean.
            ConsoleHelper.PrintLogo();

            // Check Installation
            try 
            {
                if (!InstallationHelper.IsInstalled())
                {
                    ConsoleHelper.PrintWarning("System not installed. Launching Setup Wizard...");
                    var setup = new S2O1.CLI.Flows.SetupWizard();
                    await setup.RunAsync();
                    // After setup, continue to login flow
                }
            } 
            catch (Exception ex)
            {
                 ConsoleHelper.PrintError($"Installation check failed: {ex.Message}");
            }

            if (args.Length > 0)
            {
                if (args[0] == "-setup")
                {
                    var setup = new S2O1.CLI.Flows.SetupWizard();
                    await setup.RunAsync();
                    // Fallthrough to login? No, explicit -setup usually means just setup.
                    return; 
                }
                
                if (args[0] == "login")
                {
                    await RunLoginFlow();
                    return;
                }
            }

            // Default action: Start Login Flow
            await RunLoginFlow();
            
            // Keep window open if debugging or ended abruptly
            // Console.WriteLine("Press any key to exit...");
            // Console.ReadKey();
        }
        
        static async Task RunLoginFlow()
        {
            string connString = ReadConfigConnection();
            
            if (string.IsNullOrEmpty(connString))
            {
                ConsoleHelper.PrintWarning("Configuration not found. Launching Setup Wizard...");
                var setup = new S2O1.CLI.Flows.SetupWizard();
                await setup.RunAsync();
                
                // Retry reading config
                connString = ReadConfigConnection();
                if (string.IsNullOrEmpty(connString))
                {
                    ConsoleHelper.PrintError("Setup failed or cancelled. Exiting.");
                    return;
                }
            }
            
            var services = new ServiceCollection();
            services.AddDataAccess(connString);
            services.AddBusinessLayer();
            services.AddSingleton<ICurrentUserService, CliCurrentUser>();
            
            var provider = services.BuildServiceProvider();
            
            // Start Monitor
            ConsoleHelper.StartInactivityMonitor();
            
            // Check Database Connection Before Login
            try
            {
                var dbContext = provider.GetRequiredService<S2O1.DataAccess.Contexts.S2O1DbContext>();
                if (!await dbContext.Database.CanConnectAsync())
                {
                    ConsoleHelper.PrintWarning("Could not connect to database (or it doesn't exist). Launching Setup Wizard...");
                    var setup = new S2O1.CLI.Flows.SetupWizard();
                    await setup.RunAsync();
                    return;
                }
            }
            catch (Exception)
            {
                ConsoleHelper.PrintWarning("Could not connect to database. Launching Setup Wizard...");
                var setup = new S2O1.CLI.Flows.SetupWizard();
                await setup.RunAsync();
                return;
            }

            // Login
            Console.Write("Username: ");
            string username = ConsoleHelper.ReadLine();
            Console.Write("Password: ");
            string password = ConsoleHelper.ReadPassword();
            
            var authService = provider.GetRequiredService<IAuthService>();
            try
            {
                var user = await authService.LoginAsync(new S2O1.Business.DTOs.Auth.LoginDto { UserName = username, Password = password });
                if (user != null)
                {
                    ConsoleHelper.PrintSuccess($"Welcome {user.UserName} ({user.Role})");
                    var currentUser = (CliCurrentUser)provider.GetRequiredService<ICurrentUserService>();
                    currentUser.SetUser(user.Id, user.UserName, user.Role);
                    
                    // Show Main Menu
                    ConsoleHelper.PrintSuccess("Login Successful! Loading Menu...");
                    await Task.Delay(1000);
                    
                    var mainMenu = new S2O1.CLI.Flows.MainMenuFlow(provider);
                    await mainMenu.RunAsync();
                    
                    // After logout/exit
                    ConsoleHelper.PrintInfo("Session ended.");
                }
                else
                {
                    ConsoleHelper.PrintError("Invalid credentials.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Login failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                     ConsoleHelper.PrintError($"Inner: {ex.InnerException.Message}");
                }
                ConsoleHelper.PrintError("Please check your database connection or try running setup again.");
            }
        }

        static string ReadConfigConnection()
        {
            try
            {
                string configPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "dbconfig.txt");
                if (System.IO.File.Exists(configPath))
                {
                   return System.IO.File.ReadAllText(configPath).Trim();
                }
            }
            catch { }
            return null;
        }
    }
}
