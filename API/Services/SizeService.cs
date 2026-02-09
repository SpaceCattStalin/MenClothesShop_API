using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;

namespace API.Services
{
    public class SizeService : ISizeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SizeService> _logger;
        public SizeService(AppDbContext context, ILogger<SizeService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Size> GetSizeByName(string name)
        {
            try
            {
                return await _context.Sizes.FirstOrDefaultAsync(s => s.Name.Equals(name));
            }
            catch (Exception ex)
            {
                _logger.LogError("SIZE_SERVICE_ERR", $"Error {ex.Message}");
                return null;
            }
        }
    }
}
