namespace VethorScan.Contracts
{
    public class UserVetResultDto
    {
        public VetResultDto VetResultDto { get; set; }
        public VetThorResultDto VetThorResultDto { get; set; }
        
        public UserVetResultDto()
        {
            VetResultDto = new VetResultDto();
            VetThorResultDto = new VetThorResultDto();
        }
    }
}
