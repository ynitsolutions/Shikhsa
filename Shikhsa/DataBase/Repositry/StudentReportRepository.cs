using Dapper;
using Microsoft.Data.SqlClient;
using Shikhsa.ViewModels.DataFilter;
using System.Data;

namespace Shikhsa.DataBase.Repositry
{
    public class StudentReportRepository
    {
        private readonly IConfiguration _configuration;

        public StudentReportRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<StudentListReportVM>>GetStudentReport(StudentListFilterVM filter)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();
            param.Add("@ApplicationNo", filter.ApplicationNo);
            param.Add("@StudentName", filter.StudentName);
            param.Add("@FatherName", filter.FatherName);
            param.Add("@MotherName", filter.MotherName);
            param.Add("@GuardianName", filter.GuardianName);
            param.Add("@MobileNo", filter.MobileNo);
            param.Add("@CategoryId", filter.CategoryId);
            param.Add("@GenderId", filter.GenderId);
            param.Add("@ReligionId", filter.ReligionId);
            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
            param.Add("@RegClassId", filter.RegClassId);

            var result =
                await con.QueryAsync<StudentListReportVM>(
                    "Sp_GetStudentListReport",
                    param,
                    commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
        public async Task<List<StudentListReportVM>> GetAdmittedStudentsList(StudentListFilterVM filter)
        {
            using var con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            var param = new DynamicParameters();
            param.Add("@ApplicationNo", filter.ApplicationNo);
            param.Add("@StudentName", filter.StudentName);
            param.Add("@FatherName", filter.FatherName);
            param.Add("@MotherName", filter.MotherName);
           
            param.Add("SectionId", filter.SectionId);
            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
            param.Add("@AdmitClassId", filter.RegClassId);

            var result =
                await con.QueryAsync<StudentListReportVM>(
                    "USP_AdmittedStudentsList",
                    param,
                    commandType: CommandType.StoredProcedure);

            return result.ToList();
        }
    }
}
