using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.ViewModels;

namespace Shikhsa.DataBase.Repositry
{
    public class StaffAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public StaffAttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<StaffAttendanceRowVM>> GetStaffAttendanceAsync(
    DateOnly attendanceDate,
    string? search)
        {
            var staffs = await _context.StaffMasters
                .Where(x => x.IsActive)
                .ToListAsync();

            var attendance = await _context.StaffAttendances
                .Where(x => x.AttendanceDate == attendanceDate)
                .ToListAsync();

            var result = staffs.Select(s =>
            {
                var att = attendance.FirstOrDefault(a => a.StaffId == s.StaffId);

                return new StaffAttendanceRowVM
                {
                    StaffId = s.StaffId,
                    StaffCode = s.StaffCode,
                    StaffName = $"{s.FirstName}{s.MiddleName} {s.LastName}",
                    AttendanceId = att?.AttendanceId ?? 0,
                    AttendanceTypeId = att?.AttendanceTypeId ?? 0,
                    Remarks = att?.Remarks
                };
            });

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(x =>
                    x.StaffName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.StaffCode.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return result.OrderBy(x => x.StaffName).ToList();
        }
        public async Task<List<StaffMaster>> GetStaffListAsync(string? search = null)
        {
            var query = _context.StaffMasters
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    x.StaffCode.Contains(search));
            }

            return await query
                .OrderBy(x => x.FirstName)
                .ToListAsync();
        }
        public async Task<List<StaffAttendance>> GetAttendanceByDateAsync(DateOnly attendanceDate)
        {
            return await _context.StaffAttendances
                .Include(x => x.AttendanceType)
                .Where(x => x.AttendanceDate == attendanceDate)
                .ToListAsync();
        }
        public async Task<bool> SaveAttendanceAsync(StaffAttendance attendance)
        {
            var existing = await _context.StaffAttendances
                .FirstOrDefaultAsync(x =>
                    x.StaffId == attendance.StaffId &&
                    x.AttendanceDate == attendance.AttendanceDate);

            if (existing == null)
            {
                await _context.StaffAttendances.AddAsync(attendance);
            }
            else
            {
                existing.AttendanceTypeId = attendance.AttendanceTypeId;
                existing.Remarks = attendance.Remarks;

                existing.UpdatedBy = attendance.UpdatedBy;
                existing.UpdatedDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteAttendanceAsync(long attendanceId)
        {
            var attendance = await _context.StaffAttendances
                .FindAsync(attendanceId);

            if (attendance == null)
                return false;

            _context.StaffAttendances.Remove(attendance);

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> MarkAllPresentAsync(DateOnly attendanceDate, string userId)
        {
            const int presentTypeId = 1;

            var staffs = await _context.StaffMasters
                .Where(x => x.IsActive)
                .ToListAsync();

            var existing = await _context.StaffAttendances
                .Where(x => x.AttendanceDate == attendanceDate)
                .ToListAsync();

            foreach (var staff in staffs)
            {
                var attendance = existing
                    .FirstOrDefault(x => x.StaffId == staff.StaffId);

                if (attendance == null)
                {
                    await _context.StaffAttendances.AddAsync(new StaffAttendance
                    {
                        StaffId = staff.StaffId,
                        AttendanceDate = attendanceDate,
                        AttendanceTypeId = presentTypeId,
                        AddedBy= userId,
                        AddedDate = DateTime.Now
                    });
                }
                else
                {
                    attendance.AttendanceTypeId = presentTypeId;
                    attendance.UpdatedBy = userId;
                    attendance.UpdatedDate = DateTime.Now;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<AttendanceSummary> GetSummaryAsync(DateOnly attendanceDate)
        {
            var summary = new AttendanceSummary();

            summary.TotalStaff = await _context.StaffMasters
                .CountAsync(x => x.IsActive);

            var attendance = await _context.StaffAttendances
                .Include(x => x.AttendanceType)
                .Where(x => x.AttendanceDate == attendanceDate)
                .ToListAsync();

            summary.Present = attendance.Count(x =>
                x.AttendanceType.Code == "P");

            summary.Absent = attendance.Count(x =>
                x.AttendanceType.Code == "A");

            summary.HalfDay = attendance.Count(x =>
                x.AttendanceType.Code == "H");

            summary.Leave = attendance.Count(x =>
                x.AttendanceType.IsLeave);

            return summary;
        }
        public async Task<bool> SaveAllAttendanceAsync(StaffAttendanceVM vm, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in vm.Staffs)
                {
                    if (item.AttendanceTypeId == 0)
                        continue;

                    var attendance = await _context.StaffAttendances
                        .FirstOrDefaultAsync(x =>
                            x.StaffId == item.StaffId &&
                            x.AttendanceDate == vm.AttendanceDate);

                    if (attendance == null)
                    {
                        attendance = new StaffAttendance
                        {
                            StaffId = item.StaffId,
                            AttendanceDate = vm.AttendanceDate,
                            AttendanceTypeId = item.AttendanceTypeId,
                            Remarks = item.Remarks,
                           AddedBy = userName,
                            AddedDate = DateTime.Now
                        };

                        _context.StaffAttendances.Add(attendance);
                    }
                    else
                    {
                        attendance.AttendanceTypeId = item.AttendanceTypeId;
                        attendance.Remarks = item.Remarks;
                        attendance.UpdatedBy = userName;
                        attendance.UpdatedDate = DateTime.Now;

                        _context.StaffAttendances.Update(attendance);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<AttendanceType>> GetAllAsync()
        {
            return await _context.AttendanceTypes
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }
        public async Task<AttendanceType?> GetByIdAsync(int id)
        {
            return await _context.AttendanceTypes
                .FirstOrDefaultAsync(x => x.AttendanceTypeId == id);
        }
        public async Task SaveAsync(AttendanceType model)
        {
            if (model.AttendanceTypeId == 0)
            {
                model.AddedDate = DateTime.Now;
                _context.AttendanceTypes.Add(model);
            }
            else
            {
                var data = await _context.AttendanceTypes.FindAsync(model.AttendanceTypeId);

                if (data != null)
                {
                    data.Code = model.Code;
                    data.Name = model.Name;
                    data.IsLeave = model.IsLeave;
                    data.IsActive = model.IsActive;
                    data.DisplayOrder = model.DisplayOrder;
                    data.UpdatedDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var data = await _context.AttendanceTypes.FindAsync(id);

            if (data != null)
            {
                _context.AttendanceTypes.Remove(data);
                await _context.SaveChangesAsync();
            }
        }
    }
}
