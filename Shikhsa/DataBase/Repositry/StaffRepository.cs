using Dapper;
using Microsoft.Data.SqlClient;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using System.Data;

namespace Shikhsa.DataBase.Repositry
{
    public class StaffRepository
    {

        private readonly IConfiguration _configuration;

        public StaffRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));
            }
        }
        public async Task<ResponseModel> SaveStaff(string staffXml)
        {
            using var con = Connection;

            var param = new DynamicParameters();

            param.Add("@StaffXML", staffXml);

            var result = await con.QueryFirstOrDefaultAsync<ResponseModel>(
                            "USP_SaveStaff",
                            param,
                            commandType: CommandType.StoredProcedure);

            return result;
        }
        //public async Task<StaffMaster> GetStaffById(long id)
        //{
        //    using var con = Connection;

        //    var param = new DynamicParameters();

        //    param.Add("@StaffId", id);

        //    var result = await con.QueryFirstOrDefaultAsync<StaffMaster>(
        //                    "USP_GetStaffById",
        //                    param,
        //                    commandType: CommandType.StoredProcedure);

        //    return result;
        //}
        public async Task<StaffMaster?> GetStaffById(long id)
        {
            using var con = Connection;

            var param = new DynamicParameters();

            param.Add("@StaffId", id);

            using var multi = await con.QueryMultipleAsync(
                "USP_GetStaffById",
                param,
                commandType: CommandType.StoredProcedure);

            // Result Set 1
            var staff = await multi.ReadFirstOrDefaultAsync<StaffMaster>();

            if (staff == null)
                return null;

            // Result Set 2
            staff.Academics = (await multi.ReadAsync<StaffAcademic>()).ToList();

            // Result Set 3
            staff.Experiences = (await multi.ReadAsync<StaffExperience>()).ToList();

            // Result Set 4
            staff.Documents = (await multi.ReadAsync<StaffDocument>()).ToList();

            // Result Set 5
            staff.EmergencyContacts =
                (await multi.ReadAsync<StaffEmergencyContact>()).ToList();

            return staff;
        }
        public async Task<List<StaffListModel>> GetStaffList(StaffFilterVM filter)
        {
            using var con = Connection;

            var param = new DynamicParameters();

            param.Add("@StaffCode", filter.StaffCode);
            param.Add("@StaffName", filter.StaffName);
            param.Add("@MobileNo", filter.MobileNo);
            param.Add("@Email", filter.Email);
            param.Add("@DepartmentId", filter.DepartmentId);
            param.Add("@DesignationId", filter.DesignationId);
            param.Add("@StaffTypeId", filter.StaffTypeId);
            param.Add("@IsActive", filter.IsActive);

            var result = await con.QueryAsync<StaffListModel>(
                            "USP_GetStaffList",
                            param,
                            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
        public async Task<ResponseModel> DeleteStaff(long staffId)
        {
            using var con = Connection;

            var param = new DynamicParameters();

            param.Add("@StaffId", staffId);

            var result = await con.QueryFirstOrDefaultAsync<ResponseModel>(
                            "USP_DeleteStaff",
                            param,
                            commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
