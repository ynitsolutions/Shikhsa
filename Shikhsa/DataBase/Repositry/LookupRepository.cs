using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
namespace Shikhsa.Repositories
{
    public class LookupRepository 
    {
        private readonly ApplicationDbContext _context;

        public LookupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Batches>> GetBatchesAsync()
        {
            return await _context.Batches
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.BatchId)
                .ToListAsync();
        }

        public async Task<List<StaffMaster>> GetStaffsAsync()
        {
            return await _context.StaffMasters
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FirstName).ThenBy(x=>x.MiddleName).ThenBy(x=>x.LastName)
                .ToListAsync();
        }

        public async Task<StaffMaster?> GetStaffAsync(long staffId)
        {
            return await _context.StaffMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StaffId == staffId && x.IsActive);
        }

        public async Task<ClassTeacher?> GetClassTeacherAsync(long staffId, int batchId)
        {
            return await _context.ClassTeachers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.StaffId == staffId &&
                    x.BatchId == batchId &&
                    x.IsActive);
        }

        public async Task<List<DataListItem>> GetClassesAsync(int batchId, long staffId)
        {
            var classIds = await _context.ClassTeacherSubjectAssignments
                .AsNoTracking()
                .Where(x =>
                    x.BatchId == batchId &&
                    x.StaffId == staffId &&
                    x.IsActive)
                .Select(x => x.ClassId)
                .Distinct()
                .ToListAsync();

            return await _context.DataListItems
                .AsNoTracking()
                .Where(x => classIds.Contains(x.DataListItemId))
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<DataListItem>> GetSectionsAsync(int batchId, int classId, long staffId)
        {
            var sectionIds = await _context.ClassTeacherSubjectAssignments
                .AsNoTracking()
                .Where(x =>
                    x.BatchId == batchId &&
                    x.ClassId == classId &&
                    x.StaffId == staffId &&
                    x.IsActive)
                .Select(x => x.SectionId)
                .Distinct()
                .ToListAsync();

            return await _context.DataListItems
                .AsNoTracking()
                .Where(x => sectionIds.Contains(x.DataListItemId))
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }
        public async Task<StaffMaster?> GetStaffByUserIdAsync(string userId)
        {
            return await _context.StaffMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.IsActive);
        }
        public async Task<List<Tbl_Students>> GetStudentsAsync(
    int batchId,
    int classId,
    int sectionId)
        {
            int Admitted = _context.DataListItems.Where(x => x.DataListItemValue == "Admitted" && x.IsActive).Select(x => x.DataListItemId).FirstOrDefault();
            return await _context.Tbl_Students
                .AsNoTracking()
                .Where(x =>
                   x.AdmitBatchId == batchId &&
                    x.AdmitClassId == classId &&
                    x.AdmitSectionId == sectionId &&
                    x.IsActive && x.Status == Admitted)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.MiddleName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
        }
        public async Task<bool> IsClassTeacherAsync(
    int batchId,
    int classId,
    int sectionId,
    long staffId)
        {
            return await _context.ClassTeachers
                .AsNoTracking()
                .AnyAsync(x =>
                    x.BatchId == batchId &&
                    x.ClassId == classId &&
                    x.SectionId == sectionId &&
                    x.StaffId == staffId &&
                    x.IsActive);
        }
    }
}