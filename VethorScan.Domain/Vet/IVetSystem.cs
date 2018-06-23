using System.Threading.Tasks;
using VethorScan.Contracts;

namespace VethorScan.Domain.Vet
{
    public interface IVetSystem
    {
        Task<VetMetaDataDto> GetVetMetadata();
    }
}