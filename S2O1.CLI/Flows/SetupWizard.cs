using S2O1.CLI.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using S2O1.DataAccess;
using S2O1.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
using S2O1.Core.Security;
using Microsoft.Data.Sql;
using System.IO;
using System.Linq;

namespace S2O1.CLI.Flows
{
    public class SetupWizard
    {
        public async Task RunAsync()
        {
            ConsoleHelper.PrintInfo("Starting First Run Setup...");
            while (true)
            {
                // Menu
                var options = new List<string> { "Program Installation", "Create Database", "Connect Database", "Uninstall Program", "Exit" };
                int selection = MenuHelper.ShowMenu("Select an option:", options);

                try
                {
                    switch (selection)
                    {
                        case 0: // Install
                            await InstallProgramFiles();
                            break;
                        case 1: // Create DB
                            await CreateDatabaseFlow();
                            break;
                        case 2: // Connect DB
                             await ConnectDatabaseFlow();
                            break;
                        case 3: // Uninstall
                            await UninstallProgram();
                            break;
                        case 4:
                            Environment.Exit(0);
                            break;
                    }
                }
                catch (Exception ex)
                {
                   ConsoleHelper.PrintError($"Operation Failed: {ex.Message}");
                }
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private async Task InstallProgramFiles()
        {
            Console.WriteLine("Installing to Program Files...");
            string targetDir = @"C:\Program Files\2S1O"; // Windows specific
            
            // Check OS
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                targetDir = "/usr/local/bin/2s1o"; // Linux placeholder
                ConsoleHelper.PrintWarning("Linux installation path: " + targetDir);
            }

            try 
            {
                if (!System.IO.Directory.Exists(targetDir))
                {
                    System.IO.Directory.CreateDirectory(targetDir);
                    ConsoleHelper.PrintInfo($"Created directory: {targetDir}");
                }

                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] files = System.IO.Directory.GetFiles(sourceDir);

                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string destFile = System.IO.Path.Combine(targetDir, fileName);
                    System.IO.File.Copy(file, destFile, true);
                    Console.WriteLine($"Copied: {fileName}");
                }
                
                InstallationHelper.MarkAsInstalled();
                ConsoleHelper.PrintSuccess($"Installation Complete at {targetDir}.");
                ConsoleHelper.PrintSuccess("Registry/Flag updated. Please restart CLI or use the installed executable.");
            }
            catch (UnauthorizedAccessException) 
            {
                ConsoleHelper.PrintError("Installation Failed: Access Denied. Please Run as Administrator.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Installation Failed: {ex.Message}");
            }
        }

        private async Task UninstallProgram()
        {
             Console.WriteLine("Uninstalling Program...");
             string targetDir = @"C:\Program Files\2S1O";
             if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
             {
                 targetDir = "/usr/local/bin/2s1o";
             }
             
             try
             {
                 // 1. Remove Registry Key
                 if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                 {
                     Console.WriteLine("Removing Registry Keys...");
                     try 
                     {
                         using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software", true);
                         if (key != null)
                         {
                             // Clean up key
                             Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(@"Software\2S1O", false);
                             ConsoleHelper.PrintSuccess("Registry keys removed.");
                         }
                         else
                         {
                             ConsoleHelper.PrintWarning("Registry key not found.");
                         }
                     }
                     catch(Exception ex) 
                     {
                         ConsoleHelper.PrintError($"Registry cleanup failed (might not exist or permission denied): {ex.Message}");
                     }
                 }
                 
                 // 2. Remove Files
                 if (System.IO.Directory.Exists(targetDir))
                 {
                     Console.WriteLine($"Deleting files from {targetDir}...");
                     System.IO.Directory.Delete(targetDir, true);
                     ConsoleHelper.PrintSuccess("Program files removed.");
                 }
                 else
                 {
                     ConsoleHelper.PrintWarning("Program files directory not found.");
                 }
                 
                 ConsoleHelper.PrintSuccess("Uninstallation Complete.");
             }
             catch(UnauthorizedAccessException)
             {
                 ConsoleHelper.PrintError("Uninstall Failed: Access Denied. Please Run as Administrator.");
             }
             catch (Exception ex)
             {
                 ConsoleHelper.PrintError($"Uninstall Failed: {ex.Message}");
             }
        }


        private async Task CreateDatabaseFlow()
        {
            ConsoleHelper.PrintWarning("⚠️  UYARI: Bu seçenek YENİ bir veritabanı oluşturur.");
            ConsoleHelper.PrintWarning("Eğer veritabanı zaten varsa, 'Connect Database' seçeneğini kullanın!");
            Console.WriteLine();
            
            List<string> servers = new List<string>();
            await ConsoleHelper.ShowSpinner("Discovering SQL Servers...", async () => 
            {
                servers = await Task.Run(() => DiscoverSqlServers());
            });
            
            if (servers.Count == 0)
            {
                ConsoleHelper.PrintWarning("No SQL Servers found. Please enter manually.");
                servers.Add("localhost");
                servers.Add("(localdb)\\MSSQLLocalDB");
            }

            var selectedServer = MenuHelper.ShowMenu("Select SQL Server:", servers);
            string server = servers[selectedServer];
            
            ConsoleHelper.PrintInfo($"Selected: {server}");
            
            // Determine if LocalDB or regular SQL Server
            bool isLocalDb = server.ToLower().Contains("localdb");
            string connStr;
            
            if (isLocalDb)
            {
                connStr = $"Server={server};Database=2S1O;Integrated Security=true;TrustServerCertificate=True;";
                ConsoleHelper.PrintInfo("Using Windows Authentication (Integrated Security)");
            }
            else
            {
                Console.Write("Enter User ID (Default: sa): ");
                string userId = ConsoleHelper.ReadLine();
                if (string.IsNullOrWhiteSpace(userId)) userId = "sa";

                Console.Write("Enter Password: ");
                string pass = ConsoleHelper.ReadPassword();
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleHelper.PrintError("Password cannot be empty for SQL Server authentication.");
                    return;
                }

                connStr = $"Server={server};Database=2S1O;User Id={userId};Password={pass};TrustServerCertificate=True;";
            }
            
            ConsoleHelper.PrintInfo($"Connecting to {server}...");

            try
            {
                var services = new ServiceCollection();
                services.AddScoped<IPasswordHasher, PasswordHasher>();
                services.AddDataAccess(connStr);
                
                var provider = services.BuildServiceProvider();
                var dbContext = provider.GetRequiredService<S2O1DbContext>();

                // Check if database exists
                bool dbExists = await dbContext.Database.CanConnectAsync();
                
                if (dbExists)
                {
                    ConsoleHelper.PrintInfo("Database connection established. Checking existing tables...");
                    
                    // Check if ANY user tables exist (not just migration history)
                    // We check for 'Users', 'AuditLogs', or '__EFMigrationsHistory'
                    var tableCheckSql = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_TYPE = 'BASE TABLE' 
                        AND TABLE_NAME IN ('Users', 'AuditLogs', '__EFMigrationsHistory')";
                        
                    var existingTableCount = await dbContext.Database.ExecuteSqlRawAsync(tableCheckSql);
                    // Note: ExecuteSqlRawAsync returns rows affected, not scalar. 
                    // Better to use a command connection for scalar or check specific table existance.
                    // Let's use a robust check:
                    
                    bool hasTables = false;
                    try
                    {
                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = "SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != 'sysdiagrams'";
                            await dbContext.Database.OpenConnectionAsync();
                            var result = await command.ExecuteScalarAsync();
                            hasTables = result != null;
                            dbContext.Database.CloseConnection();
                        }
                    }
                    catch { hasTables = false; } // Access error or db issue

                    if (hasTables)
                    {
                        ConsoleHelper.PrintWarning("Existing tables found in the database!");
                        ConsoleHelper.PrintWarning("Continue will DELETE ALL DATA and recreate the database.");
                        ConsoleHelper.PrintWarning("Do you want to RESET the database? (Y/N)");
                        
                        var options = new List<string> { "YES - Reset Database (Data Loss)", "NO - Cancel (Go to Connect)" };
                        int choice = MenuHelper.ShowMenu("Select Action:", options);
                        
                        if (choice == 0) // YES - Reset
                        {
                            ConsoleHelper.PrintInfo("Deleting existing database...");
                            await dbContext.Database.EnsureDeletedAsync();
                            ConsoleHelper.PrintSuccess("Database deleted.");
                            
                            ConsoleHelper.PrintInfo("Creating new database schema...");
                            await dbContext.Database.MigrateAsync();
                            ConsoleHelper.PrintSuccess("Database created successfully!");
                        }
                        else // NO - Cancel
                        {
                            ConsoleHelper.PrintInfo("Operation cancelled. Please use 'Connect to Existing Database' in main menu.");
                            return; 
                        }
                    }
                    else
                    {
                        // Database exists but is empty (no tables)
                        ConsoleHelper.PrintInfo("Database is empty. Applying migrations...");
                        await dbContext.Database.MigrateAsync();
                    }
                }
                else
                {
                    // Database doesn't exist - create it
                    ConsoleHelper.PrintInfo("Creating new database...");
                    await dbContext.Database.MigrateAsync();
                    ConsoleHelper.PrintSuccess("Database created successfully!");
                }
                
                // Seed initial data
                ConsoleHelper.PrintInfo("Seeding initial data...");
                await DbInitializer.InitializeAsync(dbContext, provider);
                ConsoleHelper.PrintSuccess("Database setup complete!");
                
                // Save connection string
                SaveConnectionString(connStr);
                InstallationHelper.MarkAsInstalled();
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : "";
                throw new Exception($"Database creation failed: {ex.Message}{innerMsg}");
            }
        }

        private List<string> DiscoverSqlServers()
        {
            var servers = new List<string>();
            
            try
            {
                var enumerator = Microsoft.Data.Sql.SqlDataSourceEnumerator.Instance;
                var table = enumerator.GetDataSources();
                
                foreach (System.Data.DataRow row in table.Rows)
                {
                    string serverName = row["ServerName"]?.ToString();
                    string instanceName = row["InstanceName"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(serverName))
                    {
                        if (string.IsNullOrEmpty(instanceName))
                        {
                            servers.Add(serverName);
                        }
                        else
                        {
                            servers.Add($"{serverName}\\{instanceName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintWarning($"Discovery failed: {ex.Message}");
            }
            
            // Always add LocalDB as an option
            if (!servers.Any(s => s.ToLower().Contains("localdb")))
            {
                servers.Add("(localdb)\\MSSQLLocalDB");
            }
            
            return servers;
        }

        private void SaveConnectionString(string connStr)
        {
            try
            {
                // Save to a config file or registry for future use
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.txt");
                File.WriteAllText(configPath, connStr);
                ConsoleHelper.PrintInfo("Connection string saved.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintWarning($"Could not save connection string: {ex.Message}");
            }
        }

        private async Task ConnectDatabaseFlow()
        {
            Console.WriteLine("Connect to Existing Database");
            Console.WriteLine("----------------------------");
            
            Console.WriteLine("Discovering SQL Servers...");
            var servers = DiscoverSqlServers();
            
            if (servers.Count == 0)
            {
                ConsoleHelper.PrintWarning("No SQL Servers found. Please enter manually.");
                servers.Add("localhost");
                servers.Add("(localdb)\\MSSQLLocalDB");
            }

            var selectedServer = MenuHelper.ShowMenu("Select SQL Server:", servers);
            string server = servers[selectedServer];
            
            ConsoleHelper.PrintInfo($"Selected: {server}");
            
            // Determine if LocalDB or regular SQL Server
            bool isLocalDb = server.ToLower().Contains("localdb");
            string connStr;
            
            if (isLocalDb)
            {
                connStr = $"Server={server};Database=2S1O;Integrated Security=true;TrustServerCertificate=True;";
                ConsoleHelper.PrintInfo("Using Windows Authentication (Integrated Security)");
            }
            else
            {
                Console.Write("Enter User ID (Default: sa): ");
                string userId = ConsoleHelper.ReadLine();
                if (string.IsNullOrWhiteSpace(userId)) userId = "sa";

                Console.Write("Enter Password: ");
                string pass = ConsoleHelper.ReadPassword();
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleHelper.PrintError("Password cannot be empty for SQL Server authentication.");
                    return;
                }

                connStr = $"Server={server};Database=2S1O;User Id={userId};Password={pass};TrustServerCertificate=True;";
            }

            ConsoleHelper.PrintInfo($"Testing connection to {server}...");

            try
            {
                var services = new ServiceCollection();
                services.AddScoped<IPasswordHasher, PasswordHasher>();
                services.AddDataAccess(connStr);
                
                var provider = services.BuildServiceProvider();
                var dbContext = provider.GetRequiredService<S2O1DbContext>();

                if (await dbContext.Database.CanConnectAsync())
                {
                     ConsoleHelper.PrintSuccess("Connection Successful!");
                     InstallationHelper.MarkAsInstalled(); 
                     SaveConnectionString(connStr);
                     ConsoleHelper.PrintSuccess("Configuration Saved/Marked as Installed.");
                }
                else
                {
                    ConsoleHelper.PrintError("Connection Failed: Cannot connect to database.");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Connection Failed: {ex.Message}");
            }
        }
    }
}
