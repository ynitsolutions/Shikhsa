using Shikhsa.Data;
using Shikhsa.Models;
using Microsoft.EntityFrameworkCore; // Yeh hona zaroori hai EF Core ke ToListAsync ke liye

namespace Shikhsa.DataBase.Repositry
{
    public class CertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Certificates>> GetAllByTypeAsync(string type)
        {
            return await _context.Certificates
                .Where(c => c.Type == type)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Certificates?> GetByIdAsync(int id)
        {
            return await _context.Certificates.FindAsync(id);
        }

        public async Task<Certificates> CreateAsync(Certificates certificate)
        {
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task<Certificates?> UpdateAsync(int id, Certificates updated)
        {
            var existing = await _context.Certificates.FindAsync(id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(updated);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Certificates.FindAsync(id);
            if (existing == null) return false;

            _context.Certificates.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

