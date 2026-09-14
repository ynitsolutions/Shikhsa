using Shikhsa.Data;
using Shikhsa.Models.Certificate;
using Microsoft.EntityFrameworkCore;

namespace Shikhsa.DataBase.Repositry
{
    public class CertificateTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CertificateTypeMaster>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.CertificateTypes.AsQueryable();
            if (activeOnly) query = query.Where(x => x.IsActive);

            return await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.TypeName).ToListAsync();
        }

        public async Task<CertificateTypeMaster?> GetByIdAsync(long id)
        {
            return await _context.CertificateTypes.FindAsync(id);
        }

        public async Task<SaveResult> SaveAsync(CertificateTypeMaster model)
        {
            try
            {
                if (model.CertificateTypeId == 0)
                {
                    _context.CertificateTypes.Add(model);
                }
                else
                {
                    var existing = await _context.CertificateTypes.FindAsync(model.CertificateTypeId);
                    if (existing == null)
                    {
                        return new SaveResult { Status = 0, Message = "Certificate type not found." };
                    }

                    existing.TypeName = model.TypeName;
                    existing.TypeCode = model.TypeCode;
                    existing.AppliesTo = model.AppliesTo;
                    existing.IsActive = model.IsActive;
                    existing.DisplayOrder = model.DisplayOrder;
                    model.CertificateTypeId = existing.CertificateTypeId;
                }

                await _context.SaveChangesAsync();
                return new SaveResult { Status = 1, Message = "Certificate type saved.", Id = model.CertificateTypeId };
            }
            catch (Exception ex)
            {
                return new SaveResult { Status = 0, Message = ex.Message };
            }
        }

        public async Task<SaveResult> DeleteAsync(long id)
        {
            var inUse = await _context.CertificateTemplates.AnyAsync(t => t.CertificateTypeId == id);
            if (inUse)
            {
                return new SaveResult { Status = 0, Message = "Templates already use this type - deactivate it instead of deleting." };
            }

            var existing = await _context.CertificateTypes.FindAsync(id);
            if (existing == null)
            {
                return new SaveResult { Status = 0, Message = "Certificate type not found." };
            }

            _context.CertificateTypes.Remove(existing);
            await _context.SaveChangesAsync();
            return new SaveResult { Status = 1, Message = "Certificate type deleted." };
        }
    }
}
