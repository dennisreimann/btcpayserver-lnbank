namespace LNblitz.Data.Queries
{
    public class WalletQuery
    {
        public string UserId { get; set; }
        public string WalletId { get; set; }
        public bool IncludeTransactions { get; set; }
    }
}
