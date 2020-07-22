using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LNblitz.Data.Models
{
    public class User : IdentityUser
    {
        public List<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
