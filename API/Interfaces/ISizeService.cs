using Repositories.Models;

namespace API.Interfaces
{
    public interface ISizeService
    {
        Task<Size> GetSizeByName(string name);
    }
}
