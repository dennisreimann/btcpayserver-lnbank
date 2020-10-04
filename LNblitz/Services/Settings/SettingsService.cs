using System;
using System.Threading.Tasks;
using LNblitz.Data;

namespace LNblitz.Services.Settings
{
    public interface ISettings
    {
        AppSettings App { get; }
        BtcPaySettings BtcPay { get; }
        Task SaveAsync();
    }

    public class SettingsService : ISettings
    {
        private readonly Lazy<AppSettings> _appSettings;
        public AppSettings App => _appSettings.Value;

        private readonly Lazy<BtcPaySettings> _btcpaySettings;
        public BtcPaySettings BtcPay => _btcpaySettings.Value;

        private readonly ApplicationDbContext _dbContext;
        public SettingsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _appSettings = new Lazy<AppSettings>(CreateSettings<AppSettings>);
            _btcpaySettings = new Lazy<BtcPaySettings>(CreateSettings<BtcPaySettings>);
        }

        public async Task SaveAsync()
        {
            if (_appSettings.IsValueCreated)
                _appSettings.Value.Save(_dbContext);

            if (_btcpaySettings.IsValueCreated)
                _btcpaySettings.Value.Save(_dbContext);

            await _dbContext.SaveChangesAsync();
        }

        private T CreateSettings<T>() where T : SettingsBase, new()
        {
            var settings = new T();
            settings.Load(_dbContext);
            return settings;
        }
    }
}
