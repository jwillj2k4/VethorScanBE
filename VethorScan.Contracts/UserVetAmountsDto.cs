namespace VethorScan.Contracts
{
    public class UserVetAmountsDto
    {
        public virtual decimal TransactionsPerSecond { get; set; } = 50m;
        public virtual decimal UserVetAmount { get; set; }
        public virtual decimal CurrentThorPrice { get; set; } = 0.5m;
        public virtual long CirculatingSupply { get; set; }
        public SplitType Split { get; set; }
    }
}