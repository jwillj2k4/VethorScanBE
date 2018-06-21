namespace VethorScan.Contracts
{
    public class VetInformationDto
    {
        public long TransactionsPerSecond { get; set; }
        public long TotalVetAmount { get; set; }
        public long TotalCirculatingSupply { get; set; }
        public double PriceOfVet { get; set; }
        public double PriceofThor { get; set; }

    }
}