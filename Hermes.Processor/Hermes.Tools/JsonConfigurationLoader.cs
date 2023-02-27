using log4net;
using Microsoft.Extensions.Configuration;

namespace Hermes.Tools
{
    public static class JsonConfigurationLoader
    {
        private static ILog _logger = LogManager.GetLogger(typeof(JsonConfigurationLoader));
        public static bool TryBuildConfigurationStore<T>(string section, out T settings, string settingsFile = "appsettings.json")
        {
            try
            {
                IConfiguration config = GetConfiguration(settingsFile);
                settings = config.GetSection(section).Get<T>();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load section {section} from {settingsFile}.", ex);
                settings = default(T);
                return false;
            }
        }

        public static IConfiguration GetConfiguration(string settingsFile = "appsettings.json")
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(settingsFile, optional: false);
                return builder.Build();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load configuration from {settingsFile}.", ex);
                throw;
            }
        }

        public static Settings LoadConfiguration()
        {
            _logger.Debug("Loading configuration");
            if (!TryBuildConfigurationStore<ExchangeSettings>(ExchangeSettings.SectionName, out var exchangeSettings)) throw new MissingConfigurationException(section: ExchangeSettings.SectionName);
            if (!TryBuildConfigurationStore<TradingSettings>(TradingSettings.SectionName, out var tradingSettings)) throw new MissingConfigurationException(section: ExchangeSettings.SectionName);
            if (!TryBuildConfigurationStore<BacktestingSettings>(BacktestingSettings.SectionName, out var backtestingSettings)) throw new MissingConfigurationException(section: ExchangeSettings.SectionName);

            return new Settings { ExchangeSettings = exchangeSettings, TradingSettings = tradingSettings, BacktestingSettings = backtestingSettings };
        }
    }
}
