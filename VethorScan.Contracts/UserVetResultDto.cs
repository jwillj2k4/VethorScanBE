namespace VethorScan.Contracts
{
    public class UserVetResultDto
    {
        public VetResultDto VetResultDto { get; set; }
        public VeThorResultDto VeThorResultDto { get; set; }
        
        public UserVetResultDto()
        {
            VetResultDto = new VetResultDto();
            VeThorResultDto = new VeThorResultDto();
        }
    }
}
