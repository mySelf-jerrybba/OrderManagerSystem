//連接mySQL
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MySqlConnector;
using OrderManagerSystem.Dto;
using OrderManagerSystem.Services;
using System.Reflection.PortableExecutable;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OrderManagerSystem.Dao
{
    public class selectDao
    {
        //宣告一個資料庫服務變數
        //readonly指得是建立後不能被改掉
        private readonly MySqlService _MySqlService;

        public selectDao(MySqlService MySqlService)
        {
            _MySqlService = MySqlService;
        }

        public async Task<List<ScheduleDto>> setChart(string monthStartId, string monthEndId)
        {
            System.Console.WriteLine("開始執行setChart");
            //開啟 DB 連線
            using var connection = _MySqlService.GetConnection2();
            //這裡的await是指等連線開啟在執行查詢
            await connection.OpenAsync();

            //從月初查詢到月底，參數為date
            //用join的緣故是希望抓出沒有排成的日期
            string sql = @"SELECT
                b.bucketDate as `排成日期`,
                b.remainTime as `時間桶餘額`,
                b.workStation as `工站名稱`,
                IFNULL(s.orderId,'沒有訂單') as `訂單編號` ,
                IFNULL(s.workingHours,'沒有工時') as `預估工時`,
                IFNULL(s.isWorkingOverNight,'未規劃') as `是否跨日加工`
                FROM scheduledproduceplan.timebucketscheduled as s
                right join scheduledproduceplan.timebucket as b
                on b.bucketDate=s.bucketDate
                where b.bucketDate BETWEEN @monthStartId AND @monthEndId
                ORDER BY b.bucketDate,s.orderId ASC;";
            //建立 SQL Command
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@monthStartId", monthStartId);
            command.Parameters.AddWithValue("@monthEndId", monthEndId);
            //ExecuteReaderAsync() 回傳的是 Task
            using var reader = await command.ExecuteReaderAsync();

            //使用dto讓資料有鑑別度而不是一坨字串
            var result = new List<ScheduleDto>();

            while (await reader.ReadAsync())
            {
                var dto = new ScheduleDto
                {
                    bucketDate = reader["排成日期"].ToString(),
                    workStation = reader["工站名稱"].ToString(),
                    orderId = reader["訂單編號"].ToString(),
                    workingHours = reader["預估工時"].ToString(),
                    isWorkingOverNight = reader["是否跨日加工"].ToString(),
                    remainTime = Convert.ToInt32(reader["時間桶餘額"])
                };
                result.Add(dto);

            }
            System.Console.WriteLine("setChart執行結束");
            return result;
        }//end by setChart

        public async Task<List<timebuckDto>> getTimeBucketTime(DateTime startDate,DateTime endDate)
        {
            System.Console.WriteLine("取出"+ startDate+"~"+endDate+"內所有的時間桶餘額");
            System.Console.WriteLine("開始執行getTimeBucketTime");
            using var connection = _MySqlService.GetConnection2();//GetConnection2連接scheduledproduceplan
            await connection.OpenAsync();
            string sql = @"SELECT
                    bucketDate as `時間桶日期`,
                    remainTime as `時間桶餘額`,
                    usedTime as `已被使用的容量`,                      
                    whoIsWorkingOverNight as `哪筆訂單會在今天跨日加工到隔日` 
                    FROM scheduledproduceplan.timebucket 
                    where bucketDate between @startDate and @endDate
            ";
           
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            using var reader = await command.ExecuteReaderAsync();
            var result = new List<timebuckDto>();

            while (await reader.ReadAsync())
            {
                var dto = new timebuckDto {
                    bucketDate = Convert.ToDateTime(reader["時間桶日期"]),
                    remainTime =Convert.ToInt32(reader["時間桶餘額"]),
                    usedTime = Convert.ToInt32(reader["已被使用的容量"]),
                    whoIsWorkingOverNight = reader["哪筆訂單會在今天跨日加工到隔日"].ToString()
                };
                result.Add(dto);
            }
            System.Console.WriteLine("getTimeBucketTime執行結束");
            return result;
        }
    }//end by public class selectDao
}
    
