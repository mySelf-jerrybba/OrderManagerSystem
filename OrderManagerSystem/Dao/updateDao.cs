using Microsoft.VisualBasic;
using MySqlConnector;
using OrderManagerSystem.Dto;
using OrderManagerSystem.Services;

namespace OrderManagerSystem.Dao
{
    public class updateDao
    {
        private readonly MySqlService _MySqlService;
        public updateDao(MySqlService MySqlService)
        {
            _MySqlService = MySqlService;
        }

        public async Task<string> updateTimebucketscheduled(addOrderDto dto,int workingHour, DateTime thisDate,string whoIsWorkingOverNight)
        {
            System.Console.WriteLine("開始執行insertIntoSchedules");
            //開啟 DB 連線
            using var connection = _MySqlService.GetConnection2();//GetConnection2連接scheduledproduceplan
            //這裡的await是指等連線開啟在執行查詢
            await connection.OpenAsync();

            //要新增的資料
            string orderId = dto.orderId.ToString();
            DateTime estimateStartDate = Convert.ToDateTime(dto.estimateStartDate);
            DateTime estimateEndDate = Convert.ToDateTime(dto.estimateEndDate);
            int estimateWorkingTime = Convert.ToInt32(dto.estimateWorkingTime);
            //workingHour=本日工時

            var sql = @"UPDATE scheduledproduceplan.timebucket
                            SET
                            usedTime = usedTime + @workingHours,
                            remainTime = remainTime - @workingHours,
                            whoIsWorkingOverNight=@whoIsWorkingOverNight
                            WHERE bucketDate = @thisDate;
                        ";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@orderId", orderId);
                command.Parameters.AddWithValue("@workingHours", workingHour);
                command.Parameters.AddWithValue("@whoIsWorkingOverNight", whoIsWorkingOverNight);
                command.Parameters.AddWithValue("@thisDate", thisDate);
                

            int rowsAffected = await command.ExecuteNonQueryAsync();
            System.Console.WriteLine("updateTimebucketscheduled執行結束");
            if (rowsAffected > 0)
            {
                System.Console.WriteLine("時間桶資訊更新成功");
                return "true";
            }
            else
            {
                System.Console.WriteLine("時間桶資訊更新失敗");
                return "時間桶資訊更新失敗";
            }

        }//end by updateTimebucketscheduled
    }
}
