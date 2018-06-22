namespace VethorScan.Contracts
{
    public class VetInformationDto
    {
        public long TransactionsPerSecond { get; set; }
        public long TotalVetAmount { get; set; }
        public long TotalCirculatingSupply { get; set; }
        public double CurrentfVetPrice { get; set; }
        public double CurrentThorPrice { get; set; }

    }
}