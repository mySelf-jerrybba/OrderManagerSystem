using MySqlConnector;
using OrderManagerSystem.Dto;
using OrderManagerSystem.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace OrderManagerSystem.Dao
{
    public class insertDao
    {
        private readonly MySqlService _MySqlService;

        public insertDao(MySqlService MySqlService)
        {
            _MySqlService = MySqlService;
        }

        public async Task<string> insertIntoOrder(addOrderDto dto) {
            //新增訂單明細
            System.Console.WriteLine("開始執行insertIntoSchedules");
            //開啟 DB 連線
            using var connection = _MySqlService.GetConnection1();//GetConnection1連接orderlist
            //這裡的await是指等連線開啟在執行查詢
            await connection.OpenAsync();

            //要新增的資料
            string orderId = dto.orderId.ToString();
            DateTime applyDate = Convert.ToDateTime(dto.applyDate);
            DateTime deadline = Convert.ToDateTime(dto.deadline);
            DateTime estimateStartDate = Convert.ToDateTime(dto.estimateStartDate);
            DateTime estimateEndDate = Convert.ToDateTime(dto.estimateEndDate);
            int amount = Convert.ToInt32(dto.amount);
            string location = dto.location.ToString();
            string mainReceiver = dto.mainReceiver.ToString();
            int estimateWorkingTime= Convert.ToInt32(dto.estimateWorkingTime);
            System.Console.WriteLine("以下為要新增的排程資料：");
            System.Console.WriteLine("訂單編號(orderId)=" + orderId);
            System.Console.WriteLine("申請日期(applyDate)=" + applyDate);
            System.Console.WriteLine("訂單截止日期(deadline)=" + deadline);
            System.Console.WriteLine("預估開始加工日期(estimateStartDate)=" + estimateStartDate);
            System.Console.WriteLine("預估結束加工日期(estimateEndDate)=" + estimateEndDate);
            System.Console.WriteLine("數量(amount)=" + amount);
            System.Console.WriteLine("交貨地址(location)=" + location);
            System.Console.WriteLine("收貨負責人(mainReceiver)=" + mainReceiver);
            System.Console.WriteLine("預估工時(estimateWorkingTime)=" + estimateWorkingTime);


            string sql = @"
                    insert into orderlist.`order`(
                        orderId,
                        applyDate,
                        deadline,
                        estimateStartDate,
                        estimateEndDate,
                        amount,
                        orderLocation,
                        receiver,
                        estimatedWorkingTime
                    )
                    values(
                    @orderId,
                    @applyDate, 
                    @deadline,
                    @estimateStartDate,
                    @estimateEndDate,
                    @amount,
                    @location,
                    @mainReceiver,
                    @estimateWorkingTime
                )";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.Parameters.AddWithValue("@applyDate", applyDate);
            command.Parameters.AddWithValue("@deadline", deadline);
            command.Parameters.AddWithValue("@estimateStartDate", estimateStartDate);
            command.Parameters.AddWithValue("@estimateEndDate", estimateEndDate);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@location", location);
            command.Parameters.AddWithValue("@mainReceiver", mainReceiver);
            command.Parameters.AddWithValue("@estimateWorkingTime", estimateWorkingTime);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            System.Console.WriteLine("insertIntoOrder執行結束");
            if (rowsAffected > 0)
            {
                System.Console.WriteLine("訂單新增成功");
                return "true";
            }
            else
            {
                System.Console.WriteLine("訂單新增失敗");
                return "訂單新增失敗";
            }
        }//end by insertIntoOrder

        public async Task<string> insertTimebucketscheduled(addOrderDto dto, int workingHour,DateTime thisDate,string isWorkingOverNight)
        {

            System.Console.WriteLine("開始執行insertTimebucketscheduled");
            //開啟 DB 連線
            using var connection = _MySqlService.GetConnection2();//GetConnection2連接scheduledproduceplan
            //這裡的await是指等連線開啟在執行查詢
            await connection.OpenAsync();

            //要新增的資料
            string orderId = dto.orderId.ToString();
            DateTime estimateStartDate = Convert.ToDateTime(dto.estimateStartDate);
            DateTime estimateEndDate = Convert.ToDateTime(dto.estimateEndDate);
            int estimateWorkingTime = Convert.ToInt32(dto.estimateWorkingTime);

            var sql = @"insert into scheduledproduceplan.`timebucketscheduled`(
                    orderId,
                    bucketDate,
                    workingHours,
                    isWorkingOverNight
                    )
                    values( 
                    @orderId,
                    @thisDate,
                    @workingHours,
                    @isWorkingOverNight
                    )
                ";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@orderId", orderId);
                command.Parameters.AddWithValue("@workingHours", workingHour);
                command.Parameters.AddWithValue("@thisDate", thisDate);
                command.Parameters.AddWithValue("@isWorkingOverNight", isWorkingOverNight);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                System.Console.WriteLine("insertTimebucketscheduled執行結束");
                if (rowsAffected > 0)
                {
                    System.Console.WriteLine("排程新增成功");
                    return "true";
                }
                else
                {
                    System.Console.WriteLine("排程新增失敗");
                    return "排程新增失敗";
                }


        }//end by insertTimebucketscheduled
    }
}
