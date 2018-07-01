namespace VethorScan.Contracts
{
    public class VeThorResultDto : UserProfitDto
    {
        /// <summary>
        /// The amount of vethor generated per day per vet
        /// </summary>
        public decimal AmountPerDay { get; set; }
    }
}