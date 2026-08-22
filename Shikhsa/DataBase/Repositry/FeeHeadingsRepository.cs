using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Models.Payment;
using Shikhsa.ViewModels;
using System;

namespace Shikhsa.Repository
{
    public class FeeHeadingRepository
    {
        private readonly ApplicationDbContext _context;

        public FeeHeadingRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        #region frequency
        public async Task<List<FeeFrequency>> GetAllFrequencyAsync()
        {
            return await _context.FeeFrequencies
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<FeeFrequency>> GetActiveFrequencyAsync()
        {
            return await _context.FeeFrequencies
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FeeFrequency?> GetFrequencyByIdAsync(int id)
        {
            return await _context.FeeFrequencies
                .FirstOrDefaultAsync(x => x.FrequencyId == id);
        }

        public async Task<bool> IsDuplicateFrequencyAsync(string value, int frequencyId)
        {
            return await _context.FeeFrequencies.AnyAsync(x =>
                x.Value.ToLower() == value.ToLower()
                && x.FrequencyId != frequencyId);
        }

        public async Task<ResponseModel> FrequencySaveAsync(FeeFrequency model)
        {
            ResponseModel response = new();

            try
            {
                await _context.FeeFrequencies.AddAsync(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Frequency saved successfully.";
                response.Id = model.FrequencyId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> FrequencyUpdateAsync(FeeFrequency model)
        {
            ResponseModel response = new();

            try
            {
                _context.FeeFrequencies.Update(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Frequency updated successfully.";
                response.Id = model.FrequencyId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> DeleteFrequencyAsync(int id, string username)
        {
            ResponseModel response = new();

            try
            {
                var data = await _context.FeeFrequencies.FindAsync(id);

                if (data == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                // Toggle Active/Inactive
                data.IsActive = !data.IsActive;


                data.UpdatedBy = username;
                data.UpdatedDate = DateTime.Now;

                _context.FeeFrequencies.Update(data);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = data.FrequencyId;
                response.Message = data.IsActive
                    ? "Frequency activated successfully."
                    : "Frequency deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }
        #endregion
        #region Heading
        public async Task<List<FeeHeading>> GetAllFeeHeadingAsync()
        {
            return await _context.FeeHeadings
                .Include(x => x.Frequency)
                .OrderBy(x => x.FeeHeadingName)
                .ToListAsync();
        }

        public async Task<FeeHeading?> GetFeeHeadingByIdAsync(long id)
        {
            return await _context.FeeHeadings
                .FirstOrDefaultAsync(x => x.FeeHeadingId == id);
        }

        public async Task<bool> IsDuplicateFeeHeadingAsync(string feeHeadingName, long feeHeadingId)
        {
            return await _context.FeeHeadings.AnyAsync(x =>
                x.FeeHeadingName.ToLower() == feeHeadingName.ToLower()
                && x.FeeHeadingId != feeHeadingId);
        }

        public async Task<ResponseModel> SaveFeeHeadingAsync(FeeHeading model)
        {
            ResponseModel response = new();

            try
            {
                await _context.FeeHeadings.AddAsync(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Fee Heading saved successfully.";
                response.Id = model.FeeHeadingId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> UpdateFeeHeadingAsync(FeeHeading model)
        {
            ResponseModel response = new();

            try
            {
                _context.FeeHeadings.Update(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Fee Heading updated successfully.";
                response.Id = model.FeeHeadingId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseModel> DeleteFeeHeadingAsync(long id, string username)
        {
            ResponseModel response = new();

            try
            {
                var data = await _context.FeeHeadings.FindAsync(id);

                if (data == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                // Toggle Active/Inactive
                data.IsActive = !data.IsActive;


                data.UpdatedBy = username;
                data.UpdatedDate = DateTime.Now;

                _context.FeeHeadings.Update(data);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = data.FrequencyId;
                response.Message = data.IsActive
                    ? "Frequency activated successfully."
                    : "Frequency deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion

        #region Hostel Fee Plan


        //public async Task<List<HostelFeePlan>> GetAllHostelFeePlanAsync()
        //{
        //    return await _context.HostelFeePlans
        //        .Include(x => x.FeeHeading)
        //        .OrderByDescending(x => x.HostelFeePlanId)
        //        .ToListAsync();
        //}
        public async Task<List<HostelFeePlan>> GetAllHostelFeePlanAsync()
        {
            return await (
                from hfp in _context.HostelFeePlans.Include(x => x.FeeHeading).Include(x => x.Batch)
                join h in _context.DataListItems on hfp.HostelId equals h.DataListItemId into hJoin
                from h in hJoin.DefaultIfEmpty()
                join r in _context.DataListItems on hfp.RoomType equals r.DataListItemId into rJoin
                from r in rJoin.DefaultIfEmpty()
                join m in _context.DataListItems on hfp.MealPlan equals m.DataListItemId into mJoin
                from m in mJoin.DefaultIfEmpty()
                orderby hfp.HostelFeePlanId descending
                select new HostelFeePlan
                {
                    HostelFeePlanId = hfp.HostelFeePlanId,
                    FeeHeadingId = hfp.FeeHeadingId,
                    HostelId = hfp.HostelId,
                    RoomType = hfp.RoomType,
                    MealPlan = hfp.MealPlan,
                    HostelFee = hfp.HostelFee,
                    FeeHeading = hfp.FeeHeading,
                    Batch = hfp.Batch,
                    HostelName = h != null ? h.DataListItemText : "",
                    RoomTypeName = r != null ? r.DataListItemText : "",
                    MealPlanName = m != null ? m.DataListItemText : ""
                }
            ).ToListAsync();
        }

        public async Task<List<HostelFeePlan>> GetActiveHostelFeePlanAsync()
        {
            return await _context.HostelFeePlans
                .Include(x => x.FeeHeading)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.HostelFeePlanId)
                .ToListAsync();
        }

        public async Task<HostelFeePlan?> GetHostelFeePlanByIdAsync(long id)
        {
            return await _context.HostelFeePlans
                .FirstOrDefaultAsync(x => x.HostelFeePlanId == id);
        }

        public async Task<bool> IsDuplicateHostelFeePlanAsync(long feeHeadingId, int hostelId, string roomType, long hostelFeePlanId)
        {
            return await _context.HostelFeePlans.AnyAsync(x =>
                x.FeeHeadingId == feeHeadingId
                && x.HostelId == hostelId
               
                && x.HostelFeePlanId != hostelFeePlanId);
        }

        public async Task<ResponseModel> SaveUpdateHostelFeePlanAsync(HostelFeePlan model, string userName)
        {
            ResponseModel response = new();

            try
            {
                if (model.HostelFeePlanId == 0)
                {
                    // Insert
                    model.AddedBy = userName;
                    model.AddedDate = DateTime.Now;
                    model.IsActive = true;

                    await _context.HostelFeePlans.AddAsync(model);

                    response.Message = "Hostel Fee Plan saved successfully.";
                }
                else
                {
                    // Update
                    var existing = await _context.HostelFeePlans
                        .FirstOrDefaultAsync(x => x.HostelFeePlanId == model.HostelFeePlanId);

                    if (existing == null)
                    {
                        response.Status = 0;
                        response.Message = "Hostel Fee Plan not found.";
                        return response;
                    }

                    _context.Entry(existing).CurrentValues.SetValues(model);

                    existing.UpdatedBy = userName;
                    existing.UpdatedDate = DateTime.Now;

                    response.Message = "Hostel Fee Plan updated successfully.";
                }

                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = model.HostelFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> DeleteHostelFeePlanAsync(long id, string username)
        {
            ResponseModel response = new();

            try
            {
                var data = await _context.HostelFeePlans.FindAsync(id);

                if (data == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                // Toggle Active/Inactive
                data.IsActive = !data.IsActive;

                data.UpdatedBy = username;
                data.UpdatedDate = DateTime.Now;

                _context.HostelFeePlans.Update(data);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = data.HostelFeePlanId;
                response.Message = data.IsActive
                    ? "Hostel Fee Plan activated successfully."
                    : "Hostel Fee Plan deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion

        #region Transport Fee Plan

        //public async Task<List<TransportFeePlan>> GetAllTransportFeePlanAsync()
        //{
        //    return await _context.TransportFeePlans
        //        .Include(x => x.FeeHeading)
        //        .Include(x => x.Batch)  
        //        .OrderByDescending(x => x.TransportFeePlanId)
        //        .ToListAsync();
        //}
        public async Task<List<TransportFeePlan>> GetAllTransportFeePlanAsync()
        {
            var list = await _context.TransportFeePlans
                .Include(x => x.FeeHeading)
                .Include(x => x.Batch)
                .OrderByDescending(x => x.TransportFeePlanId)
                .ToListAsync();

            var transportIds = list.Select(x => x.TransportId).Distinct().ToList();

            var transportNames = await _context.DataListItems
                .Where(d => transportIds.Contains(d.DataListItemId))
                .ToDictionaryAsync(d => d.DataListItemId, d => d.DataListItemText);

            foreach (var item in list)
            {
                item.TransportName = transportNames.TryGetValue(item.TransportId, out var name) ? name : null;
            }

            return list;
        }

        public async Task<List<TransportFeePlan>> GetActiveTransportFeePlanAsync()
        {
            return await _context.TransportFeePlans
                .Include(x => x.FeeHeading)
                .Include(x => x.Batch)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.TransportFeePlanId)
                .ToListAsync();
        }

        public async Task<TransportFeePlan?> GetTransportFeePlanByIdAsync(long id)
        {
            return await _context.TransportFeePlans
                .FirstOrDefaultAsync(x => x.TransportFeePlanId == id);
        }

        public async Task<bool> IsDuplicateTransportFeePlanAsync(long feeHeadingId, int transportId, int academicYear, long transportFeePlanId)
        {
            return await _context.TransportFeePlans.AnyAsync(x =>
                x.FeeHeadingId == feeHeadingId
                && x.TransportId == transportId
                && x.BatchId == academicYear
                && x.TransportFeePlanId != transportFeePlanId);
        }

        public async Task<ResponseModel> SaveUpdateTransportFeePlanAsync(TransportFeePlan model, string userName)
        {
            ResponseModel response = new();

            try
            {
                if (model.TransportFeePlanId == 0)
                {
                    // Insert
                    model.AddedBy = userName;
                    model.AddedDate = DateTime.Now;
                    model.IsActive = true;

                    await _context.TransportFeePlans.AddAsync(model);

                    response.Message = "Transport Fee Plan saved successfully.";
                }
                else
                {
                    // Update
                    var existing = await _context.TransportFeePlans
                        .FirstOrDefaultAsync(x => x.TransportFeePlanId == model.TransportFeePlanId);

                    if (existing == null)
                    {
                        response.Status = 0;
                        response.Message = "Transport Fee Plan not found.";
                        return response;
                    }

                    _context.Entry(existing).CurrentValues.SetValues(model);

                    existing.UpdatedBy = userName;
                    existing.UpdatedDate = DateTime.Now;

                    response.Message = "Transport Fee Plan updated successfully.";
                }

                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = model.TransportFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> DeleteTransportFeePlanAsync(long id, string username)
        {
            ResponseModel response = new();

            try
            {
                var data = await _context.TransportFeePlans.FindAsync(id);

                if (data == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                // Toggle Active/Inactive
                data.IsActive = !data.IsActive;

                data.UpdatedBy = username;
                data.UpdatedDate = DateTime.Now;

                _context.TransportFeePlans.Update(data);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = data.TransportFeePlanId;
                response.Message = data.IsActive
                    ? "Transport Fee Plan activated successfully."
                    : "Transport Fee Plan deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion
        #region Tuition Fee Plan

        //public async Task<List<TuitionFeePlan>> GetAllTuitionFeePlanAsync()
        //{
        //    return await _context.TuitionFeePlans
        //        .Include(x => x.FeeHeading)
        //        .OrderByDescending(x => x.TuitionFeePlanId)
        //        .ToListAsync();
        //}
        public async Task<List<TuitionFeePlan>> GetAllTuitionFeePlanAsync()
        {
            return await _context.TuitionFeePlans
                .Include(x => x.FeeHeading)
                .Include(x => x.Batch)
                .Select(x => new TuitionFeePlan
                {
                    TuitionFeePlanId = x.TuitionFeePlanId,
                    FeeHeadingId = x.FeeHeadingId,
                    ClassId = x.ClassId,
                    FeeValue = x.FeeValue,
                    FeeHeading = x.FeeHeading,
                    Batch = x.Batch,
                    ClassName = _context.DataListItems
                        .Where(d => d.DataListItemId == x.ClassId)
                        .Select(d => d.DataListItemText)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.TuitionFeePlanId)
                .ToListAsync();
        }
        public async Task<List<TuitionFeePlan>> GetActiveTuitionFeePlanAsync()
        {
            return await _context.TuitionFeePlans
                .Include(x => x.FeeHeading)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.TuitionFeePlanId)
                .ToListAsync();
        }

        public async Task<TuitionFeePlan?> GetTuitionFeePlanByIdAsync(long id)
        {
            return await _context.TuitionFeePlans
                .FirstOrDefaultAsync(x => x.TuitionFeePlanId == id);
        }

     

        public async Task<ResponseModel> SaveOrUpdateTuitionFeePlanAsync(TuitionFeePlan model, string userName)
{
    ResponseModel response = new();

    try
    {
        if (model.ClassIds == null || !model.ClassIds.Any())
        {
            response.Status = 0;
            response.Message = "Please select at least one class.";
            return response;
        }

        foreach (var classId in model.ClassIds.Distinct())
        {
            bool duplicate = await _context.TuitionFeePlans.AnyAsync(x =>
                x.ClassId == classId &&
                x.FeeHeading == model.FeeHeading &&
                x.BatchId == model.BatchId &&
                x.TuitionFeePlanId != model.TuitionFeePlanId);

            if (duplicate)
                continue;

            if (model.TuitionFeePlanId == 0)
            {
                TuitionFeePlan entity = new TuitionFeePlan
                {
                    ClassId = classId,
                    FeeHeadingId = model.FeeHeadingId,
                    FeeValue = model.FeeValue,
                    BatchId = model.BatchId,
                    Medium = model.Medium,

                    AddedBy = userName,
                   // AddedDate = DateTime.Now,
                    IsActive = true
                };

                _context.TuitionFeePlans.Add(entity);
            }
            else
            {
                var entity = await _context.TuitionFeePlans
                    .FirstOrDefaultAsync(x => x.TuitionFeePlanId == model.TuitionFeePlanId);

                if (entity == null)
                    continue;

                entity.ClassId = classId;
                entity.FeeHeadingId = model.FeeHeadingId;
                entity.FeeValue = model.FeeValue;
                entity.BatchId = model.BatchId;
                entity.Medium = model.Medium;

                entity.UpdatedBy = userName;
                entity.UpdatedDate = DateTime.Now;

                _context.TuitionFeePlans.Update(entity);
            }
        }

        await _context.SaveChangesAsync();

        response.Status = 1;
        response.Message = "Record saved successfully.";

        return response;
    }
    catch (Exception ex)
    {
        response.Status = 0;
        response.Message = ex.Message;
        return response;
    }
}
        public async Task<ResponseModel> DeleteTuitionFeePlanAsync(long id, string username)
        {
            ResponseModel response = new();

            try
            {
                var data = await _context.TuitionFeePlans.FindAsync(id);

                if (data == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                // Toggle Active/Inactive
                data.IsActive = !data.IsActive;

                data.UpdatedBy = username;
                data.UpdatedDate = DateTime.Now;

                _context.TuitionFeePlans.Update(data);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Id = data.TuitionFeePlanId;
                response.Message = data.IsActive
                    ? "Tuition Fee Plan activated successfully."
                    : "Tuition Fee Plan deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion
        #region Fee Receipt
        public StudentFeePageVM GetStudentFeePage(int? classId = null,int? batchId = null,long? studentId = null)
        {
            var vm = new StudentFeePageVM
            {
                SelectedClassId = classId,
                SelectedBatchId = batchId,
                SelectedStudentId = studentId
            };

            // -----------------------------------------------------
            // STUDENTS
            // -----------------------------------------------------

            if (classId.HasValue && batchId.HasValue)
            {
                vm.Students = GetStudents(classId, batchId);
            }


            // -----------------------------------------------------
            // SELECTED STUDENT
            // -----------------------------------------------------

            if (studentId.HasValue)
            {
                vm.SelectedStudent = GetStudentDetails(studentId.Value);

                if (vm.SelectedStudent != null)
                {
                    vm.DueFeeBatch = vm.SelectedStudent.DueBalance;
                }
            }

            return vm;
        }
        public List<StudentFeeStudentVM> GetStudents(
           int? classId,
           int? batchId)
        {
            var query = _context.Tbl_Students
                .AsNoTracking()
                .Where(x => x.IsActive);


            // Current class
            if (classId.HasValue)
            {
                query = query.Where(x =>
                    x.AdmitClassId == classId.Value);
            }


            // Current batch
            if (batchId.HasValue)
            {
                query = query.Where(x =>
                    x.AdmitBatchId == batchId.Value);
            }


            var students = query
                .Select(x => new
                {
                    x.StudentId,

                    x.FirstName,
                    x.MiddleName,
                    x.LastName,

                    x.ContactNo,

                    x.Parent,

                    x.ScholarNumber,

                    x.AdmitClassId,
                    x.AdmitSectionId,
                    x.AdmitBatchId
                })
                .ToList();


            var result = new List<StudentFeeStudentVM>();


            foreach (var student in students)
            {
                result.Add(new StudentFeeStudentVM
                {
                    StudentId = student.StudentId,

                    StudentName = BuildFullName(
                        student.FirstName,
                        student.MiddleName,
                        student.LastName),

                    FatherName = student.Parent == null
                        ? string.Empty
                        : BuildFullName(
                            student.Parent.FatherFirstName,
                            student.Parent.FatherMiddleName,
                            student.Parent.FatherLastName),

                    ContactNo = student.ContactNo ?? string.Empty,

                    RollNo = student.ScholarNumber ?? "Not assigned",

                    ClassId = student.AdmitClassId,

                    SectionId = student.AdmitSectionId,

                    BatchId = student.AdmitBatchId,
                    ClassName=_context.DataListItems.Where(x=>x.DataListItemId==student.AdmitClassId).Select(x=>x.DataListItemText).FirstOrDefault(),
                    SectionName= _context.DataListItems.Where(x => x.DataListItemId == student.AdmitSectionId).Select(x => x.DataListItemText).FirstOrDefault(),
                    DueBalance = GetStudentonlyDueBalance(student.StudentId)
                });
            }


            return result
                .OrderBy(x => x.StudentName)
                .ToList();
        }


        // =========================================================
        // STUDENT DETAILS
        // =========================================================

        public StudentFeeStudentVM? GetStudentDetails(long studentId)
        {
            var student = _context.Tbl_Students
                .AsNoTracking()
                .Include(x => x.Parent)
                .FirstOrDefault(x =>
                    x.StudentId == studentId &&
                    x.IsActive);

            if (student == null)
                return null;


            var result = new StudentFeeStudentVM
            {
                StudentId = student.StudentId,

                StudentName = BuildFullName(
                    student.FirstName,
                    student.MiddleName,
                    student.LastName),

                FatherName = student.Parent == null
                    ? string.Empty
                    : BuildFullName(
                        student.Parent.FatherFirstName,
                        student.Parent.FatherMiddleName,
                        student.Parent.FatherLastName),

                ContactNo = student.ContactNo ?? "Not available",

                RollNo = student.ScholarNumber ?? "Not assigned",

                ClassId = student.AdmitClassId,

                SectionId = student.AdmitSectionId,

                BatchId = student.AdmitBatchId,

                //DueBalance = GetStudentDueBalance(
                //    student.StudentId)
                DueBalance= GetStudentonlyDueBalance(student.StudentId)
            };


            // Class name
            if (student.AdmitClassId.HasValue)
            {
                result.ClassName =
                    _context.DataListItems
                        .Where(x =>
                            x.DataListItemId ==
                            student.AdmitClassId.Value)
                        .Select(x => x.DataListItemText)
                        .FirstOrDefault()
                    ?? string.Empty;
            }


            // Section name
            if (student.AdmitSectionId.HasValue)
            {
                result.SectionName =
                    _context.DataListItems
                        .Where(x =>
                            x.DataListItemId ==
                            student.AdmitSectionId.Value)
                        .Select(x => x.DataListItemText)
                        .FirstOrDefault()
                    ?? string.Empty;
            }


            // Batch name
            if (student.AdmitBatchId.HasValue)
            {
                result.BatchName =
                    _context.Batches
                        .Where(x =>
                            x.BatchId ==
                            student.AdmitBatchId.Value)
                        .Select(x => x.AcademicYear)
                        .FirstOrDefault()
                    ?? string.Empty;
            }


            return result;
        }


        // =========================================================
        // STUDENT DUE BALANCE
        // =========================================================
        //public decimal GetStudentonlyDueBalance(long studentId)
        //{
        //    var student = _context.Tbl_Students
        //        .AsNoTracking()
        //        .FirstOrDefault(x =>
        //            x.StudentId == studentId &&
        //            x.IsActive);

        //    if (student == null)
        //        return 0;


        //    // ============================================
        //    // CURRENT CLASS & BATCH
        //    // ============================================

        //    int classId = student.AdmitClassId ?? 0;
        //    int batchId = student.AdmitBatchId ?? 0;


        //    // ============================================
        //    // 1. TUITION FEE
        //    // ============================================

        //    decimal tuitionFee = _context.TuitionFeePlans
        //        .AsNoTracking()
        //        .Where(x =>
        //            x.IsActive &&
        //            x.ClassId == classId &&
        //            x.BatchId == batchId)
        //        .Sum(x => (decimal?)x.FeeValue) ?? 0;


        //    // ============================================
        //    // 2. TRANSPORT FEE
        //    // ============================================

        //    decimal transportFee = 0;

        //    if (student.IsTranspot && student.TranspotId.HasValue)
        //    {
        //        transportFee = _context.TransportFeePlans
        //            .AsNoTracking()
        //            .Where(x =>
        //                x.IsActive &&
        //                x.TransportId == student.TranspotId.Value &&
        //                x.BatchId == batchId)
        //            .Sum(x => (decimal?)x.TransportFee) ?? 0;
        //    }


        //    // ============================================
        //    // 3. HOSTEL FEE
        //    // ============================================

        //    decimal hostelFee = 0;

        //    if (student.IsHostel && student.HostelId.HasValue)
        //    {
        //        hostelFee = _context.HostelFeePlans
        //            .AsNoTracking()
        //            .Where(x =>
        //                x.IsActive &&
        //                x.HostelId == student.HostelId.Value )
        //                //&& x. == batchId)
        //            .Sum(x => (decimal?)x.HostelFee) ?? 0;
        //    }


        //    // ============================================
        //    // TOTAL APPLICABLE FEE
        //    // ============================================

        //    decimal totalFee =
        //        tuitionFee +
        //        transportFee +
        //        hostelFee;


        //    // ============================================
        //    // TOTAL PAID BY STUDENT
        //    // ============================================

        //    decimal totalPaid = _context.StudentFees
        //        .AsNoTracking()
        //        .Where(x =>
        //            x.StudentId == studentId &&
        //            x.BatchId == batchId &&
        //            x.IsActive)
        //        .Sum(x => (decimal?)x.PaidAmount) ?? 0;


        //    // ============================================
        //    // DUE
        //    // ============================================

        //    decimal due = totalFee - totalPaid;


        //    // Advance payment होने पर Due negative नहीं होगा
        //    return Math.Max(0, due);
        //}
        public decimal GetStudentonlyDueBalance(long studentId)
        {
            var student = _context.Tbl_Students
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.StudentId == studentId &&
                    x.IsActive);

            if (student == null)
                return 0;

            int classId = student.AdmitClassId ?? 0;
            int batchId = student.AdmitBatchId ?? 0;

            if (classId <= 0 || batchId <= 0)
                return 0;

            var due = _context.StudentUnpaidFeeSPResults
                .FromSqlInterpolated($@"
            EXEC USP_GetStudentUnpaidFees
                @StudentId = {studentId},
                @ClassId = {classId},
                @BatchId = {batchId}")
                .AsNoTracking()
                .AsEnumerable()
                .Sum(x => x.Balance);

            return Math.Max(0, due);
        }
        public decimal GetStudentDueBalance(long studentId)
        {
            return _context.StudentFees
                .Where(x =>
                    x.StudentId == studentId &&
                    x.IsActive)
                .Sum(x => x.BalanceAmount);
        }


        // =========================================================
        // FULL NAME
        // =========================================================

        private static string BuildFullName(string? firstName,string? middleName,string? lastName)
        {
            return string.Join(" ",new[]
                {
                    firstName,
                    middleName,
                    lastName
                }
                .Where(x =>!string.IsNullOrWhiteSpace(x))).Trim();
        }
        //public StudentFeeReceiptVM GetUnpaidFees(long studentId,int classId,int batchId)
        //{
        //    var vm = new StudentFeeReceiptVM
        //    {
        //        StudentId = studentId,
        //        ClassId = classId,
        //        BatchId = batchId
        //    };

        //    // ============================================
        //    // STUDENT DETAILS
        //    // ============================================

        //    var student = _context.Tbl_Students
        //        .AsNoTracking()
        //        .Include(x => x.Parent)
        //        .FirstOrDefault(x =>
        //            x.StudentId == studentId &&
        //            x.IsActive);

        //    if (student == null)
        //        return vm;


        //    vm.StudentName = BuildFullName(
        //        student.FirstName,
        //        student.MiddleName,
        //        student.LastName);

        //    vm.FatherName = student.Parent == null
        //        ? string.Empty
        //        : BuildFullName(
        //            student.Parent.FatherFirstName,
        //            student.Parent.FatherMiddleName,
        //            student.Parent.FatherLastName);

        //    vm.ContactNo = student.ContactNo ?? string.Empty;

        //    vm.RollNo = student.ScholarNumber ?? "Not assigned";


        //    // ============================================
        //    // UNPAID / PARTIALLY PAID STUDENT FEES
        //    // ============================================

        //    var fees = _context.StudentFees
        //        .AsNoTracking()
        //        .Where(x =>
        //            x.StudentId == studentId &&
        //            x.ClassId == classId &&
        //            x.BatchId == batchId &&
        //            x.IsActive &&
        //            x.BalanceAmount > 0)
        //        .OrderBy(x => x.Year)
        //        .ThenBy(x => x.Month)
        //        .ThenBy(x => x.StudentFeeId)
        //        .ToList();


        //    // ============================================
        //    // MAP FEES
        //    // ============================================

        //    foreach (var fee in fees)
        //    {
        //        var item = new StudentFeeReceiptItemVM
        //        {
        //            FeeId = fee.FeeId,

        //            Amount = fee.FeeAmount,

        //            PaidAmount = fee.PaidAmount,

        //            Balance = fee.BalanceAmount,

        //            CollectAmount = fee.BalanceAmount,

        //            IsSelected = false,

        //            FeeDescription =$"{GetMonthName(fee.Month)} Month School Fee"
        //        };

        //        vm.TuitionFees.Add(item);
        //    }


        //    // ============================================
        //    // TOTAL DUE
        //    // ============================================

        //    vm.TotalAmount = vm.TuitionFees.Sum(x => x.Balance);

        //    vm.TotalFees = vm.TotalAmount;

        //    vm.DueBalance = vm.TotalAmount;

        //    return vm;
        //}
        public StudentFeeReceiptVM GetUnpaidFees(
      long studentId,
      int classId,
      int batchId)
        {
            var vm = new StudentFeeReceiptVM
            {
                StudentId = studentId,
                ClassId = classId,
                BatchId = batchId
            };

            // ============================================
            // STUDENT DETAILS
            // ============================================

            var student = _context.Tbl_Students
                .AsNoTracking()
                .Include(x => x.Parent)
                .FirstOrDefault(x =>
                    x.StudentId == studentId &&
                    x.IsActive);

            if (student == null)
                return vm;


            vm.StudentName = BuildFullName(
                student.FirstName,
                student.MiddleName,
                student.LastName);

            vm.FatherName = student.Parent == null
                ? string.Empty
                : BuildFullName(
                    student.Parent.FatherFirstName,
                    student.Parent.FatherMiddleName,
                    student.Parent.FatherLastName);

            vm.ContactNo = student.ContactNo ?? string.Empty;

            vm.RollNo = student.ScholarNumber ?? "Not assigned";


            // ============================================
            // GET UNPAID FEES FROM SP
            // ============================================

            var fees = _context.StudentUnpaidFeeSPResults
                .FromSqlInterpolated($@"
            EXEC USP_GetStudentUnpaidFees
                @StudentId = {studentId},
                @ClassId = {classId},
                @BatchId = {batchId}")
                .AsNoTracking()
                .ToList();


            // ============================================
            // DISTRIBUTE FEES BY FEE TYPE
            // ============================================

            foreach (var fee in fees)
            {
                var item = new StudentFeeReceiptItemVM
                {
                    FeeId = fee.FeeId,

                    FeePlanId = fee.FeePlanId,

                    FeeType = fee.FeeType,

                    FeeHeadingName = fee.FeeHeadingName,

                    Month = fee.Month,

                    Year = fee.Year,

                    Amount = fee.Amount,

                    PaidAmount = fee.PaidAmount,

                    Balance = fee.Balance,

                    CollectAmount = fee.Balance,

                    IsSelected = false,

                    PaymentStatus = fee.PaymentStatus,

                    FeeDescription = fee.FeeDescription
                };


                switch (fee.FeeType.Trim().ToLower())
                {
                    case "tuition":
                        vm.TuitionFees.Add(item);
                        break;

                    case "transport":
                        vm.TransportFees.Add(item);
                        break;

                    case "hostel":
                        vm.HostelFees.Add(item);
                        break;
                }
            }


            // ============================================
            // TOTAL DUE
            // ============================================

            vm.TotalAmount =
                vm.TuitionFees.Sum(x => x.Balance)
                + vm.TransportFees.Sum(x => x.Balance)
                + vm.HostelFees.Sum(x => x.Balance);

            vm.TotalFees = vm.TotalAmount;

            vm.DueBalance = vm.TotalAmount;


            return vm;
        }
        private string GetMonthName(int month)
        {
            if (month < 1 || month > 12)
                return string.Empty;

            return new DateTime(2000, month, 1)
                .ToString("MMMM");
        }
        #endregion
        #region Save Fee Receipt (Cash)

        //public long SaveCashFeeReceipt(StudentFeeReceiptVM vm)
        //{
        //    string receiptNumber = GenerateReceiptNumber();

        //    // ============================================
        //    // SELECTED FEES
        //    // ============================================

        //    var allSelectedFees = new List<StudentFeeReceiptItemVM>();
        //    allSelectedFees.AddRange(vm.TuitionFees.Where(x => x.IsSelected && x.CollectAmount > 0));
        //    allSelectedFees.AddRange(vm.TransportFees.Where(x => x.IsSelected && x.CollectAmount > 0));
        //    allSelectedFees.AddRange(vm.HostelFees.Where(x => x.IsSelected && x.CollectAmount > 0));

        //    if (!allSelectedFees.Any())
        //        throw new InvalidOperationException("Koi fee select nahi ki gayi.");


        //    // ============================================
        //    // SERVER-SIDE CALCULATION (client ki values trust nahi kar rahe)
        //    // ============================================

        //    decimal totalFeeSum = allSelectedFees.Sum(x => x.Amount);      // Actual fee amount
        //    decimal collectSum = allSelectedFees.Sum(x => x.CollectAmount); // Entered collect amount

        //    decimal lateFee = vm.LateFee;
        //    decimal concession = vm.ConcessionAmount;

        //    decimal receiptAmount = Math.Max(0, collectSum + lateFee - concession);

        //    // Student ka OVERALL due, is payment se PEHLE (SP se live fetch)
        //    decimal oldBalance = GetStudentonlyDueBalance(vm.StudentId);

        //    decimal balanceAmount = Math.Max(0, oldBalance - receiptAmount);
        //    // ============================================
        //    // RECEIPT HEADER
        //    // ============================================

        //    var receipt = new FeeReceipt
        //    {
        //        StudentId = vm.StudentId,
        //        ClassId = vm.ClassId,
        //        BatchId = vm.BatchId,
        //        PaymentModeId = vm.PaymentModeId,
        //        ReceiptDate = vm.ReceiptDate == default ? DateTime.Now : vm.ReceiptDate,
        //        ReceiptNumber = receiptNumber,

        //        ReceiptAmount = receiptAmount,
        //        TotalFee = totalFeeSum,
        //        LateFee = lateFee,
        //        Concession = vm.Concession,
        //        ConcessionAmount = concession,

        //        OldBalance = oldBalance,
        //        BalanceAmount = balanceAmount,
        //        DueAmount = balanceAmount,

        //        PaidAmount = receiptAmount,
        //        Remark = vm.Remark,
        //        CurrentYear = DateTime.Now.Year,
        //        FeeCollectedAmount = collectSum,
        //        IsActive = true
        //    };

        //    _context.FeeReceipt.Add(receipt);
        //    _context.SaveChanges();


        //    // ============================================
        //    // FEE ITEMS SAVE
        //    // ============================================

        //    foreach (var item in allSelectedFees)
        //    {
        //        var studentFee = _context.StudentFees
        //            .FirstOrDefault(x =>
        //                x.FeeId == item.FeeId &&
        //                x.StudentId == vm.StudentId &&
        //                x.Month == item.Month &&
        //                x.Year == item.Year);

        //        decimal balanceAfter;

        //        if (studentFee != null)
        //        {
        //            studentFee.FeeAmount = item.Amount;
        //            studentFee.PaidAmount += item.CollectAmount;
        //            studentFee.BalanceAmount = Math.Max(0, studentFee.FeeAmount - studentFee.PaidAmount);
        //            studentFee.IsFullyPaid = studentFee.BalanceAmount <= 0;

        //            balanceAfter = studentFee.BalanceAmount;

        //            _context.StudentFees.Update(studentFee);
        //        }
        //        else
        //        {
        //            studentFee = new StudentFee
        //            {
        //                StudentId = vm.StudentId,
        //                FeeId = item.FeeId,
        //                ClassId = vm.ClassId,
        //                BatchId = vm.BatchId,
        //                Month = item.Month,
        //                Year = item.Year,
        //                FeeType = item.FeeType,

        //                FeeAmount = item.Amount,
        //                PaidAmount = item.CollectAmount,
        //                BalanceAmount = Math.Max(0, item.Amount - item.CollectAmount),
        //                IsFullyPaid = (item.Amount - item.CollectAmount) <= 0,

        //                IsActive = true
        //            };

        //            _context.StudentFees.Add(studentFee);
        //            _context.SaveChanges();

        //            balanceAfter = studentFee.BalanceAmount;
        //        }

        //        var detail = new FeeReceiptDetail
        //        {
        //            FeeReceiptId = receipt.FeeReceiptId,
        //            StudentFeeId = studentFee.StudentFeeId,
        //            FeeId = item.FeeId,
        //            FeeDescription = item.FeeDescription,
        //            Month = item.Month,
        //            Year = item.Year,
        //            Amount = item.Amount,
        //            PaidAmount = item.CollectAmount,
        //            BalanceAmount = balanceAfter,
        //            AdjustedAmount = 0,
        //            IsActive = true
        //        };

        //        _context.FeeReceiptDetail.Add(detail);
        //    }

        //    _context.SaveChanges();

        //    return receipt.FeeReceiptId;
        //}
        #region 

        public long SaveCashFeeReceipt(StudentFeeReceiptVM vm)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                string receiptNumber = GenerateReceiptNumber();

                var allSelectedFees = new List<StudentFeeReceiptItemVM>();
                allSelectedFees.AddRange(vm.TuitionFees.Where(x => x.IsSelected && x.CollectAmount > 0));
                allSelectedFees.AddRange(vm.TransportFees.Where(x => x.IsSelected && x.CollectAmount > 0));
                allSelectedFees.AddRange(vm.HostelFees.Where(x => x.IsSelected && x.CollectAmount > 0));

                if (!allSelectedFees.Any())
                    throw new InvalidOperationException("No fee selected!");


                // ============================================
                // SERVER-SIDE CALCULATION
                // ============================================

                decimal totalFeeSum = allSelectedFees.Sum(x => x.Amount);
                decimal collectSum = allSelectedFees.Sum(x => x.CollectAmount);

                decimal lateFee = vm.LateFee;
                decimal concession = vm.ConcessionAmount;

                decimal receiptAmount = Math.Max(0, collectSum + lateFee - concession);

                decimal oldBalance = GetStudentonlyDueBalance(vm.StudentId);

                decimal balanceAmount = Math.Max(0, oldBalance - collectSum - concession);


                // ============================================
                // CONCESSION TARGET FEE NIRDHARIT KARO
                // ============================================

                var dueAfterCollectMap = new Dictionary<StudentFeeReceiptItemVM, decimal>();

                foreach (var item in allSelectedFees)
                {
                    var existing = _context.StudentFees
                        .FirstOrDefault(x =>
                            x.FeeId == item.FeeId &&
                            x.StudentId == vm.StudentId &&
                            x.Month == item.Month &&
                            x.Year == item.Year);

                    decimal dueBeforeThisPayment = existing?.BalanceAmount ?? item.Amount;
                    decimal dueAfterCollect = Math.Max(0, dueBeforeThisPayment - item.CollectAmount);

                    dueAfterCollectMap[item] = dueAfterCollect;
                }

                StudentFeeReceiptItemVM concessionTargetItem = null;

                if (concession > 0)
                {
                    concessionTargetItem = allSelectedFees
                        .FirstOrDefault(x => dueAfterCollectMap[x] == concession);

                    if (concessionTargetItem == null)
                    {
                        concessionTargetItem = allSelectedFees.Last();
                    }
                }


                // ============================================
                // RECEIPT HEADER
                // ============================================

                var receipt = new FeeReceipt
                {
                    StudentId = vm.StudentId,
                    ClassId = vm.ClassId,
                    BatchId = vm.BatchId,
                    PaymentModeId = vm.PaymentModeId,
                    ReceiptDate = vm.ReceiptDate == default ? DateTime.Now : vm.ReceiptDate,
                    ReceiptNumber = receiptNumber,

                    ReceiptAmount = receiptAmount,
                    TotalFee = totalFeeSum,
                    LateFee = lateFee,

                    ConcessionAmount = concession,
                    Concession = vm.Concession,

                    OldBalance = oldBalance,
                    BalanceAmount = balanceAmount,
                    DueAmount = balanceAmount,

                    PaidAmount = receiptAmount,
                    Remark = vm.Remark,
                    CurrentYear = DateTime.Now.Year,
                    FeeCollectedAmount = collectSum,
                    IsActive = true
                };

                _context.FeeReceipt.Add(receipt);
                _context.SaveChanges();


                // ============================================
                // FEE ITEMS SAVE
                // ============================================

                foreach (var item in allSelectedFees)
                {
                    var studentFee = _context.StudentFees
                        .FirstOrDefault(x =>
                            x.FeeId == item.FeeId &&
                            x.StudentId == vm.StudentId &&
                            x.Month == item.Month &&
                            x.Year == item.Year);

                    decimal previousPaid;
                    decimal previousBalance;
                    decimal balanceAfter;
                    decimal itemConcession = 0;

                    if (item == concessionTargetItem && concession > 0)
                    {
                        decimal dueAfterCollect = dueAfterCollectMap[item];

                        itemConcession = concession <= dueAfterCollect
                            ? concession
                            : dueAfterCollect;
                    }


                    if (studentFee != null)
                    {
                        previousPaid = studentFee.PaidAmount;
                        previousBalance = studentFee.BalanceAmount;

                        studentFee.FeeAmount = item.Amount;
                        studentFee.PaidAmount += item.CollectAmount + itemConcession;
                        studentFee.BalanceAmount = Math.Max(0, studentFee.FeeAmount - studentFee.PaidAmount);
                        studentFee.IsFullyPaid = studentFee.BalanceAmount <= 0;

                        balanceAfter = studentFee.BalanceAmount;

                        _context.StudentFees.Update(studentFee);
                    }
                    else
                    {
                        previousPaid = 0;
                        previousBalance = item.Amount;

                        decimal paidWithConcession = item.CollectAmount + itemConcession;

                        studentFee = new StudentFee
                        {
                            StudentId = vm.StudentId,
                            FeeId = item.FeeId,
                            ClassId = vm.ClassId,
                            BatchId = vm.BatchId,
                            Month = item.Month,
                            Year = item.Year,
                            FeeType = item.FeeType,

                            FeeAmount = item.Amount,
                            PaidAmount = paidWithConcession,
                            BalanceAmount = Math.Max(0, item.Amount - paidWithConcession),
                            IsFullyPaid = (item.Amount - paidWithConcession) <= 0,

                            IsActive = true
                        };

                        _context.StudentFees.Add(studentFee);
                        _context.SaveChanges();

                        balanceAfter = studentFee.BalanceAmount;
                    }

                    var detail = new FeeReceiptDetail
                    {
                        FeeReceiptId = receipt.FeeReceiptId,
                        StudentFeeId = studentFee.StudentFeeId,
                        FeeId = item.FeeId,
                        FeeType = item.FeeType,
                        FeeDescription = item.FeeDescription,
                        Month = item.Month,
                        Year = item.Year,
                        Amount = item.Amount,

                        PreviousPaidAmount = previousPaid,
                        PreviousBalanceAmount = previousBalance,

                        PaidAmount = item.CollectAmount,
                        BalanceAmount = balanceAfter,
                        AdjustedAmount = itemConcession,
                        IsActive = true
                    };

                    _context.FeeReceiptDetail.Add(detail);
                }

                _context.SaveChanges();

                // ============================================
                // SAB KUCH SAHI GAYA — COMMIT KARO
                // ============================================

                transaction.Commit();

                return receipt.FeeReceiptId;
            }
            catch (Exception)
            {
                // ============================================
                // KAHIN BHI ERROR AAYA — SAB KUCH ROLLBACK KARO
                // ============================================

                transaction.Rollback();
                throw;   // 👈 exception ko dubara throw karo taaki controller/caller ko pata chale
            }
        }

        #endregion

        private string GenerateReceiptNumber()
        {
            int year = DateTime.Now.Year;

            var lastReceipt = _context.FeeReceipt          // ✅ singular
                .Where(x => x.CurrentYear == year)
                .OrderByDescending(x => x.FeeReceiptId)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastReceipt != null && !string.IsNullOrEmpty(lastReceipt.ReceiptNumber))
            {
                var parts = lastReceipt.ReceiptNumber.Split('-');
                if (parts.Length > 0 && int.TryParse(parts[^1], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"RC-{year}-{nextNumber:D5}";
        }


        public FeeReceipt? GetReceiptForPrint(long feeReceiptId)
        {
            return _context.FeeReceipt              // ✅ singular
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.PaymentMode)
                .Include(x => x.Details)
                .FirstOrDefault(x => x.FeeReceiptId == feeReceiptId);
        }

        #endregion

    }


}

