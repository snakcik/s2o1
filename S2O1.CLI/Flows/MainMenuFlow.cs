using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using S2O1.Core.Interfaces;
using S2O1.CLI.Helpers;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;

namespace S2O1.CLI.Flows
{
    public class MainMenuFlow
    {
        private readonly IServiceProvider _provider;
        private readonly ICurrentUserService _currentUser;
        private readonly S2O1DbContext _context;

        public MainMenuFlow(IServiceProvider provider)
        {
            _provider = provider;
            _currentUser = provider.GetRequiredService<ICurrentUserService>();
            _context = provider.GetRequiredService<S2O1DbContext>();
        }

        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintLogo();
                
                var workMode = await GetSettingAsync("UsageType") ?? "Master";
                
                ConsoleHelper.PrintInfo($"User: {_currentUser.UserName} | Role: {_currentUser.UserRole}");
                ConsoleHelper.PrintWarning($"System Mode: {workMode}");
                Console.WriteLine("--------------------------------------------------");

                var options = new List<string>
                {
                    "Db-setup: Database Wizard",
                    "API Key Management",
                    "System Work Mode (Master/Slave)",
                    "Deployment Environment (Dev/Prod)",
                    "Licence Settings",
                    "System Statistics & Security Reset",
                    "Log out"
                };

                int choice = MenuHelper.ShowMenu("Main Menu", options);

                switch (choice)
                {
                    case 0: await RunDbSetup(); break;
                    case 1: await RunApiKeyManagement(); break;
                    case 2: await RunSystemWorkMode(); break;
                    case 3: await RunDeploymentEnvironment(); break;
                    case 4: await RunLicenceSettings(); break;
                    case 5: await RunStatistics(); break;
                    case 6: 
                        ConsoleHelper.PrintInfo("Logging out...");
                        return;
                }
            }
        }

        #region 1. Db-setup
        private async Task RunDbSetup()
        {
            while (true)
            {
                var options = new List<string>
                {
                    "View Current Connection String",
                    "Update Connection (Run Wizard)",
                    "Test Connection",
                    "Back to Main Menu"
                };

                int choice = MenuHelper.ShowMenu("Database Setup", options);

                if (choice == 0)
                {
                    string connStr = _context.Database.GetConnectionString();
                    ConsoleHelper.PrintInfo($"Current Connection Information (Saved in dbconfig.txt):");
                    Console.WriteLine(connStr);
                    ConsoleHelper.ReadLine();
                }
                else if (choice == 1)
                {
                    ConsoleHelper.PrintWarning("This will launch the Setup Wizard. You may need to restart the CLI after changes.");
                    ConsoleHelper.PrintWarning("Continue? (Y/N)");
                    string confirm = ConsoleHelper.ReadLine();
                    if (confirm?.ToLower() == "y")
                    {
                        var wizard = new SetupWizard();
                        await wizard.RunAsync();
                        ConsoleHelper.PrintInfo("Configuration updated. Please restart the application.");
                        Environment.Exit(0);
                    }
                }
                else if (choice == 2)
                {
                    ConsoleHelper.ShowSpinner("Testing Connection...", async () =>
                    {
                        try
                        {
                            bool canConnect = await _context.Database.CanConnectAsync();
                            if (canConnect) ConsoleHelper.PrintSuccess("Connection Successful!");
                            else ConsoleHelper.PrintError("Connection Failed!");
                        }
                        catch (Exception ex)
                        {
                            ConsoleHelper.PrintError($"Error: {ex.Message}");
                        }
                    });
                    ConsoleHelper.ReadLine();
                }
                else return;
            }
        }
        #endregion

        #region 2. API Key Management
        private async Task RunApiKeyManagement()
        {
            while (true)
            {
                var options = new List<string>
                {
                    "Create New API Key (Root)",
                    "List Active Keys",
                    "Revoke Key",
                    "Back to Main Menu"
                };

                int choice = MenuHelper.ShowMenu("API Key Management", options);

                if (choice == 0) // Create
                {
                    if (!_currentUser.UserId.HasValue) 
                    {
                        ConsoleHelper.PrintError("User ID not found (Login issue).");
                        return;
                    }

                    var newKey = new UserApiKey
                    {
                        UserId = _currentUser.UserId.Value,
                        KeyName = "CLI Generated Key",
                        ApiKey = "s2o1_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                        SecretKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                        CreateDate = DateTime.Now,
                        ExpiresDate = DateTime.Now.AddYears(1),
                        IsActive = true
                    };
                    
                    await _context.UserApiKeys.AddAsync(newKey);
                    await _context.SaveChangesAsync();
                    
                    ConsoleHelper.PrintSuccess("API Key Created Successfully!");
                    Console.WriteLine($"Name: {newKey.KeyName}");
                    Console.WriteLine($"API Key: {newKey.ApiKey}");
                    Console.WriteLine($"Secret Key: {newKey.SecretKey}");
                    ConsoleHelper.PrintWarning("SAVE THESE KEYS! Secret cannot be recovered securely later.");
                    await LogActionAsync("Create API Key", $"Created key ID: {newKey.Id}");
                    ConsoleHelper.ReadLine();
                }
                else if (choice == 1) // List
                {
                    var keys = await _context.UserApiKeys
                        .Where(k => k.IsActive)
                        .Select(k => new { k.Id, k.ApiKey, k.ExpiresDate })
                        .ToListAsync();
                        
                    ConsoleHelper.PrintInfo("Active API Keys:");
                    foreach (var key in keys)
                    {
                        Console.WriteLine($"ID: {key.Id} | Expires: {key.ExpiresDate} | Key: {key.ApiKey}");
                    }
                    ConsoleHelper.ReadLine();
                }
                else if (choice == 2) // Revoke
                {
                    Console.Write("Enter Key ID to Revoke: ");
                    string input = ConsoleHelper.ReadLine();
                    if (int.TryParse(input, out int keyId))
                    {
                        var key = await _context.UserApiKeys.FindAsync(keyId);
                        if (key != null)
                        {
                            key.IsActive = false;
                            await _context.SaveChangesAsync();
                            ConsoleHelper.PrintSuccess("Key revoked.");
                            await LogActionAsync("Revoke API Key", $"Revoked key ID: {keyId}");
                        }
                        else ConsoleHelper.PrintError("Key not found.");
                    }
                    ConsoleHelper.ReadLine();
                }
                else return;
            }
        }
        #endregion

        #region 3. System Work Mode
        private async Task RunSystemWorkMode()
        {
            while (true)
            {
                var usageType = await GetSettingAsync("UsageType") ?? "Master";
                
                var options = new List<string>
                {
                    $"Set Mode: Master {(usageType == "Master" ? "[CURRENT]" : "")}",
                    $"Set Mode: Slave {(usageType == "Slave" ? "[CURRENT]" : "")}",
                    "Back to Main Menu"
                };

                int mainChoice = MenuHelper.ShowMenu($"System Work Mode (Current: {usageType})", options);

                if (mainChoice == 0) // Master
                {
                    await SetSettingAsync("UsageType", "Master");
                    ConsoleHelper.PrintSuccess("System set to MASTER mode (Standalone).");
                    await LogActionAsync("Update Work Mode", "Set to Master");
                    ConsoleHelper.ReadLine();
                }
                else if (mainChoice == 1) // Slave
                {
                    await SetSettingAsync("UsageType", "Slave");
                    ConsoleHelper.PrintSuccess("System set to SLAVE mode.");
                    await LogActionAsync("Update Work Mode", "Set to Slave");
                    
                    while (true)
                    {
                        var ips = await GetSettingAsync("AllowedIPs") ?? "None";
                        var domains = await GetSettingAsync("AllowedDomains") ?? "None";
                        
                        var slaveOptions = new List<string>
                        {
                            $"Set Allowed IPs (Current: {ips})",
                            $"Set Allowed Domains (Current: {domains})",
                            "Back to Mode Selection"
                        };
                        
                        int slaveChoice = MenuHelper.ShowMenu("Slave Configuration", slaveOptions);
                        
                        if (slaveChoice == 0)
                        {
                            Console.WriteLine("Enter IPs separated by comma (e.g. 192.168.1.10, 10.0.0.5):");
                            Console.WriteLine("Type 'exit' to cancel.");
                            string input = ConsoleHelper.ReadLine();
                            if (input?.ToLower() != "exit")
                            {
                                await SetSettingAsync("AllowedIPs", input ?? "");
                                ConsoleHelper.PrintSuccess("Allowed IPs updated.");
                            }
                        }
                        else if (slaveChoice == 1)
                        {
                            Console.WriteLine("Enter Domains separated by comma (e.g. https://main.com, https://app.com):");
                            Console.WriteLine("Type 'exit' to cancel.");
                            string input = ConsoleHelper.ReadLine();
                            if (input?.ToLower() != "exit")
                            {
                                await SetSettingAsync("AllowedDomains", input ?? "");
                                ConsoleHelper.PrintSuccess("Allowed Domains updated.");
                            }
                        }
                        else break;
                    }
                }
                else return;
            }
        }
        #endregion

        #region 3a. Deployment Environment
        private async Task RunDeploymentEnvironment()
        {
            while (true)
            {
                var currentEnv = await GetSettingAsync("DeploymentEnvironment") ?? "Production";
                
                var options = new List<string>
                {
                    $"Set Environment: Production {(currentEnv == "Production" ? "[CURRENT]" : "")}",
                    $"Set Environment: Development (Enables Swagger API Docs) {(currentEnv == "Development" ? "[CURRENT]" : "")}",
                    "Back to Main Menu"
                };

                int envChoice = MenuHelper.ShowMenu($"Deployment Environment (Current: {currentEnv})", options);

                if (envChoice == 0) // Production
                {
                    await SetSettingAsync("DeploymentEnvironment", "Production");
                    ConsoleHelper.PrintSuccess("Environment set to Production.");
                    ConsoleHelper.PrintWarning("Please restart the API container (e.g., docker restart s2o1_api) for changes to take effect.");
                    await LogActionAsync("Update Environment", "Set to Production");
                    ConsoleHelper.ReadLine();
                }
                else if (envChoice == 1) // Development
                {
                    await SetSettingAsync("DeploymentEnvironment", "Development");
                    ConsoleHelper.PrintSuccess("Environment set to Development. Swagger API docs will be enabled.");
                    ConsoleHelper.PrintWarning("Please restart the API container (e.g., docker restart s2o1_api) for changes to take effect.");
                    await LogActionAsync("Update Environment", "Set to Development");
                    ConsoleHelper.ReadLine();
                }
                else return;
            }
        }
        #endregion

        #region 4. Licence Settings
        private async Task RunLicenceSettings()
        {
            bool isEnabled = (await GetSettingAsync("LicenceCheckEnabled") ?? "true") == "true";
            
            var options = new List<string>
            {
                $"Disable Licence Check {(isEnabled ? "" : "[CURRENT]")}",
                $"Enable Licence Check {(isEnabled ? "[CURRENT]" : "")}",
                "Back"
            };

            int choice = MenuHelper.ShowMenu("Licence Settings", options);
            if (choice == 0)
            {
                await SetSettingAsync("LicenceCheckEnabled", "false");
                ConsoleHelper.PrintSuccess("Licence check DISABLED.");
            }
            else if (choice == 1)
            {
                await SetSettingAsync("LicenceCheckEnabled", "true");
                ConsoleHelper.PrintSuccess("Licence check ENABLED.");
            }
            if (choice != 2) ConsoleHelper.ReadLine();
        }
        #endregion

        #region 5. System Statistics
        private async Task RunStatistics()
        {
            await ConsoleHelper.ShowSpinner("Fetching Statistics...", async () =>
            {
                try {
                    int productCount = await _context.Products.CountAsync();
                    
                    // Join Products with StockAlerts to find critical levels
                    int alertCount = await _context.StockAlerts
                        .Include(a => a.Product)
                        .CountAsync(a => a.Product.CurrentStock <= a.MinStockLevel);
                    
                    Console.WriteLine($"\nTotal Products: {productCount}");
                    Console.WriteLine($"Critical Stock Alerts: {alertCount}");
                    Console.WriteLine($"Connected Clients (24h): 5 (Placeholder)");
                } catch (Exception ex) {
                    Console.WriteLine($"Error fetching stats: {ex.Message}");
                }
            });

            Console.WriteLine("\n[Security Reset]");
            Console.WriteLine("1. Unlock User");
            Console.WriteLine("0. Back");
            
            string choice = ConsoleHelper.ReadLine();
            if (choice == "1")
            {
                Console.Write("Enter Username to Unlock: ");
                string username = ConsoleHelper.ReadLine();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null)
                {
                    user.AccessFailedCount = 0;
                    user.LockoutEnd = null;
                    await _context.SaveChangesAsync();
                    ConsoleHelper.PrintSuccess($"User '{username}' unlocked.");
                    await LogActionAsync("Security Reset", $"Unlocked user {username}");
                }
                else ConsoleHelper.PrintError("User not found.");
            }
            
            ConsoleHelper.ReadLine();
        }
        #endregion

        #region Helpers
        private async Task<string> GetSettingAsync(string key)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            return setting?.SettingValue;
        }

        private async Task SetSettingAsync(string key, string value)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                setting = new SystemSetting 
                { 
                    SettingKey = key, 
                    SettingValue = value,
                    AppVersion = "1.0.0", // Default required value
                    LogoAscii = "S2O1"    // Default required value
                };
                await _context.SystemSettings.AddAsync(setting);
            }
            else
            {
                setting.SettingValue = value;
            }
            await _context.SaveChangesAsync();
        }

        private async Task LogActionAsync(string action, string details)
        {
            try {
                var log = new AuditLog 
                { 
                    Source = "CLI", 
                    ActionType = action, 
                    ActionDescription = details, 
                    CreateDate = DateTime.Now, 
                    ActorUserId = _currentUser.UserId,
                    ActorRole = _currentUser.UserRole,
                    IPAddress = "Local/CLI",
                    EntityName = "System",
                    EntityId = "0"
                };
                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            } catch {}
        }
        #endregion
    }
}
