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
		

        public async Task<List<HostelFeePlan>> GetAllHostelFeePlanAsync()
        {
            return await _context.HostelFeePlans
                .Include(x => x.FeeHeading)
                .OrderByDescending(x => x.HostelFeePlanId)
                .ToListAsync();
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
                && x.RoomType.ToLower() == roomType.ToLower()
                && x.HostelFeePlanId != hostelFeePlanId);
        }

        public async Task<ResponseModel> SaveHostelFeePlanAsync(HostelFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                await _context.HostelFeePlans.AddAsync(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Hostel Fee Plan saved successfully.";
                response.Id = model.HostelFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> UpdateHostelFeePlanAsync(HostelFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                _context.HostelFeePlans.Update(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Hostel Fee Plan updated successfully.";
                response.Id = model.HostelFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
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

        public async Task<List<TransportFeePlan>> GetAllTransportFeePlanAsync()
        {
            return await _context.TransportFeePlans
                .Include(x => x.FeeHeading)
                .OrderByDescending(x => x.TransportFeePlanId)
                .ToListAsync();
        }

        public async Task<List<TransportFeePlan>> GetActiveTransportFeePlanAsync()
        {
            return await _context.TransportFeePlans
                .Include(x => x.FeeHeading)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.TransportFeePlanId)
                .ToListAsync();
        }

        public async Task<TransportFeePlan?> GetTransportFeePlanByIdAsync(long id)
        {
            return await _context.TransportFeePlans
                .FirstOrDefaultAsync(x => x.TransportFeePlanId == id);
        }

        public async Task<bool> IsDuplicateTransportFeePlanAsync(long feeHeadingId, int transportId, string academicYear, long transportFeePlanId)
        {
            return await _context.TransportFeePlans.AnyAsync(x =>
                x.FeeHeadingId == feeHeadingId
                && x.TransportId == transportId
                && x.AcademicYear == academicYear
                && x.TransportFeePlanId != transportFeePlanId);
        }

        public async Task<ResponseModel> SaveTransportFeePlanAsync(TransportFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                await _context.TransportFeePlans.AddAsync(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Transport Fee Plan saved successfully.";
                response.Id = model.TransportFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> UpdateTransportFeePlanAsync(TransportFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                _context.TransportFeePlans.Update(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Transport Fee Plan updated successfully.";
                response.Id = model.TransportFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
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

        public async Task<List<TuitionFeePlan>> GetAllTuitionFeePlanAsync()
        {
            return await _context.TuitionFeePlans
                .Include(x => x.FeeHeading)
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

        public async Task<bool> IsDuplicateTuitionFeePlanAsync(long feeHeadingId, int classId, string academicYear, long tuitionFeePlanId)
        {
            return await _context.TuitionFeePlans.AnyAsync(x =>
                x.FeeHeadingId == feeHeadingId
                && x.ClassId == classId
                && x.AcademicYear == academicYear
                && x.TuitionFeePlanId != tuitionFeePlanId);
        }

        public async Task<ResponseModel> SaveTuitionFeePlanAsync(TuitionFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                await _context.TuitionFeePlans.AddAsync(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Tuition Fee Plan saved successfully.";
                response.Id = model.TuitionFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> UpdateTuitionFeePlanAsync(TuitionFeePlan model)
        {
            ResponseModel response = new();

            try
            {
                _context.TuitionFeePlans.Update(model);
                await _context.SaveChangesAsync();

                response.Status = 1;
                response.Message = "Tuition Fee Plan updated successfully.";
                response.Id = model.TuitionFeePlanId;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
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
