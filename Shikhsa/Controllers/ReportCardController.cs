using Microsoft.AspNetCore.Mvc;
using Shikhsa.Attributes;
using Shikhsa.Repositories;
using Shikhsa.Repository;
using Shikhsa.Sevices;
using Shikhsa.ViewModels;
using System.IO.Compression;

namespace Shikhsa.Controllers
{
    public class ReportCardController : Controller
    {
        private readonly ReportCardService _service;
        private readonly ReportCardPdfService _pdfService;
        private readonly ReportCardRepository _repository;
        private readonly LookupRepository _lookupRepository;
        public ReportCardController(
            ReportCardService service,
            ReportCardPdfService pdfService,
            ReportCardRepository repository,
            LookupRepository lookupRepository)
        {
            _service = service;
            _pdfService = pdfService;
            _repository = repository;
            _lookupRepository = lookupRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Preview(long studentId,int batchId,int examCategoryId)
        {
            var vm = await _service.GetAsync(
                studentId,
                batchId,
                examCategoryId);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> Download(long studentId,int batchId,int examCategoryId)
        {
            var vm = await _service.GetAsync(
                studentId,
                batchId,
                examCategoryId);

            if (vm == null)
                return NotFound();

            var pdf = _pdfService.Generate(vm);

            return File(
                pdf,
                "application/pdf",
                $"ReportCard_{studentId}.pdf");
        }
        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> DownloadSelected(
     List<long> studentIds,
     int batchId,
     int examCategoryId)
        {
            if (studentIds == null || !studentIds.Any())
                return BadRequest("Please select at least one student.");

            var reports = await _service.GetBulkAsync(
                studentIds,
                batchId,
                examCategoryId);

            if (reports == null || !reports.Any())
                return NotFound("No report cards found.");

            using var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(
                zipStream,
                ZipArchiveMode.Create,
                true))
            {
                foreach (var report in reports)
                {
                    var pdf = _pdfService.Generate(report);

                    var studentName = report.Student.StudentName
                        .Replace("/", "_")
                        .Replace("\\", "_")
                        .Replace(":", "_")
                        .Replace("*", "_")
                        .Replace("?", "_")
                        .Replace("\"", "_")
                        .Replace("<", "_")
                        .Replace(">", "_")
                        .Replace("|", "_");

                    var entry = archive.CreateEntry(
                        $"{studentName}_{report.Student.StudentId}.pdf");

                    using var entryStream = entry.Open();

                    await entryStream.WriteAsync(pdf);
                }
            }

            return File(
                zipStream.ToArray(),
                "application/zip",
                "Selected_ReportCards.zip");
        }
        //    [HttpGet]
        //    [SkipPermission]
        //    public async Task<IActionResult> DownloadClass(int batchId,int classId,int sectionId,int examCategoryId)
        //    {
        //        //var reports = await _service.GetClassReportCardsAsync(
        //        //    batchId,
        //        //    classId,
        //        //    sectionId,
        //        //    examCategoryId);

        //        //// Same ZIP Logic
        //        ///
        //        var studentIds = await _repository.GetStudentIdsAsync(
        //batchId,
        //classId,
        //sectionId);

        //        var reports = await _service.GetBulkAsync(
        //            studentIds,
        //            batchId,
        //            examCategoryId);

        //        return Ok();
        //    }

        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> DownloadClass(
    int batchId,
    int classId,
    int sectionId,
    int examCategoryId)
        {
            var studentIds = await _repository.GetStudentIdsAsync(
                batchId,
                classId,
                sectionId);

            if (studentIds == null || !studentIds.Any())
                return NotFound("No students found.");

            var reports = await _service.GetBulkAsync(
                studentIds,
                batchId,
                examCategoryId);

            if (reports == null || !reports.Any())
                return NotFound("No report cards found.");

            using var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(
                zipStream,
                ZipArchiveMode.Create,
                true))
            {
                foreach (var report in reports)
                {
                    var pdf = _pdfService.Generate(report);

                    var studentName = report.Student.StudentName
                        .Replace("/", "_")
                        .Replace("\\", "_")
                        .Replace(":", "_")
                        .Replace("*", "_")
                        .Replace("?", "_")
                        .Replace("\"", "_")
                        .Replace("<", "_")
                        .Replace(">", "_")
                        .Replace("|", "_");

                    var entry = archive.CreateEntry(
                        $"{studentName}_{report.Student.StudentId}.pdf");

                    using var entryStream = entry.Open();

                    await entryStream.WriteAsync(pdf);
                }
            }

            return File(
                zipStream.ToArray(),
                "application/zip",
                "ReportCards.zip");
        }
        [HttpGet]
        public async Task<IActionResult> ReportCard()
        {
            var vm = new ReportCardFilterVM();

            vm.Batches = await _lookupRepository.GetBatchesAsync();

            vm.ExamCategories = await _lookupRepository.GetExamCategoriesAsync();

            vm.Classes = _lookupRepository.GetDataListItems("Class");
            vm.Sections = _lookupRepository.GetDataListItems("Section");
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportCard(ReportCardFilterVM vm)
        {
            vm.Batches = await _lookupRepository.GetBatchesAsync();

            vm.ExamCategories = await _lookupRepository.GetExamCategoriesAsync();

            vm.Classes = _lookupRepository.GetDataListItems("Class");

            if (!vm.BatchId.HasValue ||!vm.ClassId.HasValue ||!vm.SectionId.HasValue)
            {
                ModelState.AddModelError("","Please select Batch, Class and Section.");

                return View(vm);
            }

            vm.Sections = _lookupRepository.GetDataListItems("Section");

            vm.Students =await _service.GetStudentListAsync(vm.BatchId.Value,vm.ClassId.Value,vm.SectionId.Value);

            return View(vm);
        }
    }
}
