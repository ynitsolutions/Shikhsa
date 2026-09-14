using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Certificate;

namespace Shikhsa.DataBase.Repositry
{
    public class GeneratedCertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public GeneratedCertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GeneratedCertificate?> GetByIdAsync(long id)
        {
            return await _context.GeneratedCertificates
                .Include(g => g.CertificateTemplate)
                .FirstOrDefaultAsync(g => g.GeneratedCertificateId == id);
        }

        public async Task<GeneratedCertificate> SaveAsync(GeneratedCertificate model)
        {
            model.CreatedDate = DateTime.UtcNow;
            _context.GeneratedCertificates.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<List<GeneratedCertificate>> GetIssuedDocumentsAsync(int subjectId, string subjectType)
        {
            return await _context.GeneratedCertificates
                .Where(x => x.SubjectId == subjectId
                         && x.SubjectType == subjectType
                         && x.IsActive)
                .Join(_context.CertificateTemplates,
                      gc => gc.CertificateTemplateId,
                      ct => ct.CertificateTemplateId,
                      (gc, ct) => new GeneratedCertificate
                      {
                          GeneratedCertificateId = gc.GeneratedCertificateId,
                          TemplateName = ct.TemplateName,
                          AddedDate = gc.AddedDate
                      })
                .OrderByDescending(x => x.AddedDate)
                .ToListAsync();
        }
    }
    public class CertificateTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CertificateTemplate>> GetAllAsync()
        {
            return await _context.CertificateTemplates
                .Include(t => t.CertificateTemplateCategories).Where(x=>x.IsActive==true)
                .OrderByDescending(t => t.AddedDate)
                .ToListAsync();
        }

        public async Task<CertificateTemplate?> GetByIdAsync(long id)
        {
            return await _context.CertificateTemplates
                .Include(t => t.CertificateTemplateCategories)
                .FirstOrDefaultAsync(t => t.CertificateTemplateId == id);
        }

        public async Task<SaveResult> SaveAsync(CertificateTemplate model,string username)
        {
            try
            {
                if (model.CertificateTemplateId == 0)
                {
                    model.AddedDate = DateTime.UtcNow;
                    model.AddedBy = username;
                    _context.CertificateTemplates.Add(model);
                }
                else
                {
                    var existing = await _context.CertificateTemplates
                        .FirstOrDefaultAsync(t => t.CertificateTemplateId == model.CertificateTemplateId);

                    if (existing == null)
                    {
                        return new SaveResult { Status = 0, Message = "Template not found." };
                    }

                    existing.TemplateName = model.TemplateName;
                    existing.TemplateCode = model.TemplateCode;
                    existing.CertificateType = model.CertificateType;
                    existing.Body = model.Body;
                    existing.UpdatedDate = DateTime.UtcNow;
                    existing.UpdatedBy = username;
                    model.CertificateTemplateId = existing.CertificateTemplateId;
                }

                await _context.SaveChangesAsync();

                return new SaveResult
                {
                    Status = 1,
                    Message = "Certificate template saved successfully.",
                    Id = model.CertificateTemplateId,
                };
            }
            catch (Exception ex)
            {
                return new SaveResult { Status = 0, Message = ex.Message };
            }
        }

        public async Task<SaveResult> DeleteAsync(long id,string userName)
        {
            var existing = await _context.CertificateTemplates.FindAsync(id);
            if (existing == null)
            {
                return new SaveResult { Status = 0, Message = "Template not found." };
            }
            existing.IsActive = !existing.IsActive;

            existing.UpdatedDate = DateTime.Now;
            existing.UpdatedBy = userName;

            _context.CertificateTemplates.Update(existing);
            //  _context.CertificateTemplates.Remove(existing);
            await _context.SaveChangesAsync();

            return new SaveResult { Status = 1, Message = "Template deleted successfully." };
        }
        public async Task<List<CertificateTemplate>> GetStudentCertificateTemplatesAsync(string AppliedTo= "Student")
        {
            return await _context.CertificateTemplates
                .Include(x => x.CertificateType)
                .Where(x =>
                    x.IsActive &&
                    x.CertificateType != null &&
                    x.CertificateType.IsActive &&
                    x.CertificateType.AppliesTo == AppliedTo)
                .OrderBy(x => x.CertificateType!.DisplayOrder)
                .ThenBy(x => x.TemplateName)
                .ToListAsync();
        }
        
    }

}
