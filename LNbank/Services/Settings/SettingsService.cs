using System;
using System.Threading.Tasks;
using LNbank.Data;

namespace LNbank.Services.Settings
{
    public interface ISettings
    {
        AppSettings App { get; }
        BtcPaySettings BtcPay { get; }
        Task SaveAsync();
    }

    public class SettingsService : ISettings
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly Lazy<AppSettings> _appSettings;
        public AppSettings App => _appSettings.Value;

        private readonly Lazy<BtcPaySettings> _btcpaySettings;
        public BtcPaySettings BtcPay => _btcpaySettings.Value;
        public SettingsService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _appSettings = new Lazy<AppSettings>(CreateSettings<AppSettings>);
            _btcpaySettings = new Lazy<BtcPaySettings>(CreateSettings<BtcPaySettings>);
        }

        public async Task SaveAsync()
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            if (_appSettings.IsValueCreated)
                _appSettings.Value.Save(dbContext);

            if (_btcpaySettings.IsValueCreated)
                _btcpaySettings.Value.Save(dbContext);

            await dbContext.SaveChangesAsync();
        }

        private T CreateSettings<T>() where T : SettingsBase, new()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var settings = new T();
            settings.Load(dbContext);
            return settings;
        }

        public bool NeedsSettings =>
            string.IsNullOrEmpty(BtcPay.ApiKey) ||
            string.IsNullOrEmpty(BtcPay.StoreId) ||
            string.IsNullOrEmpty(BtcPay.Endpoint);
    }
}
