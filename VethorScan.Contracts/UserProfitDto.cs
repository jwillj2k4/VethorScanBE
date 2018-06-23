namespace VethorScan.Contracts
{
    public class UserProfitDto
    {
        public decimal ProfitPerDay { get; set; }
        public decimal ProfitPerWeek { get; set; }
        public decimal ProfitPerMonth { get; set; }
        public decimal ProfitPerYear { get; set; }
        public decimal Profit3Year { get; set; }
        public decimal Profit5Year { get; set; }
        public decimal Profit10Year { get; set; }
    }
}