namespace VethorScan.Contracts
{
    public class UserVetAmountsDto
    {
        public decimal TransactionsPerSecond { get; set; } = 50m;
        public decimal UserVetAmount { get; set; }
        public decimal CurrentThorPrice { get; set; } = 0.5m;
    }
}