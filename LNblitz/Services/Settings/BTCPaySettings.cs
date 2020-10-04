using System;
using System.ComponentModel;

namespace LNblitz.Services.Settings
{
    public class BtcPaySettings : SettingsBase
    {
        [DisplayName("API Key")]
        public string ApiKey { get; set; }
        [DisplayName("Store ID")]
        public string StoreId { get; set; }
        public string Endpoint { get; set; }
    }
}
