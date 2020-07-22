using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNblitz.Data.Models
{
    public class Wallet
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string WalletId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string AdminKey { get; set; }
        public string InvoiceKey { get; set; }
        public string ReadonlyKey { get; set; }

        public User User { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
