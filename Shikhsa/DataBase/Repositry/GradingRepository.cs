using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;

namespace Shikhsa.DataBase.Repositry
{
    public class GradingRepository  
    {
        private readonly ApplicationDbContext _context;

        public GradingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GradingCriteria>> GetAll()
        {
            return await _context.GradingCriteria
                .Include(x => x.Term)
                .Include(x => x.Class)
                .Include(x => x.Batch)
                .OrderBy(x => x.MinPercentage)
                .ToListAsync();
        }

        public async Task Save(GradingCriteria model)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task Update(GradingCriteria model)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var obj = await _context.GradingCriteria.FindAsync(id);

            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<GradingCriteria> Get(int id)
        {
            return await _context.GradingCriteria.FindAsync(id);
        }

        public async Task<ResponseModel> SaveBulkGradingCriteria(GradingCriteriaVM vm, string userId)
        {
            ResponseModel response = new ResponseModel();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                bool exists = await _context.GradingCriteria.AnyAsync(x =>
                    x.BatchId == vm.Criteria.BatchId &&
                    x.ClassId == vm.Criteria.ClassId &&
                    x.TermId == vm.Criteria.TermId &&
                    x.IsActive);

                if (exists)
                {
                    response.Status = 0;
                    response.Message = "Grading criteria already exists for selected Batch, Class and Term.";
                    return response;
                }

                foreach (var item in vm.GradeRanges)
                {
                    _context.GradingCriteria.Add(new GradingCriteria
                    {
                        BatchId = vm.Criteria.BatchId,
                        ClassId = vm.Criteria.ClassId,
                        TermId = vm.Criteria.TermId,

                        MinPercentage = item.MinPercentage,
                        MaxPercentage = item.MaxPercentage,
                        Grade = item.Grade,
                        Description = item.Description,

                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Status = 1;
                response.Message = "Grading criteria saved successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        //public bool ValidateRange(BulkGradingVM model)
        //{
        //    var list = model.Grades.OrderBy(x => x.MinPercentage).ToList();

        //    for (int i = 0; i < list.Count - 1; i++)
        //    {
        //        if (list[i].MaxPercentage >= list[i + 1].MinPercentage)
        //            return false;
        //    }

        //    return true;
        //}
    }
}
