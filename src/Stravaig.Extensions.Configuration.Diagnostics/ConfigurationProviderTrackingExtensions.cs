using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Stravaig.Extensions.Configuration.Diagnostics
{
    /// <summary>
    /// Extension methods for tracking where a value came from in the configuration.
    /// </summary>
    public static class ConfigurationProviderTrackingExtensions
    {
        /// <summary>
        /// Logs the source provider(s) for the given configuration key as Trace.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="configRoot">The configuration source.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="compressed">If true, skips logging provider details that have no value for the key.</param>
        /// <param name="options">The options to use when logging, if not set the <see cref="ConfigurationDiagnosticsOptions.GlobalOptions"/> are used.</param>
        public static void LogConfigurationKeySourceAsTrace(this ILogger logger, IConfigurationRoot configRoot,
            string key, bool compressed = false, ConfigurationDiagnosticsOptions options = null)
        {
            logger.LogConfigurationKeySource(LogLevel.Trace, configRoot, key, compressed, options);
        }

        /// <summary>
        /// Logs the source provider(s) for the given configuration key as Debug.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="configRoot">The configuration source.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="compressed">If true, skips logging provider details that have no value for the key.</param>
        /// <param name="options">The options to use when logging, if not set the <see cref="ConfigurationDiagnosticsOptions.GlobalOptions"/> are used.</param>
        public static void LogConfigurationKeySourceAsDebug(this ILogger logger, IConfigurationRoot configRoot,
            string key, bool compressed = false, ConfigurationDiagnosticsOptions options = null)
        {
            logger.LogConfigurationKeySource(LogLevel.Debug, configRoot, key, compressed, options);
        }

        /// <summary>
        /// Logs the source provider(s) for the given configuration key as Information.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="configRoot">The configuration source.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="compressed">If true, skips logging provider details that have no value for the key.</param>
        /// <param name="options">The options to use when logging, if not set the <see cref="ConfigurationDiagnosticsOptions.GlobalOptions"/> are used.</param>
        public static void LogConfigurationKeySourceAsInformation(this ILogger logger, IConfigurationRoot configRoot,
            string key, bool compressed = false, ConfigurationDiagnosticsOptions options = null)
        {
            logger.LogConfigurationKeySource(LogLevel.Information, configRoot, key, compressed, options);
        }

        /// <summary>
        /// Logs the source provider(s) for the given configuration key.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="level">The log level to write at.</param>
        /// <param name="configRoot">The configuration source.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="compressed">If true, skips logging provider details that have no value for the key.</param>
        /// <param name="options">The options to use when logging, if not set the <see cref="ConfigurationDiagnosticsOptions.GlobalOptions"/> are used.</param>
        public static void LogConfigurationKeySource(this ILogger logger, LogLevel level, IConfigurationRoot configRoot, string key, bool compressed = false, ConfigurationDiagnosticsOptions options = null)
        {
            if (options == null)
                options = ConfigurationDiagnosticsOptions.GlobalOptions;
            
            StringBuilder report;
            if (configRoot.Providers.Any())
                report = ReportProvidersWithKey(configRoot, key, compressed, options);
            else
                report = ReportNoProviders(key);
            
            logger.Log(level, report.ToString());
        }

        private static StringBuilder ReportNoProviders(string key)
        {
            var report = new StringBuilder("Cannot track ");
            report.Append(key);
            report.Append(". No configuration providers found.");
            return report;
        }

        private static StringBuilder ReportProvidersWithKey(IConfigurationRoot configRoot, string key, bool compressed,
            ConfigurationDiagnosticsOptions options)
        {
            bool found = false;
            bool obfuscate = options.ConfigurationKeyMatcher.IsMatch(key);
            var report = new StringBuilder();
            report.Append("Provider sources for value of ");
            report.Append(key);
            foreach (IConfigurationProvider provider in configRoot.Providers)
            {
                if (provider.TryGet(key, out string value))
                {
                    found = true;
                    report.AppendLine();
                    report.Append("* ");
                    report.Append(provider);
                    report.Append(" ==> ");
                    if (obfuscate)
                    {
                        value = options.Obfuscator.Obfuscate(value);
                        report.Append(value);
                    }
                    else
                    {
                        report.Append('"');
                        report.Append(value);
                        report.Append('"');
                    }
                }
                else if (!compressed)
                {
                    report.AppendLine();
                    report.Append("* ");
                    report.Append(provider);
                    report.Append(" ==> ");
                    report.Append("null");
                }
            }

            if (!found)
            {
                if (compressed)
                    report.Append(" were not found.");
                else
                {
                    report.AppendLine();
                    report.Append(key);
                    report.Append(" not found in any provider.");
                }
            }

            return report;
        }
    }
}