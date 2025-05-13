using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Config;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace asset_allocation_api.Service.Implementation
{
    public class DashboardService(AppDbContext dbContext)
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<Response<DashboardListResult?>> GetAllocationSummary(ILogger _logger, int departmentId)
        {
            Response<DashboardListResult?> resp = new();
            if (_dbContext.AssetAllocations == null)
            {
                ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, "AssetAllocations context is null");
                return resp;
            }
            DataTable dt = new();
            using (AppDbContext db = new())
            {
                using NpgsqlConnection con = (NpgsqlConnection)db.Database.GetDbConnection();
                using NpgsqlCommand cmd = new(@"SELECT * FROM usp_get_allocation_summary(@p_department_id, @p_shift_start_time, @p_shift_end_time, @p_ds_shift_name, @p_ns_shift_name)", con);
                
                // cmd.Parameters.Add(new NpgsqlParameter("@p_department_id", NpgsqlDbType.Integer) { Value = 1 });
                // cmd.Parameters.Add(new NpgsqlParameter("@p_shift_start_time", NpgsqlDbType.Integer) { Value = AssetAllocationConfig.assetAllocShiftStart });
                // cmd.Parameters.Add(new NpgsqlParameter("@p_shift_end_time", NpgsqlDbType.Integer) { Value = AssetAllocationConfig.assetAllocShiftEnd });
                // cmd.Parameters.Add(new NpgsqlParameter("@p_ds_shift_name", NpgsqlDbType.Text) { Value = AssetAllocationConfig.assetAllocShiftName1 });
                // cmd.Parameters.Add(new NpgsqlParameter("@p_ns_shift_name", NpgsqlDbType.Text) { Value = AssetAllocationConfig.assetAllocShiftName2 });
                
                cmd.Parameters.Add(new NpgsqlParameter("@p_department_id", NpgsqlDbType.Integer) { Value = 1 });
                cmd.Parameters.Add(new NpgsqlParameter("@p_shift_start_time", NpgsqlDbType.Integer) { Value = 4});
                cmd.Parameters.Add(new NpgsqlParameter("@p_shift_end_time", NpgsqlDbType.Integer) { Value = 16 });
                cmd.Parameters.Add(new NpgsqlParameter("@p_ds_shift_name", NpgsqlDbType.Varchar) { Value = "Morning" });
                cmd.Parameters.Add(new NpgsqlParameter("@p_ns_shift_name", NpgsqlDbType.Varchar) { Value = "Night" });
            
                await con.OpenAsync();
            
                using NpgsqlDataReader rdr = await cmd.ExecuteReaderAsync();
                dt.Load(rdr);
            }

            List<Dashboard> list = [];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Dashboard dashboard = new()
                {
                    Date = (DateTime)dt.Rows[i]["Date"],
                    Shift = (string)dt.Rows[i]["Shift"],
                    Name = (string)dt.Rows[i]["Name"],
                    Count = (long)dt.Rows[i]["Count"]
                };
                list.Add(dashboard);
            }
            
            // Dashboard dashboard = new()
            // {
            //     Date = DateTime.Now,
            //     Shift = "Morning",
            //     Name = "Test",
            //     Count = 1
            // };
            // list.Add(dashboard);

            ResponseUtils.ReturnResponse(_logger, null, resp, new DashboardListResult(list), 200, true, "Success"); ;
            return resp;
        }

        public async Task<Dashboard> ConvertAllocationToDashboard(AssetAllocation alloc, int num)
        {
            Dashboard dash = new()
            {
                Date = DateTime.SpecifyKind(alloc.AssignedDate!.Value.Date, DateTimeKind.Unspecified),
                Shift = alloc.AssignedDate!.Value.Hour >= AssetAllocationConfig.assetAllocShiftStart && alloc.AssignedDate!.Value.Hour < AssetAllocationConfig.assetAllocShiftEnd ? AssetAllocationConfig.assetAllocShiftName1 : AssetAllocationConfig.assetAllocShiftName2,
                Name = await (from a in _dbContext.Assets
                              join b in _dbContext.AssetTypes on a.AssetTypeId equals b.Id
                              where a.Id == alloc.AssetId
                              select b.Name).FirstOrDefaultAsync(),
                Count = num
            };

            return dash;
        }
    }
}
