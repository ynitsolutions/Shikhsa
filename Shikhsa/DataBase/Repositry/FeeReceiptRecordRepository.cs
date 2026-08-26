using Dapper;
using Microsoft.Data.SqlClient;
using Shikhsa.ViewModels;
using System.Data;

namespace Shikhsa.DataBase.Repositry
{
    public class FeeReceiptRecordRepository
    {
        private readonly IConfiguration _configuration;

        public FeeReceiptRecordRepository(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<FeeReceiptDataTableResponse>
            GetReceiptRecordsAsync(
                FeeReceiptDataTableRequest request)
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString(
                    "DefaultConnection"));

            var parameters = new DynamicParameters();

            parameters.Add("@DateFrom", request.DateFrom);
            parameters.Add("@DateTo", request.DateTo);
            parameters.Add("@Search", request.Search);
            parameters.Add("@ClassId", request.ClassId);
            parameters.Add("@BatchId", request.BatchId);
            parameters.Add("@PaymentModeId", request.PaymentModeId);
            parameters.Add("@Start", request.Start);
            parameters.Add("@Length", request.Length);

            parameters.Add(
                "@OrderBy",
                string.IsNullOrWhiteSpace(request.OrderColumn)
                    ? "ReceiptDate"
                    : request.OrderColumn);

            parameters.Add(
                "@OrderDirection",
                string.Equals(
                    request.OrderDirection,
                    "ASC",
                    StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC");


            using var multi =
                await connection.QueryMultipleAsync(
                    "Sp_GetFeeReceiptRecords",
                    parameters,
                    commandType: CommandType.StoredProcedure);


            var data =
                (await multi.ReadAsync<FeeReceiptRecordItemVM>())
                .ToList();


            var count =
                await multi.ReadFirstOrDefaultAsync<FeeReceiptCountVM>();


            return new FeeReceiptDataTableResponse
            {
                Draw = request.Draw,

                RecordsTotal =
                    count?.RecordsTotal ?? 0,

                RecordsFiltered =
                    count?.RecordsFiltered ?? 0,

                Data = data
            };
        }


        public async Task<List<FeeReceiptRecordItemVM>>
            GetReceiptRecordsForExportAsync(
                FeeReceiptDataTableRequest request)
        {
            request.Start = 0;
            request.Length = -1;

            var result =
                await GetReceiptRecordsAsync(request);

            return result.Data;
        }
    }


    public class FeeReceiptCountVM
    {
        public int RecordsTotal { get; set; }

        public int RecordsFiltered { get; set; }
    }
}