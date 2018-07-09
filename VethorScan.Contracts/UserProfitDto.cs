namespace VethorScan.Contracts
{
    public class UserProfitDto
    {
        public NodeType NodeType { get; set; }

        public UserVetProjectedProfitDto VetVetProjectedProfits { get; set; }
        public UserVetThorProjectedProfitDto VeThorVetProjectedProfits { get; set; }

        public UserProfitDto()
        {
            VetVetProjectedProfits = new UserVetProjectedProfitDto();
            VeThorVetProjectedProfits = new UserVetThorProjectedProfitDto();
        }
    }
}