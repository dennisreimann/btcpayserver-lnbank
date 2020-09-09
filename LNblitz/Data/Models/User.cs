using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNblitz.Data.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }
        
        public string BTCPayUserId { get; set; }

        public string AccessToken { get; set; }

        public List<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
