using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models;
using Shikhsa.Models.Common;
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


    }
}
