//using Dapper;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Shikhsa.Data;
//using Shikhsa.Models;
//using Shikhsa.Models.Payment;
//using Shikhsa.ViewModels;
//using Shikhsa.ViewModels.DataFilter;
//using System.Data;

//namespace Shikhsa.DataBase.Repositry
//{
//    public class StudentReportRepository
//    {
//        private readonly IConfiguration _configuration;
//        private readonly ApplicationDbContext _db;
//        public StudentReportRepository(IConfiguration configuration,ApplicationDbContext db)
//        {
//            _configuration = configuration;
//            _db = db;
//        }

//        public async Task<List<StudentListReportVM>>GetStudentReport(StudentListFilterVM filter)
//        {
//            using var con = new SqlConnection(
//                _configuration.GetConnectionString("DefaultConnection"));

//            var param = new DynamicParameters();
//            param.Add("@ApplicationNo", filter.ApplicationNo);
//            param.Add("@StudentName", filter.StudentName);
//            param.Add("@FatherName", filter.FatherName);
//            param.Add("@MotherName", filter.MotherName);
//            param.Add("@GuardianName", filter.GuardianName);
//            param.Add("@MobileNo", filter.MobileNo);
//            param.Add("@CategoryId", filter.CategoryId);
//            param.Add("@GenderId", filter.GenderId);
//            param.Add("@ReligionId", filter.ReligionId);
//            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
//            param.Add("@RegClassId", filter.RegClassId);

//            var result =
//                await con.QueryAsync<StudentListReportVM>(
//                    "Sp_GetStudentListReport",
//                    param,
//                    commandType: CommandType.StoredProcedure);

//            return result.ToList();
//        }
//        public async Task<List<StudentListReportVM>> GetAdmittedStudentsList(StudentListFilterVM filter)
//        {
//            using var con = new SqlConnection(
//                _configuration.GetConnectionString("DefaultConnection"));

//            var param = new DynamicParameters();
//            param.Add("@ApplicationNo", filter.ApplicationNo);
//            param.Add("@StudentName", filter.StudentName);
//            param.Add("@FatherName", filter.FatherName);
//            param.Add("@MotherName", filter.MotherName);
//            param.Add("SectionId", filter.SectionId);
//            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
//            param.Add("@AdmitClassId", filter.RegClassId);
//            var result =
//                await con.QueryAsync<StudentListReportVM>(
//                    "USP_AdmittedStudentsList",
//                    param,
//                    commandType: CommandType.StoredProcedure);

//            return result.ToList();
//        }
//        public async Task<StudentProfileViewModel> GetStudentProfileAsync(long studentId)
//        {
//            var conn = _db.Database.GetDbConnection();
//            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

//            using var multi = await conn.QueryMultipleAsync(
//                "sp_GetStudentProfile",
//                new { StudentId = studentId },
//                commandType: CommandType.StoredProcedure);


//            var student = await multi.ReadFirstOrDefaultAsync<Tbl_Students>();
//            var parent = await multi.ReadFirstOrDefaultAsync<Tbl_Parents>();

//            var documents = (await multi.ReadAsync<Tbl_StudentDocument>()).AsList();
//            var previousSchool = await multi.ReadFirstOrDefaultAsync<Tbl_PreviousSchoolRecord>();
//            var fees = (await multi.ReadAsync<StudentFee>()).AsList();
//            var receipts = (await multi.ReadAsync<FeeReceipt>()).AsList();
//            var attendance = (await multi.ReadAsync<AttendanceMonthSummary>()).AsList();
//            var batches = (await multi.ReadAsync<Batches>()).AsList();
//            var scholasticMarks = (await multi.ReadAsync<ScholasticMarkRow>()).AsList();
//            var examSummaries = (await multi.ReadAsync<ExamSummaryRow>()).AsList();
//            var coScholasticGrades = (await multi.ReadAsync<CoScholasticGradeRow>()).AsList();
//            return new StudentProfileViewModel
//            {
//                Student = student,
//                Parent = parent,
//                Documents = documents,
//                PreviousSchool = previousSchool,
//                CurrentFees = fees,
//                RecentReceipts = receipts,
//                CurrentAttendance = attendance,
//                BatchHistory = batches,
//                ScholasticMarks = scholasticMarks,
//                ExamSummaries = examSummaries,
//                CoScholasticGrades = coScholasticGrades
//            };
//        }
//        public async Task<BatchSummaryResult> GetBatchSummaryAsync(long studentId, int batchId)
//        {
//            var conn = _db.Database.GetDbConnection();
//            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

//            using var multi = await conn.QueryMultipleAsync(
//                "sp_GetStudentBatchSummary",
//                new { StudentId = studentId, BatchId = batchId },
//                commandType: CommandType.StoredProcedure);

//            var batchInfo = await multi.ReadFirstOrDefaultAsync<Batches>();
//            var fees = (await multi.ReadAsync<StudentFee>()).AsList();
//            var attendance = (await multi.ReadAsync<AttendanceMonthSummary>()).AsList();
//            var scholasticMarks = (await multi.ReadAsync<ScholasticMarkRow>()).AsList();
//            var examSummaries = (await multi.ReadAsync<ExamSummaryRow>()).AsList();
//            var coScholasticGrades = (await multi.ReadAsync<CoScholasticGradeRow>()).AsList();

//            return new BatchSummaryResult
//            {
//                BatchInfo = batchInfo,
//                Fees = fees,
//                Attendance = attendance,
//                ScholasticMarks = scholasticMarks,
//                ExamSummaries = examSummaries,
//                CoScholasticGrades = coScholasticGrades
//            };
//        }
//        public async Task<List<StudentListReportVM>> GetStudentReportStatusWise(StudentListFilterVM filter)
//        {
//            using var con = new SqlConnection(
//                _configuration.GetConnectionString("DefaultConnection"));

//            var param = new DynamicParameters();
//            param.Add("@ApplicationNo", filter.ApplicationNo);
//            param.Add("@StudentName", filter.StudentName);
//            param.Add("@FatherName", filter.FatherName);
//            param.Add("@MotherName", filter.MotherName);
//            param.Add("@GuardianName", filter.GuardianName);
//            param.Add("@MobileNo", filter.MobileNo);

//            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
//            param.Add("@RegClassId", filter.RegClassId);
//            param.Add("@RegSectionId", filter.SectionId);
//            param.Add("@RegisterId", filter.StatusId);
//            var result =
//                await con.QueryAsync<StudentListReportVM>(
//                    "Sp_GetStudentListReportStatusWise",
//                    param,
//                    commandType: CommandType.StoredProcedure);

//            return result.ToList();
//        }
//    }
//}
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Payment;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using System.Data;
using System.Threading.Tasks;

namespace Shikhsa.DataBase.Repositry
{
    // NuGet: dotnet add package Dapper
    // Dapper hume seedha SP ke result sets ko aapke EXISTING classes
    // (Tbl_Students, Tbl_Parents, StudentFee, ...) me map karne deta hai —
    // koi naya model banane ki zaroorat nahi.

    //public interface IStudentProfileRepository
    //{
    //    Task<StudentProfileViewModel> GetStudentProfileAsync(long studentId);
    //    Task<BatchSummaryResult> GetBatchSummaryAsync(long studentId, int batchId);
    //}

    public class StudentReportRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        public StudentReportRepository(ApplicationDbContext db,IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<StudentProfileViewModel> GetStudentProfileAsync(long studentId)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var multi = await conn.QueryMultipleAsync(
                "sp_GetStudentProfile",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure);

            // Result set 1: Student. Result set 2: ExtraInfo (Category/Gender/
            // Religion/CurrentBatch names). Result set 3: Parent.
            // (Pehle Student+Parent ko ek hi JOIN row me multi-mapping se split kar
            // rahe the, jisse Dapper ka generic overload resolve nahi ho pa raha
            // tha (CS0305). Ab SP khud hi sabko alag result set me bhejta hai —
            // yahan sirf seedha ReadFirstOrDefaultAsync<T>() karna hai.)
            var student = await multi.ReadFirstOrDefaultAsync<Tbl_Students>();
            var extraInfo = await multi.ReadFirstOrDefaultAsync<StudentExtraInfo>();
            var parent = await multi.ReadFirstOrDefaultAsync<Tbl_Parents>();

            var documents = (await multi.ReadAsync<Tbl_StudentDocument>()).AsList();
            var previousSchool = await multi.ReadFirstOrDefaultAsync<Tbl_PreviousSchoolRecord>();
            var fees = (await multi.ReadAsync<StudentFee>()).AsList();
            var receipts = (await multi.ReadAsync<FeeReceipt>()).AsList();
            var attendance = (await multi.ReadAsync<AttendanceMonthSummary>()).AsList();
            var batches = (await multi.ReadAsync<Batches>()).AsList();
            var scholasticMarks = (await multi.ReadAsync<ScholasticMarkRow>()).AsList();
            var examSummaries = (await multi.ReadAsync<ExamSummaryRow>()).AsList();
            var coScholasticGrades = (await multi.ReadAsync<CoScholasticGradeRow>()).AsList();

            return new StudentProfileViewModel
            {
                Student = student,
                ExtraInfo = extraInfo ?? new StudentExtraInfo(),
                Parent = parent,
                Documents = documents,
                PreviousSchool = previousSchool,
                CurrentFees = fees,
                RecentReceipts = receipts,
                CurrentAttendance = attendance,
                BatchHistory = batches,
                ScholasticMarks = scholasticMarks,
                ExamSummaries = examSummaries,
                CoScholasticGrades = coScholasticGrades
            };
        }
        public async Task<List<StudentListReportVM>> GetStudentReport(StudentListFilterVM filter)
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
        public async Task<List<StudentListReportVM>> GetStudentReportStatusWise(StudentListFilterVM filter)
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

            param.Add("@AdmissionBatchId", filter.AdmissionBatchId);
            param.Add("@RegClassId", filter.RegClassId);
            param.Add("@RegSectionId", filter.SectionId);
            param.Add("@RegisterId", filter.StatusId);
            var result = await con.QueryAsync<StudentListReportVM>(
                    "Sp_GetStudentListReportStatusWise",
                    param,
                    commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<BatchSummaryResult> GetBatchSummaryAsync(long studentId, int batchId)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var multi = await conn.QueryMultipleAsync(
                "sp_GetStudentBatchSummary",
                new { StudentId = studentId, BatchId = batchId },
                commandType: CommandType.StoredProcedure);

            var batchInfo = await multi.ReadFirstOrDefaultAsync<Batches>();
            var fees = (await multi.ReadAsync<StudentFee>()).AsList();
            var attendance = (await multi.ReadAsync<AttendanceMonthSummary>()).AsList();
            var scholasticMarks = (await multi.ReadAsync<ScholasticMarkRow>()).AsList();
            var examSummaries = (await multi.ReadAsync<ExamSummaryRow>()).AsList();
            var coScholasticGrades = (await multi.ReadAsync<CoScholasticGradeRow>()).AsList();

            return new BatchSummaryResult
            {
                BatchInfo = batchInfo,
                Fees = fees,
                Attendance = attendance,
                ScholasticMarks = scholasticMarks,
                ExamSummaries = examSummaries,
                CoScholasticGrades = coScholasticGrades
            };
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

        //public async Task<List<StudentListReportVM>> GetStudentUserNamePasswordAsync(StudentListFilterVM filter)
        //{
        //    var list = new List<StudentListReportVM>();

        //    using (SqlConnection conn = new SqlConnection("DefaultConnection"))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("GetStudentUserNamePasswordDetails", conn))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@StudentName", (object)filter.StudentName ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@FatherName", (object)filter.FatherName ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@MotherName", (object)filter.MotherName ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@ClassId", (object)filter.RegClassId ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@SectionId", (object)filter.SectionId ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@StatusId", (object)filter.StatusId ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@UserName", (object)filter.UserName ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@Email", (object)filter.Email ?? DBNull.Value);
        //            await conn.OpenAsync();
        //            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    list.Add(new StudentListReportVM
        //                    {
        //                        StudentName= reader["Email"]?.ToString(),
        //                        Email = reader["Email"]?.ToString(),
        //                        FatherName = reader["FatherName"]?.ToString(),
        //                        MotherName = reader["MotherName"]?.ToString(),
        //                        UserName = reader["UserName"]?.ToString(),
        //                        //Email = reader["Email"]?.ToString(),
        //                        Password = reader["NormalPassword "]?.ToString(),
        //                        ClassName = reader["ClassName"]?.ToString(),
        //                        SectionName = reader["SectionName"]?.ToString()
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    return list;
        //}

        public async Task<List<StudentListReportVM>> GetStudentUserNamePasswordAsync(
    StudentListFilterVM filter)
        {
            var list = new List<StudentListReportVM>();

            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "GetStudentUserNamePasswordDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@StudentName",
                        (object?)filter.StudentName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@FatherName",
                        (object?)filter.FatherName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@MotherName",
                        (object?)filter.MotherName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@ClassId",
                        (object?)filter.RegClassId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@SectionId",
                        (object?)filter.SectionId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@StatusId",
                        (object?)filter.StatusId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@UserName",
                        (object?)filter.UserName ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@Email",
                        (object?)filter.Email ?? DBNull.Value);

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new StudentListReportVM
                            {
                                StudentName = reader["StudentName"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                FatherName = reader["FatherName"]?.ToString(),
                                MotherName = reader["MotherName"]?.ToString(),
                                UserName = reader["UserName"]?.ToString(),
                                Password = reader["NormalPassword"]?.ToString(),
                                ClassName = reader["ClassName"]?.ToString(),
                                SectionName = reader["SectionName"]?.ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
