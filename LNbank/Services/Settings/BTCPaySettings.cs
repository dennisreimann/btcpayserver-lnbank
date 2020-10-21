using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LNbank.Services.Settings
{
    public class BtcPaySettings : SettingsBase
    {
        [DisplayName("API Key")]
        [Required]
        public string ApiKey { get; set; }
        [DisplayName("Store ID")]
        [Required]
        public string StoreId { get; set; }
        [DisplayName("Base URL")]
        [Required]
        public string Endpoint { get; set; }
    }
}
