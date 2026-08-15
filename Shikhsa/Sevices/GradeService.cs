using Shikhsa.Repository;
using Shikhsa.ViewModels;

namespace Shikhsa.Sevices
{
    public class GradeService
    {
        private readonly ReportCardRepository _repository;

        public GradeService(ReportCardRepository repository)
        {
            _repository = repository;
        }

        //public async Task<List<GradeRangeVM>> LoadGradeTableAsync(
        //    int batchId,
        //    int classId,
        //    int termId)
        //{
        //    return await _repository.GetGradesAsync(
        //        batchId,
        //        classId,
        //        termId);
        //}

        public string GetGrade(
            decimal percentage,
            List<GradeRangeVM> grades)
        {
            var grade = grades.FirstOrDefault(x =>
                percentage >= x.MinPercentage &&
                percentage <= x.MaxPercentage);

            return grade?.Grade ?? "-";
        }
        public async Task<List<GradeRangeVM>>LoadGradeTableAsync(
        int batchId,
        int classId,
        int termId)
        {
            var data = await _repository
                .GetGradeRangesAsync(
                    batchId,
                    classId,
                    termId);

            return data
                .Select(x => new GradeRangeVM
                {
                    BatchId = x.BatchId,
                    ClassId = x.ClassId,
                    TermId = x.TermId,
                    MinPercentage = x.MinPercentage,
                    MaxPercentage = x.MaxPercentage,
                    Grade = x.Grade,
                    Description = x.Description
                })
                .ToList();
        }
    }
}
