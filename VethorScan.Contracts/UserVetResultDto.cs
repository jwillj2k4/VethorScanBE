namespace VethorScan.Contracts
{
    public class UserVetResultDto
    {
        public VetResultDto VetResultDto { get; set; }
        public VeThorResultDto VeThorBaseResultDto { get; set; }
        
        public UserVetResultDto()
        {
            VetResultDto = new VetResultDto();
            VeThorBaseResultDto = new VeThorResultDto();
        }
    }
}
