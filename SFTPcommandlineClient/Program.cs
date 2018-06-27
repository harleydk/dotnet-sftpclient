using DotNetSftp.Settings;
using DotNetSftp.UtilityClasses;
using Microsoft.Extensions.Logging;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Diagnostics;

namespace DotNetSftp
{
    public partial class Program
    {
        private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;
        private static DotNetSftpClient _dotNetSftpClient;

        public static void Main(string[] args)
        {
            try
            {
                // Parse commandline settings.
                SettingsParser settingsParser = new SettingsParser();
                settingsParser.ParseSettingsFromCommandlineArguments(args);

                WriteErrorMessageAndQuitIfNoArgumentsGiven(args);
                ShowHelpIfArgumentsSpecifyItSo(settingsParser.ApplicationSettings.ShowHelp, settingsParser.Options);

                // Initialize logging.
                _loggerFactory = new LoggerFactory();
                _loggerFactory.AddConsole();
                AddAnyExternalLogProviders(_loggerFactory);
                _logger = _loggerFactory.CreateLogger<Program>();

                string applicationVersion = GetAssemblyVersion();
                _logger.LogInformation($"**** DotNetSftp - version {applicationVersion} ****");



                // Handle settings file functionality.
                bool settingsFilePathIsSpecified = !string.IsNullOrWhiteSpace(settingsParser.ApplicationSettings.SettingsFilePath);
                if (settingsFilePathIsSpecified)
                {
                    bool settingsFileExists = File.Exists(settingsParser.ApplicationSettings.SettingsFilePath);

                    if (settingsFileExists)
                        settingsParser = ImportSettingsFromFile(settingsParser.ApplicationSettings.SettingsFilePath, _logger, settingsParser.ApplicationSettings.SettingsKeyFilePath);
                    else
                        SaveSettingsToFile(settingsParser, settingsParser.ApplicationSettings.SettingsFilePath, _logger, settingsParser.ApplicationSettings.SettingsKeyFilePath);
                }

                // if the settings-file contains info on disk-logging, re-configure the logger.
                if (!string.IsNullOrWhiteSpace(settingsParser.ApplicationSettings.DiskLogLocation))
                {
                    _logger.LogInformation($"** Logging to directory {settingsParser.ApplicationSettings.DiskLogLocation}");
                    ConfigureFileLogger(settingsParser.ApplicationSettings.DiskLogLocation, _loggerFactory);
                    _logger = _loggerFactory.CreateLogger<Program>();
                }

                // Validate settings.
                settingsParser.ValidateTransferSettings();
                settingsParser.ValidateApplicationSettings();

                // Create sftp client
                _dotNetSftpClient = new DotNetSftpClient(settingsParser.ConnectivitySettings, _logger);
                _dotNetSftpClient.CreateConnection();

                // Initiate transfer according to settings
                if (settingsParser.TransferSettings.TransferType == 'u')
                {
                    _dotNetSftpClient.Upload(settingsParser.TransferSettings);
                }
                else if (settingsParser.TransferSettings.TransferType == 'd')
                {
                    _dotNetSftpClient.Download(settingsParser.TransferSettings);
                }
                else
                    throw new ArgumentException($"Bad transfer-type '{settingsParser.TransferSettings.TransferType}' specified.");
            }
            catch (SettingsParsingException e)
            {
                // output some error message
                Console.Write("dotnetsftp.exe: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `dotnetsftp --help' for more information.");

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured.");
                throw;
            }
        }

        private static string GetAssemblyVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            return version;
        }

        public static SettingsParser ImportSettingsFromFile(string settingsFilePath, ILogger logger, string encryptionKeyFilePath = null)
        {
            logger.LogInformation($"Will read settings-file fom '{settingsFilePath}...'");
            
            // De-serialize the settings
            string deserializedSettingsText = null; 

            if (string.IsNullOrWhiteSpace(encryptionKeyFilePath))
            {
                // No encryption-key file given, so we'll assume the settings-file is un-encrypted.
                deserializedSettingsText = File.ReadAllText(settingsFilePath);
            }
            else
            {
                // Encryption-key file was provided, so we'll assume the settings-file is encrypted and, well, decrypt it.
                logger.LogInformation($"Will decrypt settings file {settingsFilePath} using the key found in {encryptionKeyFilePath}");
                string encryptionKey = File.ReadAllText(encryptionKeyFilePath);

                FileEncryptionUtility fileEncryptionUtility = new FileEncryptionUtility();
                deserializedSettingsText = fileEncryptionUtility.DecryptFile(settingsFilePath, encryptionKey);
            }

            SettingsParser deserializedSettings = JsonConvert.DeserializeObject<SettingsParser>(deserializedSettingsText);
            // Remove the settings-filepath argument from the settings. We don't want to include this in the referenced settings, they're only used for this import-purpose.
            deserializedSettings.ApplicationSettings.SettingsFilePath = null;
            deserializedSettings.ApplicationSettings.SettingsKeyFilePath = null;

            logger.LogInformation($"Imported settings from '{settingsFilePath}'");
            return deserializedSettings;
        }

        /// <summary>
        /// Will save the settings derived from the commandline-arguments into a file. This facilitates automatic creation and utilization of transfer-settings.
        /// </summary>
        /// <param name="settingsParser"></param>
        /// <param name="settingsFilePath"></param>
        /// <param name="logger"></param>
        /// <param name="encryptionKeyFilePath">Optional. If specified, an encryption-key will be generated and stored to this file, and will be used to encrypt the settings file.</param>
        public static void SaveSettingsToFile(SettingsParser settingsParser, string settingsFilePath, ILogger logger, string encryptionKeyFilePath = null)
        {
            logger.LogInformation($"Will save settings-file to '{settingsFilePath}...'");

            // First, remove the settings-filepath argument from the settings. We don't want to include this in the saved settings.
            settingsParser.ApplicationSettings.SettingsFilePath = null;
            settingsParser.ApplicationSettings.SettingsKeyFilePath = null;

            // Now go ahead and serialize the settings
            string serializedSettings = Newtonsoft.Json.JsonConvert.SerializeObject(settingsParser, Formatting.Indented);

            if (!string.IsNullOrWhiteSpace(encryptionKeyFilePath))
            {
                logger.LogInformation($"Will encrypt settings file {settingsFilePath}");
                string encryptionKey = null;

                bool encryptionFileExists = File.Exists(encryptionKeyFilePath);
                if (encryptionFileExists)
                {
                    encryptionKey = File.ReadAllText(encryptionKeyFilePath);
                }
                else
                {
                    // no encryption file exists, generate a new encryption key, save it, then encrypt the settings.
                    logger.LogInformation($"Generating new encryption key...");
                    encryptionKey = Guid.NewGuid().ToString().Substring(0, 8);
                    logger.LogInformation($"Saving encryption key to '{encryptionKeyFilePath}'...");
                    File.WriteAllText(encryptionKeyFilePath, encryptionKey);
                }

                FileEncryptionUtility fileEncryptionUtility = new FileEncryptionUtility();
                fileEncryptionUtility.EncryptFile(serializedSettings, settingsFilePath, encryptionKey);
            }
            else
            {
                // Save the settings into an unencrypted file.
                File.WriteAllText(settingsFilePath, serializedSettings);
            }

            logger.LogInformation($"Saved settings-file to '{settingsFilePath}'");
        }

        private static void ConfigureFileLogger(string fileLogLocation, ILoggerFactory loggerFactory)
        {
            string logFileName = "dotnetsftp_log";
            string logFilesPath = string.IsNullOrWhiteSpace(fileLogLocation) ?
                /* true - default log-location to application exe-dir */ Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Logs"
                :
                /* false - use the provided log-location */ fileLogLocation;

            // instantiate and configure logging. Using serilog here, to log to console and a text-file.
            Serilog.ILogger loggerConfig = new Serilog.LoggerConfiguration()
                .WriteTo.File($@"{logFilesPath}\{logFileName}.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();
            loggerFactory.AddSerilog(loggerConfig);

            Console.WriteLine($"Logging to '{logFilesPath}\\{logFileName}.txt");
        }

        /// <summary>
        /// Add any external log-providers, that may reside in assemblies in the executing directory.
        /// The following requirements must be honored:
        /// - is that they inherit from Microsoft.Extensions.Logging.ILoggerProvider.
        /// - they have a default parameterless constructor, so they may be invoked without knowledge of their internals (if configuration is needed, this should be done via config-files).
        /// - and that they are - specifically - .Net 4.6.1 versioned.
        /// </summary>
        /// <param name="loggerFactory"></param>
        private static void AddAnyExternalLogProviders(ILoggerFactory loggerFactory)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string dll in Directory.GetFiles(path ?? throw new InvalidOperationException(), "*.dll"))
            {
                Assembly assemblyFromDll = Assembly.LoadFile(dll);

                if (assemblyFromDll.FullName == Assembly.GetExecutingAssembly().FullName)
                    continue; // Skip the current executing assembly.

                if (assemblyFromDll.FullName.ToLower().Contains("microsoft.extensions.logging"))
                    continue; // skip any Microsoft-related implementations.

                if (assemblyFromDll.FullName.ToLower().Contains("serilog.extensions.logging"))
                    continue; // skip the serilog.extensions already used within this project.

                var allILoggerproviderImplementations = assemblyFromDll.GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(ILoggerProvider)));
                foreach (var loggerProvider in allILoggerproviderImplementations)
                {
                    ILoggerProvider loggerProviderInstance = (ILoggerProvider)Activator.CreateInstance(loggerProvider);
                    loggerFactory.AddProvider(loggerProviderInstance);
                }
            }
        }

        private static void ShowHelpIfArgumentsSpecifyItSo(bool showHelp, OptionSet options)
        {
            if (showHelp)
            {
                Console.WriteLine("Usage: dotnetsftp.exe [OPTIONS]");
                Console.WriteLine("Transfer files via sftp.");
                Console.WriteLine();

                // output the options
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);

                Environment.Exit(0);
            }
        }

        private static void WriteErrorMessageAndQuitIfNoArgumentsGiven(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Try `dotnetsftp --help' for more information.");
                Environment.Exit(-1);
            }
        }
    }
}